using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using System;
using Avalonia;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Avalonia.Markup.Xaml;
using ConfigButtonDisplay.Core.Interfaces;
using ConfigButtonDisplay.Core.Services;
using ConfigButtonDisplay.Core.Configuration;
using ConfigButtonDisplay.Features.Debug.Controls;
using ConfigButtonDisplay.Infrastructure.Helpers;

namespace ConfigButtonDisplay;

public partial class MainWindow : Window
{
    private KeyboardHook? _keyboardHook;
    private bool _isListening = false;
    private KeyDisplayWindow? _keyDisplayWindow;
    private System.Threading.Timer? _hideTimer;
    private System.Threading.Timer? _hookHealthTimer;
    private DateTime _lastKeyEventTime = DateTime.MinValue;
    private string _lastImmediateText = "";
    private DateTime _lastImmediateAt = DateTime.MinValue;
    
    // 新功能组件
    private Features.TextSelection.Controls.TextSelectionPopover? _textSelectionPopover;
    private Features.EdgeComponents.Controls.SimpleEdgeComponent? _edgeSwipeComponent;  // 改为SimpleEdgeComponent
    private Features.AIChat.Controls.AIChatWindow? _aiChatWindow;
    private ConfigPopover? _configPopover;
    private DebugOverlay? _debugOverlay;  // 使用基类类型
    private Timer? _edgeAutoShowTimer; // 边缘组件自动显示计时器
    
    // 状态栏弹窗
    private PopupWindow? _popupWindow;
    
    // 创可贴标签组件
    private Features.NoteTags.Controls.NoteTagManager? _noteTagManager;
    
    // 当前按下的键
    private readonly HashSet<Key> _pressedKeys = new();
    private KeyModifiers _lastModifiers = KeyModifiers.None;
    
    // 核心服务
    private readonly IConfigurationService _configurationService;
    private readonly IWindowPositionService _windowPositionService;
    private AppSettings? _appSettings;
    
    
    public MainWindow(IConfigurationService configurationService, IWindowPositionService windowPositionService)
    {
        Console.WriteLine("MainWindow constructor starting...");
        
        // 使用依赖注入的服务
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        _windowPositionService = windowPositionService ?? throw new ArgumentNullException(nameof(windowPositionService));
        Console.WriteLine("Core services injected");
        
        AvaloniaXamlLoader.Load(this);
        Console.WriteLine("AvaloniaXamlLoader.Load completed");
        
        InitializeWindowDragBehavior(); // 添加窗口拖拽功能
        Console.WriteLine("InitializeWindowDragBehavior completed");
        InitializeSystemTray();
        Console.WriteLine("InitializeSystemTray completed");
        InitializeAdditionalFeatures();
        Console.WriteLine("InitializeAdditionalFeatures completed");
        InitializeDebugFeatures();
        Console.WriteLine("InitializeDebugFeatures completed");
        
        // 延迟初始化键盘钩子，确保控件已加载
        this.Loaded += OnWindowLoaded;
        this.Opened += OnWindowOpened;
        Console.WriteLine("MainWindow constructor completed");
    }

    private void OnWindowLoaded(object? sender, EventArgs e)
    {
        Console.WriteLine("MainWindow Loaded event triggered");
        
        // 使用延迟执行确保所有控件都已初始化
        Dispatcher.UIThread.Post(() =>
        {
            Console.WriteLine("Initializing keyboard hook in delayed task");
            InitializeKeyboardHook();
            UpdatePreview();
        }, DispatcherPriority.Render);
    }
    
    /// <summary>
    /// 窗口打开事件，应用窗口定位和配置
    /// </summary>
    private async void OnWindowOpened(object? sender, EventArgs e)
    {
        Console.WriteLine("MainWindow Opened event triggered");
        
        try
        {
            // 加载配置
            _appSettings = await _configurationService.LoadAsync();
            Console.WriteLine($"Configuration loaded: Position={_appSettings.Window.Position}");
            
            // 应用窗口定位
            if (_appSettings.Window.Position == "RightEdge")
            {
                var position = _windowPositionService.CalculateRightEdgePosition(
                    (int)this.Width, 
                    (int)this.Height
                );
                this.Position = position;
                Console.WriteLine($"Window positioned at right edge: X={position.X}, Y={position.Y}");
                
                // 执行从右侧滑入动画
                await Infrastructure.Helpers.AnimationHelper.SlideInFromRight(this, 300);
            }
            else if (_appSettings.Window.Position == "Custom" && _appSettings.Window.RememberPosition)
            {
                // 加载自定义位置
                if (_appSettings.Window.CustomX.HasValue && _appSettings.Window.CustomY.HasValue)
                {
                    var customPosition = new PixelPoint(
                        _appSettings.Window.CustomX.Value,
                        _appSettings.Window.CustomY.Value
                    );
                    
                    // 确保位置在屏幕范围内
                    var validPosition = Infrastructure.Helpers.ScreenHelper.EnsureOnScreen(
                        customPosition,
                        (int)this.Width,
                        (int)this.Height
                    );
                    
                    this.Position = validPosition;
                    Console.WriteLine($"Window positioned at custom location: X={validPosition.X}, Y={validPosition.Y}");
                }
            }
            else if (_appSettings.Window.RememberPosition)
            {
                // 尝试加载保存的位置
                var savedPosition = _windowPositionService.LoadSavedPosition();
                if (savedPosition.HasValue)
                {
                    this.Position = savedPosition.Value;
                    Console.WriteLine($"Window positioned at saved location: X={savedPosition.Value.X}, Y={savedPosition.Value.Y}");
                }
            }
            
            // 应用窗口透明度
            this.Opacity = _appSettings.Window.Opacity;
            
            // 应用置顶设置
            this.Topmost = _appSettings.Window.AlwaysOnTop;
            
            // 应用键盘监控配置到 KeyDisplayWindow
            if (_keyDisplayWindow != null && _appSettings.KeyboardMonitor != null)
            {
                _keyDisplayWindow.UpdateSettings(_appSettings.KeyboardMonitor);
            }
            
            // 启动配置文件监听
            _configurationService.StartWatching();
            
            // 订阅配置更改事件
            _configurationService.ConfigurationChanged += OnConfigurationChanged;
            
            Console.WriteLine("Window configuration applied successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying window configuration: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 配置更改事件处理
    /// </summary>
    private async void OnConfigurationChanged(object? sender, AppSettings newSettings)
    {
        try
        {
            Console.WriteLine("Configuration changed, reloading settings...");
            
            // 在 UI 线程上应用新配置
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _appSettings = newSettings;
                
                // 应用窗口透明度
                this.Opacity = newSettings.Window.Opacity;
                
                // 应用置顶设置
                this.Topmost = newSettings.Window.AlwaysOnTop;
                
                // 应用键盘监控配置
                if (_keyDisplayWindow != null && newSettings.KeyboardMonitor != null)
                {
                    _keyDisplayWindow.UpdateSettings(newSettings.KeyboardMonitor);
                }
                
                Console.WriteLine("Configuration reloaded successfully");
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reloading configuration: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 初始化窗口拖拽行为（无边框窗口需要）
    /// </summary>
    private void InitializeWindowDragBehavior()
    {
        // 为整个窗口添加拖拽功能
        this.PointerPressed += OnWindowPointerPressed;
        this.PointerMoved += OnWindowPointerMoved;
        this.PointerReleased += OnWindowPointerReleased;
    }
    
    private bool _isDragging = false;
    private Point _dragStartPoint;
    private System.Threading.Timer? _positionSaveTimer;
    
    private void OnWindowPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isDragging = true;
            _dragStartPoint = e.GetPosition(this);
            e.Pointer.Capture(this);
        }
    }
    
    private void OnWindowPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDragging)
        {
            var currentPoint = e.GetPosition(this);
            var deltaX = currentPoint.X - _dragStartPoint.X;
            var deltaY = currentPoint.Y - _dragStartPoint.Y;
            
            this.Position = new PixelPoint(
                this.Position.X + (int)deltaX,
                this.Position.Y + (int)deltaY
            );
        }
    }
    
    private void OnWindowPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            e.Pointer.Capture(null);
            
            // 使用防抖动机制保存位置（500ms 延迟）
            SchedulePositionSave();
        }
    }
    
    /// <summary>
    /// 安排位置保存（防抖动）
    /// </summary>
    private void SchedulePositionSave()
    {
        // 取消之前的计时器
        _positionSaveTimer?.Dispose();
        
        // 创建新的计时器，500ms 后保存位置
        _positionSaveTimer = new System.Threading.Timer(_ =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                SaveWindowPosition();
            });
        }, null, 500, Timeout.Infinite);
    }
    
    /// <summary>
    /// 保存窗口位置
    /// </summary>
    private async void SaveWindowPosition()
    {
        try
        {
            if (_appSettings == null || !_appSettings.Window.RememberPosition)
            {
                return;
            }
            
            // 保存到服务
            _windowPositionService.SavePosition(this);
            
            // 更新配置中的自定义位置
            _appSettings.Window.Position = "Custom";
            _appSettings.Window.CustomX = this.Position.X;
            _appSettings.Window.CustomY = this.Position.Y;
            
            // 保存配置到文件
            await _configurationService.SaveAsync(_appSettings);
            
            Console.WriteLine($"Window position saved: X={this.Position.X}, Y={this.Position.Y}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving window position: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 最小化按钮点击事件
    /// </summary>
    private void MinimizeButton_Click(object? sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }
    
    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
    
    private void InitializeDebugFeatures()
    {
        // 添加主窗口的键盘事件监听（用于双击Shift检测）
        this.AddHandler(KeyDownEvent, OnMainWindowKeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);
    }
    
    private DateTime _lastShiftPressTime = DateTime.MinValue;
    private bool _isFirstShiftPressed = false;
    private const int DOUBLE_CLICK_INTERVAL = 300; // 毫秒
    private int _totalShiftDoubleClicks = 0; // 用于统计的双击次数
    
    private void OnMainWindowKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
        {
            var now = DateTime.Now;
            var timeSinceLastPress = (now - _lastShiftPressTime).TotalMilliseconds;
            
            // 添加按键反馈
            _debugOverlay?.LogEvent($"Shift按键: {e.Key} (距离上次: {timeSinceLastPress:F0}ms)");
            
            if (_isFirstShiftPressed && timeSinceLastPress < DOUBLE_CLICK_INTERVAL)
            {
                // 双击Shift键检测成功
                _isFirstShiftPressed = false;
                _lastShiftPressTime = DateTime.MinValue;
                _totalShiftDoubleClicks++;
                
                _debugOverlay?.LogEvent($"🎉 双击Shift检测成功！ (总计: {_totalShiftDoubleClicks}次)");
                this.Get<TextBlock>("StatusTextBlock").Text = $"双击Shift检测成功！ (总计: {_totalShiftDoubleClicks}次)";
                
                // 增强调试信息
                if (_debugOverlay is EnhancedDebugOverlay enhanced)
                {
                    enhanced.LogShiftPress(e.Key, timeSinceLastPress);
                }
                
                // 切换AI聊天窗口状态
                if (_aiChatWindow != null)
                {
                    try
                    {
                        if (_aiChatWindow.IsVisible)
                        {
                            // 如果窗口已打开，则关闭
                            _aiChatWindow.HideChatWindow();
                            _debugOverlay?.LogEvent($"✅ AI聊天窗口已关闭");
                            this.Get<TextBlock>("StatusTextBlock").Text = $"AI聊天窗口已关闭";
                        }
                        else
                        {
                            // 如果窗口已关闭，则打开
                            _aiChatWindow.ShowChatWindow();
                            _debugOverlay?.LogEvent($"✅ AI聊天窗口已打开");
                            this.Get<TextBlock>("StatusTextBlock").Text = $"AI聊天窗口已打开";
                        }
                    }
                    catch (Exception ex)
                    {
                        _debugOverlay?.LogEvent($"❌ AI聊天窗口错误: {ex.Message}");
                        this.Get<TextBlock>("StatusTextBlock").Text = $"AI聊天窗口错误: {ex.Message}";
                    }
                }
                else
                {
                    _debugOverlay?.LogEvent("❌ AI聊天窗口未初始化");
                    this.Get<TextBlock>("StatusTextBlock").Text = "AI聊天窗口未初始化";
                }
            }
            else
            {
                _isFirstShiftPressed = true;
                _lastShiftPressTime = now;
                _debugOverlay?.LogEvent($"第一次Shift按下 (将在{DOUBLE_CLICK_INTERVAL}ms内等待第二次)");
                
                // 设置超时
                Task.Delay(DOUBLE_CLICK_INTERVAL).ContinueWith(_ =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (_isFirstShiftPressed)
                        {
                            _isFirstShiftPressed = false;
                            _debugOverlay?.LogEvent("⏰ 双击Shift超时，重置状态");
                        }
                    });
                });
            }
        }
    }
    
    private void InitializeKeyboardHook()
    {
        try
        {
            // 添加空值检查
            try
            {
                var statusTextBlock = this.Get<TextBlock>("StatusTextBlock");
                if (statusTextBlock == null)
                {
                    Console.WriteLine("StatusTextBlock is null - delaying keyboard hook initialization");
                    return;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("StatusTextBlock not found - delaying keyboard hook initialization");
                return;
            }
            
            _keyboardHook = new KeyboardHook();
            _keyboardHook.KeyDown += OnKeyDown;
            _keyboardHook.KeyUp += OnKeyUp;
            
            // 程序启动后立即开始监听
            _isListening = true;
            if (ListeningToggle != null)
                ListeningToggle.IsChecked = true;
            this.Get<TextBlock>("StatusText").Text = "监听中...";
            this.Get<TextBlock>("StatusTextBlock").Text = "键盘钩子已初始化，开始监听按键";

            // 启动健康检测：每5秒校正修饰键并清理卡死状态
            _hookHealthTimer = new System.Threading.Timer(HookHealthCheck, null, 5000, 5000);
        }
        catch (Exception ex)
        {
            try { this.Get<TextBlock>("StatusTextBlock").Text = $"键盘钩子初始化失败: {ex.Message}"; } 
            catch { Console.WriteLine($"键盘钩子初始化失败: {ex.Message}"); }
        }
    }
    
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!_isListening) return;
        
        Dispatcher.UIThread.Post(() =>
        {
            _pressedKeys.Add(e.Key);
            _lastModifiers = e.KeyModifiers;
            _lastKeyEventTime = DateTime.Now;

            // 立即显示当前按键，避免快速单击被异步清空导致不显示
            var immediateText = FormatImmediateKey(e.Key, _keyboardHook?.GetCurrentModifiers() ?? e.KeyModifiers);
            if (!string.IsNullOrEmpty(immediateText))
            {
                ShowKeyDisplay(immediateText);
                _keyDisplayWindow?.RefreshDisplay(); // 重置自动隐藏
                _lastImmediateText = immediateText;
                _lastImmediateAt = DateTime.Now;
            }

            UpdateKeyDisplay();
        });
    }
    
    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (!_isListening) return;
        
        Dispatcher.UIThread.Post(() =>
        {
            _pressedKeys.Remove(e.Key);
            _lastModifiers = e.KeyModifiers;
            _lastKeyEventTime = DateTime.Now;
            UpdateKeyDisplay();
        });
    }
    
    private void UpdateKeyDisplay()
    {
        // 每次刷新前，从键盘钩子拉取最新修饰键，避免长时间后事件修饰键丢失
        _lastModifiers = _keyboardHook?.GetCurrentModifiers() ?? _lastModifiers;

        if (_pressedKeys.Count == 0)
        {
            // 若刚刚发生过即时显示（例如快速单击 D/F），在200ms内保持显示，不立即隐藏
            if ((DateTime.Now - _lastImmediateAt).TotalMilliseconds < 200 && !string.IsNullOrEmpty(_lastImmediateText))
            {
                ShowKeyDisplay(_lastImmediateText);
                _keyDisplayWindow?.RefreshDisplay();
                return;
            }
            HideKeyDisplay();
            return;
        }
        
        // 使用快照（合并轮询结果），提升组合键稳定性
        var snapshot = BuildCurrentKeysSnapshot();
        var keyText = FormatKeyCombination(snapshot);
        Console.WriteLine($"[MainWindow] Computed keyText='{keyText}' from pressed={string.Join(",", _pressedKeys)} mods={_lastModifiers}");
        if (string.IsNullOrEmpty(keyText))
        {
            HideKeyDisplay();
            return;
        }
        
        ShowKeyDisplay(keyText);
        
        // 刷新显示计时器（重新触发2秒自动隐藏）
        _keyDisplayWindow?.RefreshDisplay();
        
        // 立即更新显示，不延迟隐藏
        CancelHideTimer();
    }
    
    private string FormatKeyCombination(HashSet<Key> keys)
    {
        var parts = new List<string>();

        // 只显示有实际按键的组合，避免只显示修饰键
        var normalKeys = keys.Where(k => !IsModifierKey(k)).ToList();
        if (normalKeys.Count == 0)
            return "";

        // 应用按键过滤
        var filteredKeys = ApplyKeyFilter(normalKeys);
        if (filteredKeys.Count == 0)
            return "";

        // 使用事件修饰键状态，确保组合键可靠显示
        // 根据配置决定是否显示修饰键
        var showModifiers = _appSettings?.KeyboardMonitor?.ShowModifiers ?? true;
        if (showModifiers)
        {
            if ((_lastModifiers & KeyModifiers.Control) != 0)
                parts.Add("Ctrl");
            if ((_lastModifiers & KeyModifiers.Alt) != 0)
                parts.Add("Alt");
            if ((_lastModifiers & KeyModifiers.Shift) != 0)
                parts.Add("Shift");
            if ((_lastModifiers & KeyModifiers.Meta) != 0)
                parts.Add("Win");
        }

        // 添加过滤后的普通按键
        foreach (var key in filteredKeys)
            parts.Add(FormatKeyName(key));

        return string.Join(" + ", parts);
    }
    
    /// <summary>
    /// 应用按键过滤规则
    /// </summary>
    private List<Key> ApplyKeyFilter(List<Key> keys)
    {
        if (_appSettings?.KeyboardMonitor == null)
        {
            return keys;
        }
        
        var settings = _appSettings.KeyboardMonitor;
        var filtered = new List<Key>();
        
        foreach (var key in keys)
        {
            // 功能键过滤 (F1-F12)
            if (key >= Key.F1 && key <= Key.F12)
            {
                if (settings.ShowFunctionKeys)
                {
                    filtered.Add(key);
                }
                continue;
            }
            
            // 导航键过滤
            if (IsNavigationKey(key))
            {
                if (settings.ShowNavigation)
                {
                    filtered.Add(key);
                }
                continue;
            }
            
            // 字母数字键过滤
            if (IsAlphaNumericKey(key))
            {
                if (settings.ShowAlphaNumeric)
                {
                    filtered.Add(key);
                }
                continue;
            }
            
            // 其他按键默认显示
            filtered.Add(key);
        }
        
        return filtered;
    }
    
    /// <summary>
    /// 判断是否为导航键
    /// </summary>
    private bool IsNavigationKey(Key key)
    {
        return key is Key.Up or Key.Down or Key.Left or Key.Right or
               Key.Home or Key.End or Key.PageUp or Key.PageDown or
               Key.Insert or Key.Delete;
    }
    
    /// <summary>
    /// 判断是否为字母数字键
    /// </summary>
    private bool IsAlphaNumericKey(Key key)
    {
        return (key >= Key.A && key <= Key.Z) || (key >= Key.D0 && key <= Key.D9);
    }

    // 立即显示当前按下的键（用于快速单击的可见性保障）
    private string FormatImmediateKey(Key key, KeyModifiers mods)
    {
        var parts = new List<string>();
        if ((mods & KeyModifiers.Control) != 0) parts.Add("Ctrl");
        if ((mods & KeyModifiers.Alt) != 0) parts.Add("Alt");
        if ((mods & KeyModifiers.Shift) != 0) parts.Add("Shift");
        if ((mods & KeyModifiers.Meta) != 0) parts.Add("Win");
        parts.Add(FormatKeyName(key));
        return string.Join(" + ", parts);
    }

    // 组合键快照：合并 _pressedKeys 与 KeyboardHook 的当前按键检测
    private HashSet<Key> BuildCurrentKeysSnapshot()
    {
        // 快修：仅使用事件捕获的按键集，避免错误键码映射引入的“幽灵键”
        return new HashSet<Key>(_pressedKeys);
    }
    
    private bool IsModifierKey(Key key)
    {
        return key is Key.LeftCtrl or Key.RightCtrl or
               Key.LeftAlt or Key.RightAlt or
               Key.LeftShift or Key.RightShift or
               Key.LWin or Key.RWin;
    }
    
    private string FormatKeyName(Key key)
    {
        return key switch
        {
            Key.Space => "Space",
            Key.Enter or Key.Return => "Enter",
            Key.Escape => "Escape",
            Key.Tab => "Tab",
            Key.Back => "⌫",
            Key.Delete => "Del",
            Key.Insert => "Ins",
            Key.Home => "Home",
            Key.End => "End",
            Key.PageUp => "PgUp",
            Key.PageDown => "PgDn",
            Key.Up => "↑",
            Key.Down => "↓",
            Key.Left => "←",
            Key.Right => "→",
            Key.OemComma => ",",
            Key.OemPeriod => ".",
            Key.OemQuestion => "/",
            Key.OemSemicolon => ";",
            Key.OemQuotes => "\"",
            Key.OemTilde => "`",
            Key.OemOpenBrackets => "[",
            Key.OemCloseBrackets => "]",
            Key.OemPipe => "\\",
            Key.OemPlus => "+",
            Key.OemMinus => "-",
            >= Key.F1 and <= Key.F12 => key.ToString(),
            >= Key.D0 and <= Key.D9 => key.ToString().Substring(1), // 移除D前缀
            >= Key.A and <= Key.Z => key.ToString().ToUpper(),
            _ => key.ToString()
        };
    }
    
    private void ShowKeyDisplay(string keyText)
    {
        CancelHideTimer();
        
        if (_keyDisplayWindow == null)
        {
            _keyDisplayWindow = new KeyDisplayWindow();
        }
        
        // 更新内容（窗口会自动定位到底部居中并调整宽度）
        _keyDisplayWindow.UpdateContent(keyText, GetSelectedColor(), GetFontSize());
        
        // 更新预览
        if (PreviewText != null)
            PreviewText.Text = keyText;
        try { this.Get<TextBlock>("StatusTextBlock").Text = $"显示按键: {keyText}"; } catch { }
    }
    
    private void HideKeyDisplay()
    {
        // 清空显示内容，窗口会自动隐藏
        if (_keyDisplayWindow != null)
        {
            _keyDisplayWindow.UpdateContent("", GetSelectedColor(), GetFontSize());
        }
        try { this.Get<TextBlock>("StatusTextBlock").Text = "监听中..."; } catch { }
    }
    
    private void StartHideTimer()
    {
        CancelHideTimer();
        var duration = (int)(GetDisplayDuration() * 1000);
        _hideTimer = new System.Threading.Timer(_ =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                HideKeyDisplay();
            });
        }, null, duration, Timeout.Infinite);
    }
    
    private void CancelHideTimer()
    {
        _hideTimer?.Dispose();
        _hideTimer = null;
    }
    
    private void ListeningToggle_Toggled(object? sender, RoutedEventArgs e)
    {
        _isListening = ListeningToggle?.IsChecked ?? false;
        
        if (_isListening)
        {
            try { this.Get<TextBlock>("StatusText").Text = "监听中"; } catch { }
            try { this.Get<TextBlock>("StatusTextBlock").Text = "监听中..."; } catch { }
            _pressedKeys.Clear();
            HideKeyDisplay();
        }
        else
        {
            try { this.Get<TextBlock>("StatusText").Text = "已停止"; } catch { }
            try { this.Get<TextBlock>("StatusTextBlock").Text = "监听已停止"; } catch { }
            _pressedKeys.Clear();
            HideKeyDisplay();
            CancelHideTimer();
        }
    }
    
    private void TestButton_Click(object? sender, RoutedEventArgs e)
    {
        ShowKeyDisplay("Ctrl + Shift + A");
        _keyDisplayWindow?.EnsureShowVisible();
    }
    
    private void HideButton_Click(object? sender, RoutedEventArgs e)
    {
        HideToBackground();
    }
    
    private async void ConfigButton_Click(object? sender, RoutedEventArgs e)
    {
        // 打开配置窗口
        try
        {
            // 从依赖注入容器获取 ConfigWindow 和 ConfigViewModel
            var configWindow = App.Current?.GetService<Views.ConfigWindow>();
            var viewModel = App.Current?.GetService<ViewModels.ConfigViewModel>();
            
            if (configWindow == null || viewModel == null)
            {
                throw new InvalidOperationException("无法从依赖注入容器获取 ConfigWindow 或 ConfigViewModel");
            }
            
            // 加载配置
            await viewModel.LoadAsync();
            configWindow.DataContext = viewModel;
            
            // 以模态方式显示
            await configWindow.ShowDialog(this);
            
            // 配置窗口关闭后，重新加载配置
            _appSettings = await _configurationService.LoadAsync();
            Console.WriteLine("Configuration reloaded after config window closed");
            
            // 应用新配置到 KeyDisplayWindow
            if (_keyDisplayWindow != null && _appSettings.KeyboardMonitor != null)
            {
                _keyDisplayWindow.UpdateSettings(_appSettings.KeyboardMonitor);
            }
            
            this.Get<TextBlock>("StatusTextBlock").Text = "配置已更新";
            await NotificationHelper.ShowInfo(this, "配置已重新加载并应用", "配置更新");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error opening config window: {ex.Message}");
            this.Get<TextBlock>("StatusTextBlock").Text = $"打开配置窗口失败: {ex.Message}";
            await NotificationHelper.ShowError(this, $"打开配置窗口失败：{ex.Message}", "错误");
        }
    }
    
    /// <summary>
    /// 显示状态栏弹窗菜单
    /// </summary>
    private async void ShowStatusBarPopup(Button? anchorButton)
    {
        try
        {
            if (_popupWindow == null)
            {
                _popupWindow = new PopupWindow();
            }
            
            // 显示弹窗，传入锚点按钮用于定位
            await _popupWindow.ShowPopupAsync(anchorButton);
            
            // 更新状态
            if (this.Get<TextBlock>("StatusTextBlock") != null)
                this.Get<TextBlock>("StatusTextBlock").Text = "状态栏菜单已打开";
            
            _debugOverlay?.LogEvent("✅ 状态栏弹窗菜单已显示");
        }
        catch (Exception ex)
        {
            if (this.Get<TextBlock>("StatusTextBlock") != null)
                this.Get<TextBlock>("StatusTextBlock").Text = $"弹窗显示失败: {ex.Message}";
            
            _debugOverlay?.LogEvent($"❌ 状态栏弹窗显示失败: {ex.Message}");
        }
    }
    
    private void ManualTestButton_Click(object? sender, RoutedEventArgs e)
    {
        // 手动测试所有功能
        _debugOverlay?.LogEvent("🧪 开始手动测试所有功能...");
        
        // 测试1: 触发文本选择
        if (_textSelectionPopover != null)
        {
            _debugOverlay?.LogEvent("📋 手动触发文本选择测试...");
            // 直接调用测试方法
            _textSelectionPopover.TriggerTest("手动测试文本");
        }
        
        // 测试2: 显示AI聊天窗口
        if (_aiChatWindow != null)
        {
            _debugOverlay?.LogEvent("💬 手动显示AI聊天窗口...");
            if (!_aiChatWindow.IsVisible)
            {
                _aiChatWindow.Show();
                // 不主动抢占焦点
            }
        }
        
        // 测试3: 触发边缘组件
        if (_edgeSwipeComponent != null)
        {
            if (_debugOverlay is EnhancedDebugOverlay enhanced)
            {
                enhanced.LogEvent("🎯 手动触发边缘组件...");
            }
            // 强制显示边缘组件
            _edgeSwipeComponent.ShowEdgeWindow();
        }
        
        this.Get<TextBlock>("StatusTextBlock").Text = "手动测试已执行，查看调试面板获取详细信息";
    }

    private void TagTestButton_Click(object? sender, RoutedEventArgs e)
    {
        // 测试标签功能
        if (_noteTagManager != null)
        {
            _debugOverlay?.LogEvent("🏷️ 测试标签显示功能...");
            
            try
            {
                // 获取当前状态
                var status = _noteTagManager.GetTagStatus();
                _debugOverlay?.LogEvent($"📊 {status}");
                
                // 强制显示标签
                _noteTagManager.ForceShowTags();
                _debugOverlay?.LogEvent("✅ 标签已强制显示");
                this.Get<TextBlock>("StatusTextBlock").Text = "标签已强制显示 - 查看屏幕左侧 (x=50, y=480)";
                
                // 5秒后显示状态
                Task.Delay(5000).ContinueWith(_ =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        var newStatus = _noteTagManager.GetTagStatus();
                        _debugOverlay?.LogEvent($"📊 5秒后状态: {newStatus}");
                        this.Get<TextBlock>("StatusTextBlock").Text = $"标签状态已更新: {newStatus}";
                    });
                });
            }
            catch (Exception ex)
            {
                _debugOverlay?.LogEvent($"❌ 标签测试失败: {ex.Message}");
                this.Get<TextBlock>("StatusTextBlock").Text = $"标签测试失败: {ex.Message}";
            }
        }
        else
        {
            _debugOverlay?.LogEvent("❌ 标签管理器未初始化");
            this.Get<TextBlock>("StatusTextBlock").Text = "标签管理器未初始化";
        }
    }
    
    private void BackgroundColorCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdatePreview();
    }
    
    private void DisplayDurationSlider_ValueChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        // 可以在这里添加实时更新逻辑
    }
    
    private void FontSizeSlider_ValueChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        UpdatePreview();
    }
    
    private void UpdatePreview()
    {
        if (PreviewText != null && PreviewBorder != null)
        {
            PreviewText.FontSize = GetFontSize();
            PreviewBorder.Background = new SolidColorBrush(GetSelectedColor());
        }
    }
    
    private Color GetSelectedColor()
    {
        var selectedIndex = BackgroundColorCombo?.SelectedIndex ?? 0;
        return selectedIndex switch
        {
            0 => Color.Parse("#3182CE"), // 蓝色 - Fluent Design
            1 => Color.Parse("#38A169"), // 绿色 - Fluent Design
            2 => Color.Parse("#E53E3E"), // 红色 - Fluent Design
            3 => Color.Parse("#805AD5"), // 紫色 - Fluent Design
            4 => Color.Parse("#DD6B20"), // 橙色 - Fluent Design
            _ => Color.Parse("#3182CE")
        };
    }
    
    private double GetDisplayDuration()
    {
        return DisplayDurationSlider?.Value ?? 2.0;
    }
    
    private double GetFontSize()
    {
        return FontSizeSlider?.Value ?? 28.0; // 默认28px，匹配Fluent Design
    }
    
    private void InitializeSystemTray()
    {
        // 简化实现：支持最小化到任务栏并隐藏窗口
        // 键盘监听将在后台继续运行
    }
    
    private void InitializeAdditionalFeatures()
    {
        try
        {
            // 初始化文本选择弹出框
            _textSelectionPopover = new Features.TextSelection.Controls.TextSelectionPopover(_debugOverlay);
            _textSelectionPopover.CopyRequested += OnTextCopyRequested;
            _textSelectionPopover.TranslateRequested += OnTextTranslateRequested;
            
            // 初始化边缘滑动组件
            _edgeSwipeComponent = new Features.EdgeComponents.Controls.SimpleEdgeComponent(_debugOverlay);
            _edgeSwipeComponent.WindowOpened += OnEdgeWindowOpened;
            _edgeSwipeComponent.WindowClosed += OnEdgeWindowClosed;
            
            // 启动边缘组件自动显示（可选）
            StartEdgeAutoShow();
            
            // 初始化AI聊天窗口
            _aiChatWindow = new Features.AIChat.Controls.AIChatWindow();
            
            // 初始化配置弹出框
            _configPopover = new ConfigPopover(this);
            _configPopover.AutoStartChanged += OnAutoStartConfigChanged;
            _configPopover.MinimizeToTrayChanged += OnMinimizeToTrayConfigChanged;
            _configPopover.ShowNotificationsChanged += OnShowNotificationsConfigChanged;
            
            // 初始化调试覆盖层
            _debugOverlay = new EnhancedDebugOverlay();
            _debugOverlay.ShowDebug();
            
            // 初始化状态栏弹窗
            _popupWindow = new PopupWindow();
            _debugOverlay?.LogEvent("✅ 状态栏弹窗已初始化");
            
            // 初始化创可贴标签组件
            System.Console.WriteLine($"[MainWindow] 开始初始化标签管理器...");
            _noteTagManager = new Features.NoteTags.Controls.NoteTagManager(this);
            System.Console.WriteLine($"[MainWindow] 标签管理器创建完成，设置文本...");
            _noteTagManager.SetTagText(0, "功能标签 1");
            _noteTagManager.SetTagText(1, "功能标签 2");
            _noteTagManager.SetTagText(2, "功能标签 3");
            System.Console.WriteLine($"[MainWindow] 文本设置完成，调用ShowTags...");
            _noteTagManager.ShowTags();
            System.Console.WriteLine($"[MainWindow] ShowTags调用完成");
            
            // 延迟500ms后验证便签初始状态
            Task.Delay(500).ContinueWith(_ =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    System.Console.WriteLine($"[MainWindow] 验证便签初始状态...");
                    _noteTagManager?.ValidateInitialState();
                    var status = _noteTagManager?.GetTagStatus();
                    if (status != null)
                    {
                        System.Console.WriteLine($"[MainWindow] 便签状态:\n{status}");
                    }
                });
            });
            
            if (this.Get<TextBlock>("StatusTextBlock") != null)
                this.Get<TextBlock>("StatusTextBlock").Text = "所有功能已初始化 (Fluent Design)";
        }
        catch (Exception ex)
        {
            if (this.Get<TextBlock>("StatusTextBlock") != null)
                this.Get<TextBlock>("StatusTextBlock").Text = $"功能初始化失败: {ex.Message}";
        }
    }
    
    private void OnTextCopyRequested(object? sender, string text)
    {
        if (this.Get<TextBlock>("StatusTextBlock") != null)
            this.Get<TextBlock>("StatusTextBlock").Text = $"已复制文本: {text.Substring(0, Math.Min(text.Length, 20))}...";
        
        if (_debugOverlay is EnhancedDebugOverlay enhanced)
        {
            enhanced.LogTextCopy(text);
        }
    }
    
    private void OnTextTranslateRequested(object? sender, string text)
    {
        if (this.Get<TextBlock>("StatusTextBlock") != null)
            this.Get<TextBlock>("StatusTextBlock").Text = $"翻译请求: {text.Substring(0, Math.Min(text.Length, 20))}...";
        
        if (_debugOverlay is EnhancedDebugOverlay enhanced)
        {
            enhanced.LogEvent($"翻译请求: {text.Substring(0, Math.Min(text.Length, 10))}...");
        }
        // 这里可以集成翻译API
    }
    
    private void OnEdgeWindowOpened(object? sender, EventArgs e)
    {
        if (this.Get<TextBlock>("StatusTextBlock") != null)
            this.Get<TextBlock>("StatusTextBlock").Text = "边缘工具栏已打开";
        
        if (_debugOverlay is EnhancedDebugOverlay enhanced)
        {
            enhanced.LogEdgeWindowShown();
        }
    }
    
    private void OnEdgeWindowClosed(object? sender, EventArgs e)
    {
        if (this.Get<TextBlock>("StatusTextBlock") != null)
            this.Get<TextBlock>("StatusTextBlock").Text = "边缘工具栏已关闭";
        
        if (_debugOverlay is EnhancedDebugOverlay enhanced)
        {
            enhanced.LogEdgeWindowHidden();
        }
    }
    
    private void OnAutoStartConfigChanged(object? sender, bool enabled)
    {
        if (this.Get<TextBlock>("StatusTextBlock") != null)
            this.Get<TextBlock>("StatusTextBlock").Text = $"开机自启: {(enabled ? "已启用" : "已禁用")}";
        _debugOverlay?.LogEvent($"开机自启: {(enabled ? "已启用" : "已禁用")}");
    }
    
    private void OnMinimizeToTrayConfigChanged(object? sender, bool enabled)
    {
        if (this.Get<TextBlock>("StatusTextBlock") != null)
            this.Get<TextBlock>("StatusTextBlock").Text = $"最小化到托盘: {(enabled ? "已启用" : "已禁用")}";
        _debugOverlay?.LogEvent($"最小化到托盘: {(enabled ? "已启用" : "已禁用")}");
    }
    
    private void OnShowNotificationsConfigChanged(object? sender, bool enabled)
    {
        if (this.Get<TextBlock>("StatusTextBlock") != null)
            this.Get<TextBlock>("StatusTextBlock").Text = $"显示通知: {(enabled ? "已启用" : "已禁用")}";
        _debugOverlay?.LogEvent($"显示通知: {(enabled ? "已启用" : "已禁用")}");
    }
    
    private void StartEdgeAutoShow()
    {
        // 延迟30秒后开始，每30秒一次 - 减少频率避免干扰
        _edgeAutoShowTimer = new Timer(AutoShowEdgeComponent, null, 30000, 30000);
        
        if (_debugOverlay is EnhancedDebugOverlay enhanced)
        {
            enhanced.LogEvent("⏰ 边缘组件自动显示已启动（30秒后开始，每30秒一次）");
        }
    }
    
    private void AutoShowEdgeComponent(object? state)
    {
        if (_edgeSwipeComponent != null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (_debugOverlay is EnhancedDebugOverlay enhanced)
                {
                    enhanced.LogEvent("⏰ 自动触发边缘组件显示");
                }
                _edgeSwipeComponent.TriggerAutoShow();
            });
        }
    }
    
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        
        // 清理键盘钩子
        _keyboardHook?.Dispose();
        _keyDisplayWindow?.Close();
        CancelHideTimer();
        _hookHealthTimer?.Dispose();
        
        // 清理新功能组件
        _textSelectionPopover?.Dispose();
        _edgeSwipeComponent?.Dispose();
        _aiChatWindow?.Close();
        _configPopover?.Dispose();
        _edgeAutoShowTimer?.Dispose();
        
        // 清理状态栏弹窗
        _popupWindow?.Close();
        
        // 清理标签组件
        _noteTagManager?.Dispose();
    }
    
    // 添加隐藏窗口但保持监听的功能
    public void HideToBackground()
    {
        Hide();
        // 键盘监听会继续在后台运行
    }
    
    
    public void ShowFromBackground()
    {
        Show();
        // 不主动抢占焦点
    }

    // 健康检测：若长时间无事件，重置卡死状态并校正修饰键
    private void HookHealthCheck(object? state)
    {
        var idle = (DateTime.Now - _lastKeyEventTime).TotalSeconds;
        if (idle > 5)
        {
            // 清理可能卡住的按键状态
            _keyboardHook?.ClearAllKeyStates();
            // 再次拉取修饰键，避免组合键显示缺失
            _lastModifiers = _keyboardHook?.GetCurrentModifiers() ?? KeyModifiers.None;
            try { this.Get<TextBlock>("StatusTextBlock").Text = "键盘状态已校正"; } catch { }
        }
    }
    
    /// <summary>
    /// 窗口关闭时清理资源
    /// </summary>
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        try
        {
            // 取消订阅配置更改事件
            _configurationService.ConfigurationChanged -= OnConfigurationChanged;
            
            // 停止配置文件监听
            _configurationService.StopWatching();
            
            Console.WriteLine("Configuration watching stopped");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during cleanup: {ex.Message}");
        }
        
        base.OnClosing(e);
    }
}
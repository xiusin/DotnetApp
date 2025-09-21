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

namespace ConfigButtonDisplay;

public partial class MainWindow : Window
{
    private KeyboardHook? _keyboardHook;
    private bool _isListening = false;
    private KeyDisplayWindow? _keyDisplayWindow;
    private System.Threading.Timer? _hideTimer;
    
    // 新功能组件
    private TextSelectionPopover? _textSelectionPopover;
    private SimpleEdgeComponent? _edgeSwipeComponent;  // 改为SimpleEdgeComponent
    private AIChatWindow? _aiChatWindow;
    private ConfigPopover? _configPopover;
    private DebugOverlay? _debugOverlay;  // 使用基类类型
    private Timer? _edgeAutoShowTimer; // 边缘组件自动显示计时器
    
    // 创可贴标签组件
    private NoteTagComponent.NoteTagManager? _noteTagManager;
    
    // 当前按下的键
    private readonly HashSet<Key> _pressedKeys = new();
    
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        InitializeComponent();
        InitializeKeyboardHook();
        InitializeSystemTray();
        InitializeAdditionalFeatures();
        InitializeDebugFeatures();
        UpdatePreview();
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
                StatusTextBlock.Text = $"双击Shift检测成功！ (总计: {_totalShiftDoubleClicks}次)";
                
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
                            StatusTextBlock.Text = $"AI聊天窗口已关闭";
                        }
                        else
                        {
                            // 如果窗口已关闭，则打开
                            _aiChatWindow.ShowChatWindow();
                            _debugOverlay?.LogEvent($"✅ AI聊天窗口已打开");
                            StatusTextBlock.Text = $"AI聊天窗口已打开";
                        }
                    }
                    catch (Exception ex)
                    {
                        _debugOverlay?.LogEvent($"❌ AI聊天窗口错误: {ex.Message}");
                        StatusTextBlock.Text = $"AI聊天窗口错误: {ex.Message}";
                    }
                }
                else
                {
                    _debugOverlay?.LogEvent("❌ AI聊天窗口未初始化");
                    StatusTextBlock.Text = "AI聊天窗口未初始化";
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
            _keyboardHook = new KeyboardHook();
            _keyboardHook.KeyDown += OnKeyDown;
            _keyboardHook.KeyUp += OnKeyUp;
            
            // 程序启动后立即开始监听
            _isListening = true;
            if (ListeningToggle != null)
                ListeningToggle.IsChecked = true;
            StatusTextBlock.Text = "监听中...";
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"键盘钩子初始化失败: {ex.Message}";
        }
    }
    
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!_isListening) return;
        
        Dispatcher.UIThread.Post(() =>
        {
            _pressedKeys.Add(e.Key);
            UpdateKeyDisplay();
        });
    }
    
    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (!_isListening) return;
        
        Dispatcher.UIThread.Post(() =>
        {
            _pressedKeys.Remove(e.Key);
            UpdateKeyDisplay();
        });
    }
    
    private void UpdateKeyDisplay()
    {
        if (_pressedKeys.Count == 0)
        {
            HideKeyDisplay();
            return;
        }
        
        var keyText = FormatKeyCombination(_pressedKeys);
        if (string.IsNullOrEmpty(keyText))
        {
            HideKeyDisplay();
            return;
        }
        
        ShowKeyDisplay(keyText);
        
        // 立即更新显示，不延迟隐藏
        CancelHideTimer();
    }
    
    private string FormatKeyCombination(HashSet<Key> keys)
    {
        var parts = new List<string>();
        
        // 只显示有实际按键的组合，避免只显示修饰键
        var normalKeys = keys.Where(k => !IsModifierKey(k)).ToList();
        if (normalKeys.Count == 0)
        {
            return ""; // 没有实际按键时不显示任何内容
        }
        
        // 检查修饰键（只在有实际按键时显示）
        if (keys.Contains(Key.LeftCtrl) || keys.Contains(Key.RightCtrl))
            parts.Add("Ctrl");
        if (keys.Contains(Key.LeftAlt) || keys.Contains(Key.RightAlt))
            parts.Add("Alt");
        if (keys.Contains(Key.LeftShift) || keys.Contains(Key.RightShift))
            parts.Add("Shift");
        if (keys.Contains(Key.LWin) || keys.Contains(Key.RWin))
            parts.Add("Win");
        
        // 添加普通按键
        foreach (var key in normalKeys)
        {
            parts.Add(FormatKeyName(key));
        }
        
        return string.Join(" + ", parts);
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
            Key.Escape => "Esc",
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
            
            // 设置位置为屏幕左下角，稍微调整位置
            var screens = Screens?.All;
            if (screens != null && screens.Count > 0)
            {
                var primaryScreen = screens[0];
                var screenBounds = primaryScreen.Bounds;
                
                _keyDisplayWindow.Position = new PixelPoint(
                    screenBounds.X + 50,
                    screenBounds.Y + screenBounds.Height - 120
                );
            }
            
            _keyDisplayWindow.Show();
        }
        
        // 更新内容
        _keyDisplayWindow.UpdateContent(keyText, GetSelectedColor(), GetFontSize());
        
        // 确保窗口可见
        if (!_keyDisplayWindow.IsVisible)
        {
            _keyDisplayWindow.Show();
        }
        
        // 更新预览
        if (PreviewText != null)
            PreviewText.Text = keyText;
        if (StatusTextBlock != null)
            StatusTextBlock.Text = $"显示按键: {keyText}";
    }
    
    private void HideKeyDisplay()
    {
        // 清空显示内容但保持窗口存在
        if (_keyDisplayWindow != null)
        {
            _keyDisplayWindow.UpdateContent("", GetSelectedColor(), GetFontSize());
        }
        if (StatusTextBlock != null)
            StatusTextBlock.Text = "监听中...";
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
            if (StatusTextBlock != null)
                StatusTextBlock.Text = "监听中...";
            _pressedKeys.Clear();
            HideKeyDisplay();
        }
        else
        {
            if (StatusTextBlock != null)
                StatusTextBlock.Text = "监听已停止";
            _pressedKeys.Clear();
            HideKeyDisplay();
            CancelHideTimer();
        }
    }
    
    private void TestButton_Click(object? sender, RoutedEventArgs e)
    {
        ShowKeyDisplay("Ctrl + Shift + A");
    }
    
    private void HideButton_Click(object? sender, RoutedEventArgs e)
    {
        HideToBackground();
    }
    
    private void ConfigButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_configPopover != null)
        {
            var button = sender as Button;
            if (button != null)
            {
                var screenPoint = button.PointToScreen(new Point(0, button.Bounds.Height));
                var clientPoint = this.PointToClient(screenPoint);
                _configPopover.ShowConfig(clientPoint);
            }
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
                _aiChatWindow.Activate();
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
        
        StatusTextBlock.Text = "手动测试已执行，查看调试面板获取详细信息";
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
                StatusTextBlock.Text = "标签已强制显示 - 查看屏幕左侧 (x=50, y=480)";
                
                // 5秒后显示状态
                Task.Delay(5000).ContinueWith(_ =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        var newStatus = _noteTagManager.GetTagStatus();
                        _debugOverlay?.LogEvent($"📊 5秒后状态: {newStatus}");
                        StatusTextBlock.Text = $"标签状态已更新: {newStatus}";
                    });
                });
            }
            catch (Exception ex)
            {
                _debugOverlay?.LogEvent($"❌ 标签测试失败: {ex.Message}");
                StatusTextBlock.Text = $"标签测试失败: {ex.Message}";
            }
        }
        else
        {
            _debugOverlay?.LogEvent("❌ 标签管理器未初始化");
            StatusTextBlock.Text = "标签管理器未初始化";
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
            0 => Color.Parse("#0078D4"), // 蓝色
            1 => Color.Parse("#107C10"), // 绿色
            2 => Color.Parse("#D13438"), // 红色
            3 => Color.Parse("#8764B8"), // 紫色
            4 => Color.Parse("#FF8C00"), // 橙色
            _ => Color.Parse("#0078D4")
        };
    }
    
    private double GetDisplayDuration()
    {
        return DisplayDurationSlider?.Value ?? 2.0;
    }
    
    private double GetFontSize()
    {
        return FontSizeSlider?.Value ?? 24.0;
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
            _textSelectionPopover = new TextSelectionPopover(_debugOverlay);
            _textSelectionPopover.CopyRequested += OnTextCopyRequested;
            _textSelectionPopover.TranslateRequested += OnTextTranslateRequested;
            
            // 初始化边缘滑动组件
            _edgeSwipeComponent = new SimpleEdgeComponent(_debugOverlay);
            _edgeSwipeComponent.WindowOpened += OnEdgeWindowOpened;
            _edgeSwipeComponent.WindowClosed += OnEdgeWindowClosed;
            
            // 启动边缘组件自动显示（可选）
            StartEdgeAutoShow();
            
            // 初始化AI聊天窗口
            _aiChatWindow = new AIChatWindow();
            
            // 初始化配置弹出框
            _configPopover = new ConfigPopover(this);
            _configPopover.AutoStartChanged += OnAutoStartConfigChanged;
            _configPopover.MinimizeToTrayChanged += OnMinimizeToTrayConfigChanged;
            _configPopover.ShowNotificationsChanged += OnShowNotificationsConfigChanged;
            
            // 初始化调试覆盖层
            _debugOverlay = new EnhancedDebugOverlay();
            _debugOverlay.ShowDebug();
            
            // 初始化创可贴标签组件
            System.Console.WriteLine($"[MainWindow] 开始初始化标签管理器...");
            _noteTagManager = new NoteTagComponent.NoteTagManager(this);
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
            
            if (StatusTextBlock != null)
                StatusTextBlock.Text = "所有功能已初始化 (增强调试模式)";
        }
        catch (Exception ex)
        {
            if (StatusTextBlock != null)
                StatusTextBlock.Text = $"功能初始化失败: {ex.Message}";
        }
    }
    
    private void OnTextCopyRequested(object? sender, string text)
    {
        if (StatusTextBlock != null)
            StatusTextBlock.Text = $"已复制文本: {text.Substring(0, Math.Min(text.Length, 20))}...";
        
        if (_debugOverlay is EnhancedDebugOverlay enhanced)
        {
            enhanced.LogTextCopy(text);
        }
    }
    
    private void OnTextTranslateRequested(object? sender, string text)
    {
        if (StatusTextBlock != null)
            StatusTextBlock.Text = $"翻译请求: {text.Substring(0, Math.Min(text.Length, 20))}...";
        
        if (_debugOverlay is EnhancedDebugOverlay enhanced)
        {
            enhanced.LogEvent($"翻译请求: {text.Substring(0, Math.Min(text.Length, 10))}...");
        }
        // 这里可以集成翻译API
    }
    
    private void OnEdgeWindowOpened(object? sender, EventArgs e)
    {
        if (StatusTextBlock != null)
            StatusTextBlock.Text = "边缘工具栏已打开";
        
        if (_debugOverlay is EnhancedDebugOverlay enhanced)
        {
            enhanced.LogEdgeWindowShown();
        }
    }
    
    private void OnEdgeWindowClosed(object? sender, EventArgs e)
    {
        if (StatusTextBlock != null)
            StatusTextBlock.Text = "边缘工具栏已关闭";
        
        if (_debugOverlay is EnhancedDebugOverlay enhanced)
        {
            enhanced.LogEdgeWindowHidden();
        }
    }
    
    private void OnAutoStartConfigChanged(object? sender, bool enabled)
    {
        if (StatusTextBlock != null)
            StatusTextBlock.Text = $"开机自启: {(enabled ? "已启用" : "已禁用")}";
        _debugOverlay?.LogEvent($"开机自启: {(enabled ? "已启用" : "已禁用")}");
    }
    
    private void OnMinimizeToTrayConfigChanged(object? sender, bool enabled)
    {
        if (StatusTextBlock != null)
            StatusTextBlock.Text = $"最小化到托盘: {(enabled ? "已启用" : "已禁用")}";
        _debugOverlay?.LogEvent($"最小化到托盘: {(enabled ? "已启用" : "已禁用")}");
    }
    
    private void OnShowNotificationsConfigChanged(object? sender, bool enabled)
    {
        if (StatusTextBlock != null)
            StatusTextBlock.Text = $"显示通知: {(enabled ? "已启用" : "已禁用")}";
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
        
        // 清理新功能组件
        _textSelectionPopover?.Dispose();
        _edgeSwipeComponent?.Dispose();
        _aiChatWindow?.Close();
        _configPopover?.Dispose();
        _edgeAutoShowTimer?.Dispose();
        
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
        Activate();
    }
}
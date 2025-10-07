using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;
using ConfigButtonDisplay.Features.Debug.Controls;

namespace ConfigButtonDisplay.Features.EdgeComponents.Controls;

public class ClearEdgeComponent : IDisposable
{
    private Window? _edgeWindow;
    private Border? _contentBorder;
    private bool _isWindowVisible = false;
    private DebugOverlay? _debugOverlay;
    private const int WINDOW_WIDTH = 300;
    private const int WINDOW_HEIGHT = 600;
    private DateTime _lastShowTime = DateTime.MinValue;
    private const int MIN_INTERVAL = 3000; // 最小显示间�?�?
    private Timer? _autoShowTimer;
    private int _showCount = 0;
    
    // 事件
    public event EventHandler? WindowOpened;
    public event EventHandler? WindowClosed;
    
    public ClearEdgeComponent(DebugOverlay? debugOverlay = null)
    {
        _debugOverlay = debugOverlay;
        InitializeComponent();
        _debugOverlay?.LogEvent("🚀 清晰边缘组件已初始化");
    }
    
    private void InitializeComponent()
    {
        // 创建边缘窗口
        _edgeWindow = new Window
        {
            Title = "边缘工具�?,
            Width = WINDOW_WIDTH,
            Height = WINDOW_HEIGHT,
            WindowStartupLocation = WindowStartupLocation.Manual,
            CanResize = false,
            ShowInTaskbar = false,
            Topmost = false,
            SystemDecorations = SystemDecorations.None,
            Background = new SolidColorBrush(Color.Parse("#F8F9FA")),
            IsEnabled = true
        };
        
        // 设置窗口位置（屏幕右侧）
        PositionWindowAtEdge();
        
        // 创建内容区域
        _contentBorder = new Border
        {
            Background = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Color.Parse("#E9ECEF")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12, 0, 0, 12),
            Padding = new Thickness(20)
        };
        
        // 创建内容容器
        var contentStack = new StackPanel
        {
            Spacing = 16
        };
        
        // 标题
        var titleText = new TextBlock
        {
            Text = "边缘工具�?,
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#1C1E21")),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        
        // 状态显�?
        var statusTitle = new TextBlock
        {
            Text = "📊 组件状�?,
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(Color.Parse("#1890FF"))
        };
        
        var statusText = new TextBlock
        {
            Text = "组件已初始化，等待触�?,
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#666666"))
        };
        
        // 功能按钮
        var functionPanel = new StackPanel
        {
            Spacing = 8
        };
        
        AddFunctionButton(functionPanel, "📝 笔记", "快速记录想�?, OnNoteClick);
        AddFunctionButton(functionPanel, "🔍 搜索", "搜索内容", OnSearchClick);
        AddFunctionButton(functionPanel, "🎵 音乐", "播放音乐", OnMusicClick);
        AddFunctionButton(functionPanel, "📊 统计", "查看统计", OnStatsClick);
        
        // 控制按钮
        var controlPanel = new StackPanel
        {
            Spacing = 8,
            Margin = new Thickness(0, 20, 0, 0)
        };
        
        var autoShowButton = new Button
        {
            Content = "🎯 自动显示测试",
            Background = new SolidColorBrush(Color.Parse("#1890FF")),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 8),
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        autoShowButton.Click += (s, e) => TriggerAutoShow();
        
        var manualShowButton = new Button
        {
            Content = "👆 手动显示",
            Background = new SolidColorBrush(Color.Parse("#52C41A")),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 8),
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        manualShowButton.Click += (s, e) => ShowEdgeWindow();
        
        var hideButton = new Button
        {
            Content = "�?隐藏",
            Background = new SolidColorBrush(Color.Parse("#FF4D4F")),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 8),
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        hideButton.Click += (s, e) => HideEdgeWindow();
        
        controlPanel.Children.Add(autoShowButton);
        controlPanel.Children.Add(manualShowButton);
        controlPanel.Children.Add(hideButton);
        
        contentStack.Children.Add(titleText);
        contentStack.Children.Add(statusTitle);
        contentStack.Children.Add(statusText);
        contentStack.Children.Add(functionPanel);
        contentStack.Children.Add(controlPanel);
        
        _contentBorder.Child = contentStack;
        _edgeWindow.Content = _contentBorder;
        
        UpdateStatus("组件已准备就�?);
    }
    
    private void AddFunctionButton(Panel parent, string text, string description, EventHandler<RoutedEventArgs> clickHandler)
    {
        var button = new Button
        {
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    new TextBlock { Text = text.Split(' ')[0], FontSize = 16 },
                    new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        Spacing = 2,
                        Children =
                        {
                            new TextBlock { Text = text.Split(' ')[1], FontSize = 13, FontWeight = FontWeight.Medium },
                            new TextBlock { Text = description, FontSize = 11, Foreground = new SolidColorBrush(Color.Parse("#666666")) }
                        }
                    }
                }
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Background = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Color.Parse("#D1D5DB")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12, 8),
            Margin = new Thickness(0, 2, 0, 2),
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        
        button.Click += (s, e) =>
        {
            _debugOverlay?.LogEvent($"点击�? {text}");
            clickHandler?.Invoke(s, e);
        };
        
        parent.Children.Add(button);
    }
    
    private void UpdateStatus(string message)
    {
        // 简化状态更新逻辑
        _debugOverlay?.LogEvent($"状态更�? {message}");
    }
    
    private void PositionWindowAtEdge()
    {
        if (_edgeWindow != null)
        {
            try
            {
                // 获取屏幕信息
                if (Avalonia.Application.Current?.ApplicationLifetime 
                    is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                    && desktop.MainWindow != null)
                {
                    var screens = desktop.MainWindow.Screens;
                    if (screens != null && screens.All.Count > 0)
                    {
                        var primaryScreen = screens.All[0];
                        var screenBounds = primaryScreen.Bounds;
                        
                        _edgeWindow.Position = new PixelPoint(
                            screenBounds.X + screenBounds.Width - WINDOW_WIDTH,
                            screenBounds.Y + (screenBounds.Height - WINDOW_HEIGHT) / 2
                        );
                        return;
                    }
                }
                
                // 默认位置
                _edgeWindow.Position = new PixelPoint(1620, 200); // 1920-300=1620, 居中
            }
            catch (Exception ex)
            {
                _debugOverlay?.LogEvent($"定位窗口错误: {ex.Message}");
                _edgeWindow.Position = new PixelPoint(1620, 200);
            }
        }
    }
    
    public void TriggerAutoShow()
    {
        var now = DateTime.Now;
        var timeSinceLastShow = (now - _lastShowTime).TotalMilliseconds;
        
        if (timeSinceLastShow < MIN_INTERVAL)
        {
            _debugOverlay?.LogEvent($"�?边缘组件间隔不足，跳过显�?(还需等待 {MIN_INTERVAL - timeSinceLastShow:F0}ms)");
            return;
        }
        
        if (_isWindowVisible)
        {
            _debugOverlay?.LogEvent("🔄 边缘组件已在显示中，先隐藏再显示");
            HideEdgeWindow();
            
            // 延迟后重新显�?
            Task.Delay(1000).ContinueWith(_ =>
            {
                Dispatcher.UIThread.Post(() => ShowEdgeWindow());
            });
        }
        else
        {
            ShowEdgeWindow();
        }
    }
    
    public void ShowEdgeWindow()
    {
        if (_edgeWindow == null || _isWindowVisible) return;
        
        _isWindowVisible = true;
        WindowOpened?.Invoke(this, EventArgs.Empty);
        _showCount++;
        
        try
        {
            _edgeWindow.Show();
            _lastShowTime = DateTime.Now;
            UpdateStatus($"第{_showCount}次显�?- {DateTime.Now:HH:mm:ss}");
            _debugOverlay?.LogEvent($"🎉 边缘窗口已显�?(第{_showCount}�?");
            
            // 8秒后自动隐藏
            Task.Delay(8000).ContinueWith(_ =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (_isWindowVisible)
                    {
                        HideEdgeWindow();
                    }
                });
            });
        }
        catch (Exception ex)
        {
            _debugOverlay?.LogEvent($"�?显示边缘窗口错误: {ex.Message}");
            _isWindowVisible = false;
        }
    }
    
    public void HideEdgeWindow()
    {
        if (_edgeWindow == null || !_isWindowVisible) return;
        
        _isWindowVisible = false;
        WindowClosed?.Invoke(this, EventArgs.Empty);
        
        try
        {
            _edgeWindow.Hide();
            UpdateStatus("窗口已隐�?);
            _debugOverlay?.LogEvent("边缘窗口已隐�?);
        }
        catch (Exception ex)
        {
            _debugOverlay?.LogEvent($"�?隐藏边缘窗口错误: {ex.Message}");
        }
    }
    
    private void OnNoteClick(object? sender, RoutedEventArgs e)
    {
        _debugOverlay?.LogEvent("📝 笔记功能被点�?);
    }
    
    private void OnSearchClick(object? sender, RoutedEventArgs e)
    {
        _debugOverlay?.LogEvent("🔍 搜索功能被点�?);
    }
    
    private void OnMusicClick(object? sender, RoutedEventArgs e)
    {
        _debugOverlay?.LogEvent("🎵 音乐功能被点�?);
    }
    
    private void OnStatsClick(object? sender, RoutedEventArgs e)
    {
        _debugOverlay?.LogEvent("📊 统计功能被点�?);
    }
    
    public void Dispose()
    {
        _autoShowTimer?.Dispose();
        _autoShowTimer = null;
        
        if (_edgeWindow != null)
        {
            try
            {
                _edgeWindow.Close();
                _edgeWindow = null;
            }
            catch (Exception ex)
            {
                _debugOverlay?.LogEvent($"关闭边缘窗口错误: {ex.Message}");
            }
        }
        
        _contentBorder = null;
        _debugOverlay?.LogEvent("清晰边缘组件已清�?);
    }
}

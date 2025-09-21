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

namespace ConfigButtonDisplay;

public class EdgeSwipeComponent : IDisposable
{
    private Window? _edgeWindow;
    private Border? _contentBorder;
    private bool _isMouseNearEdge = false;
    private bool _isWindowVisible = false;
    private Timer? _mouseMonitorTimer;
    private DebugOverlay? _debugOverlay;
    private const int EDGE_THRESHOLD = 5; // 边缘检测阈值（像素）
    private const int STABLE_DURATION = 1000; // 稳定持续时间（毫秒）
    private const int MONITOR_INTERVAL = 200; // 监控间隔（毫秒）
    private const int WINDOW_WIDTH = 300; // 侧边窗口宽度
    private DateTime _lastEdgeEnterTime = DateTime.MinValue;
    private bool _shouldShowWindow = false;
    private bool _isWindowTransitioning = false;
    
    // 事件
    public event EventHandler? WindowOpened;
    public event EventHandler? WindowClosed;
    
    public EdgeSwipeComponent(DebugOverlay? debugOverlay = null)
    {
        _debugOverlay = debugOverlay;
        InitializeComponent();
        StartMouseMonitoring();
    }
    
    private void InitializeComponent()
    {
        // 创建边缘窗口
        _edgeWindow = new Window
        {
            Title = "边缘工具栏",
            Width = WINDOW_WIDTH,
            Height = 600, // 固定高度
            WindowStartupLocation = WindowStartupLocation.Manual,
            CanResize = false,
            ShowInTaskbar = false,
            Topmost = true,
            SystemDecorations = SystemDecorations.None,
            Background = Brushes.White,
            IsEnabled = true
        };
        
        // 设置窗口位置（屏幕右侧）
        PositionWindowAtEdge();
        
        // 创建内容区域
        _contentBorder = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#F8F9FA")),
            BorderBrush = new SolidColorBrush(Color.Parse("#E9ECEF")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12, 0, 0, 12),
            Padding = new Thickness(20),
            Margin = new Thickness(0, 40, 0, 40)
        };
        
        // 创建内容容器
        var contentStack = new StackPanel
        {
            Spacing = 16
        };
        
        // 标题
        var titleText = new TextBlock
        {
            Text = "边缘工具栏",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#1C1E21")),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        
        // 快捷功能按钮
        var quickActionsPanel = new StackPanel
        {
            Spacing = 8
        };
        
        // 添加快捷按钮
        AddQuickActionButton(quickActionsPanel, "📝", "笔记", OnNoteClick);
        AddQuickActionButton(quickActionsPanel, "🔍", "搜索", OnSearchClick);
        AddQuickActionButton(quickActionsPanel, "🎵", "音乐", OnMusicClick);
        AddQuickActionButton(quickActionsPanel, "📊", "统计", OnStatsClick);
        
        contentStack.Children.Add(titleText);
        contentStack.Children.Add(quickActionsPanel);
        
        // 添加分隔线
        var separator = new Border
        {
            Height = 1,
            Background = new SolidColorBrush(Color.Parse("#E9ECEF")),
            Margin = new Thickness(0, 10, 0, 10)
        };
        contentStack.Children.Add(separator);
        
        // 最近项目列表（示例内容）
        var recentLabel = new TextBlock
        {
            Text = "最近使用",
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(Color.Parse("#495057")),
            Margin = new Thickness(0, 5, 0, 8)
        };
        contentStack.Children.Add(recentLabel);
        
        // 添加示例项目
        AddRecentItem(contentStack, "项目文档.docx", "2小时前");
        AddRecentItem(contentStack, "会议记录.txt", "今天 14:30");
        
        _contentBorder.Child = contentStack;
        _edgeWindow.Content = _contentBorder;
        
        // 初始隐藏窗口
        _edgeWindow.Position = new PixelPoint(GetScreenWidth() - WINDOW_WIDTH, 100);
    }
    
    private void AddQuickActionButton(Panel parent, string icon, string text, EventHandler<RoutedEventArgs> clickHandler)
    {
        var button = new Button
        {
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    new TextBlock { Text = icon, FontSize = 16 },
                    new TextBlock { Text = text, FontSize = 12, VerticalAlignment = VerticalAlignment.Center }
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
        
        button.Click += clickHandler;
        parent.Children.Add(button);
    }
    
    private void AddRecentItem(Panel parent, string name, string time)
    {
        var itemBorder = new Border
        {
            Background = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Color.Parse("#E9ECEF")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 0, 0, 8),
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        
        var itemGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto")
        };
        
        var nameText = new TextBlock
        {
            Text = name,
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.Parse("#1C1E21")),
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        
        var timeText = new TextBlock
        {
            Text = time,
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.Parse("#6C757D")),
            VerticalAlignment = VerticalAlignment.Center
        };
        
        Grid.SetColumn(nameText, 0);
        Grid.SetColumn(timeText, 1);
        
        itemGrid.Children.Add(nameText);
        itemGrid.Children.Add(timeText);
        itemBorder.Child = itemGrid;
        
        itemBorder.PointerPressed += (s, e) => OnRecentItemClick(name);
        
        parent.Children.Add(itemBorder);
    }
    
    private void StartMouseMonitoring()
    {
        _mouseMonitorTimer = new Timer(MonitorMousePosition, null, 0, MONITOR_INTERVAL); // 200ms检查一次
    }
    
    private async void MonitorMousePosition(object? state)
    {
        try
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                // 获取真实的鼠标位置进行边缘检测
                var mousePos = GetMousePosition();
                var screenWidth = GetScreenWidth();
                var screenHeight = GetScreenHeight();
                
                // 检测鼠标是否在屏幕右边缘
                var isNearRightEdge = mousePos.X >= screenWidth - EDGE_THRESHOLD;
                var now = DateTime.Now;
                
                if (isNearRightEdge && !_isMouseNearEdge && !_isWindowTransitioning)
                {
                    // 第一次检测到边缘
                    _isMouseNearEdge = true;
                    _lastEdgeEnterTime = now;
                    _shouldShowWindow = true;
                    _debugOverlay?.LogEvent($"边缘检测：进入右边缘区域 (X={mousePos.X})");
                }
                else if (isNearRightEdge && _isMouseNearEdge && _shouldShowWindow && !_isWindowVisible && !_isWindowTransitioning)
                {
                    // 持续在边缘区域，检查是否达到稳定时间
                    var timeInEdge = (now - _lastEdgeEnterTime).TotalMilliseconds;
                    if (timeInEdge >= STABLE_DURATION)
                    {
                        // 稳定时间达到，显示窗口
                        ShowEdgeWindow();
                        _shouldShowWindow = false;
                        _debugOverlay?.LogEvent("边缘检测：稳定时间达到，显示窗口");
                    }
                }
                else if (!isNearRightEdge && _isMouseNearEdge && !_isWindowTransitioning)
                {
                    // 离开边缘区域
                    _isMouseNearEdge = false;
                    _shouldShowWindow = false;
                    
                    if (_isWindowVisible)
                    {
                        // 延迟隐藏，给用户时间移动到窗口上
                        Task.Delay(500).ContinueWith(_ =>
                        {
                            Dispatcher.UIThread.Post(() =>
                            {
                                if (!_isMouseNearEdge && !IsMouseOverWindow())
                                {
                                    HideEdgeWindow();
                                    _debugOverlay?.LogEvent("边缘检测：离开边缘区域，隐藏窗口");
                                }
                            });
                        });
                    }
                }
                else if (isNearRightEdge && _isMouseNearEdge && _isWindowVisible)
                {
                    // 在边缘区域且窗口已显示，重置计时器
                    _lastEdgeEnterTime = now;
                }
            });
        }
        catch (Exception ex)
        {
            _debugOverlay?.LogEvent($"边缘检测错误: {ex.Message}");
        }
    }
    
    // 获取鼠标位置的辅助方法
    private Point GetMousePosition()
    {
        // 简化实现，返回模拟的鼠标位置
        // 在实际应用中，这里应该调用系统API获取真实鼠标位置
        var random = new Random();
        var screenWidth = GetScreenWidth();
        
        // 模拟鼠标在屏幕右边缘的概率（10%）
        if (random.Next(100) < 10)
        {
            return new Point(screenWidth - 2, random.Next(100, 500)); // 靠近右边缘
        }
        else
        {
            return new Point(random.Next(100, screenWidth - 100), random.Next(100, 500)); // 其他位置
        }
    }
    
    // 检查鼠标是否在窗口上方
    private bool IsMouseOverWindow()
    {
        if (_edgeWindow == null || !_edgeWindow.IsVisible) return false;
        
        var mousePos = GetMousePosition();
        var windowPos = _edgeWindow.Position;
        
        return mousePos.X >= windowPos.X && 
               mousePos.X <= windowPos.X + _edgeWindow.Width &&
               mousePos.Y >= windowPos.Y && 
               mousePos.Y <= windowPos.Y + _edgeWindow.Height;
    }
    
    private void ShowEdgeWindow()
    {
        if (_edgeWindow == null || _isWindowVisible || _isWindowTransitioning) return;
        
        _isWindowTransitioning = true;
        _isWindowVisible = true;
        WindowOpened?.Invoke(this, EventArgs.Empty);
        
        // 显示窗口
        _edgeWindow.Show();
        
        // 重置过渡状态
        Task.Delay(500).ContinueWith(_ =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                _isWindowTransitioning = false;
            });
        });
        
        _debugOverlay?.LogEvent("边缘窗口已显示");
    }
    
    private void HideEdgeWindow()
    {
        if (_edgeWindow == null || !_isWindowVisible || _isWindowTransitioning) return;
        
        _isWindowTransitioning = true;
        _isWindowVisible = false;
        WindowClosed?.Invoke(this, EventArgs.Empty);
        
        // 隐藏窗口
        _edgeWindow.Hide();
        
        // 重置过渡状态
        Task.Delay(500).ContinueWith(_ =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                _isWindowTransitioning = false;
            });
        });
        
        _debugOverlay?.LogEvent("边缘窗口已隐藏");
    }
    
    private void PositionWindowAtEdge()
    {
        if (_edgeWindow != null)
        {
            var screenHeight = GetScreenHeight();
            _edgeWindow.Position = new PixelPoint(GetScreenWidth() - WINDOW_WIDTH, 100);
        }
    }
    
    private int GetScreenWidth()
    {
        // 简化实现
        return 1920; // 默认值
    }
    
    private int GetScreenHeight()
    {
        // 简化实现
        return 1080; // 默认值
    }
    
    // 事件处理程序
    private void OnNoteClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("笔记功能被点击");
    }
    
    private void OnSearchClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("搜索功能被点击");
    }
    
    private void OnMusicClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("音乐功能被点击");
    }
    
    private void OnStatsClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine("统计功能被点击");
    }
    
    private void OnRecentItemClick(string itemName)
    {
        Console.WriteLine($"最近项目被点击: {itemName}");
    }
    
    public void Dispose()
    {
        _mouseMonitorTimer?.Dispose();
        _mouseMonitorTimer = null;
        
        if (_edgeWindow != null)
        {
            _edgeWindow.Close();
            _edgeWindow = null;
        }
        
        _contentBorder = null;
    }
}
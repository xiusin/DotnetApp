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

public class SimpleEdgeComponent : IDisposable
{
    private Window? _edgeWindow;
    private StackPanel? _contentPanel;
    private bool _isWindowVisible = false;
    private DebugOverlay? _debugOverlay;
    private const int WINDOW_WIDTH = 300;
    private const int WINDOW_HEIGHT = 600;
    private DateTime _lastShowTime = DateTime.MinValue;
    private const int MIN_INTERVAL = 3000; // 最小显示间隔3秒
    private Timer? _autoShowTimer;
    private int _showCount = 0;
    
    // 事件
    public event EventHandler? WindowOpened;
    public event EventHandler? WindowClosed;
    
    public SimpleEdgeComponent(DebugOverlay? debugOverlay = null)
    {
        _debugOverlay = debugOverlay;
        InitializeComponent();
        _debugOverlay?.LogEvent("🚀 简化边缘组件已初始化");
    }
    
    private void InitializeComponent()
    {
        // 创建边缘窗口
        _edgeWindow = new Window
        {
            Title = "边缘工具栏",
            Width = WINDOW_WIDTH,
            Height = WINDOW_HEIGHT,
            WindowStartupLocation = WindowStartupLocation.Manual,
            CanResize = false,
            ShowInTaskbar = false,
            Topmost = true,
            SystemDecorations = SystemDecorations.None,
            Background = new SolidColorBrush(Color.Parse("#FFFFFF")),
            IsEnabled = true
        };
        
        // 设置窗口位置（屏幕右侧）
        PositionWindowAtEdge();
        
        // 创建简单内容
        _contentPanel = new StackPanel
        {
            Spacing = 10,
            Margin = new Thickness(20)
        };
        
        // 添加标题
        var titleText = new TextBlock
        {
            Text = "边缘工具栏",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#1C1E21")),
            Margin = new Thickness(0, 0, 0, 10)
        };
        
        // 添加状态信息
        var statusText = new TextBlock
        {
            Text = "组件已初始化",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.Parse("#666666"))
        };
        
        // 添加简单按钮
        var noteButton = new Button
        {
            Content = "📝 笔记",
            Background = new SolidColorBrush(Color.Parse("#1890FF")),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 8),
            Margin = new Thickness(0, 5, 0, 5),
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        noteButton.Click += (s, e) => _debugOverlay?.LogEvent("笔记功能被点击");
        
        var searchButton = new Button
        {
            Content = "🔍 搜索",
            Background = new SolidColorBrush(Color.Parse("#52C41A")),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 8),
            Margin = new Thickness(0, 5, 0, 5),
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        searchButton.Click += (s, e) => _debugOverlay?.LogEvent("搜索功能被点击");
        
        var hideButton = new Button
        {
            Content = "✕ 隐藏窗口",
            Background = new SolidColorBrush(Color.Parse("#FF4D4F")),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 8),
            Margin = new Thickness(0, 5, 0, 5),
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        hideButton.Click += (s, e) => HideEdgeWindow();
        
        _contentPanel.Children.Add(titleText);
        _contentPanel.Children.Add(statusText);
        _contentPanel.Children.Add(noteButton);
        _contentPanel.Children.Add(searchButton);
        _contentPanel.Children.Add(hideButton);
        
        _edgeWindow.Content = _contentPanel;
        
        UpdateStatus("组件已准备就绪");
    }
    
    private void UpdateStatus(string message)
    {
        if (_contentPanel?.Children[1] is TextBlock statusText)
        {
            statusText.Text = message;
        }
        _debugOverlay?.LogEvent($"状态更新: {message}");
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
            _debugOverlay?.LogEvent($"⏰ 边缘组件间隔不足，跳过显示 (还需等待 {MIN_INTERVAL - timeSinceLastShow:F0}ms)");
            return;
        }
        
        if (_isWindowVisible)
        {
            _debugOverlay?.LogEvent("🔄 边缘组件已在显示中，先隐藏再显示");
            HideEdgeWindow();
            
            // 延迟后重新显示
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
            UpdateStatus($"第{_showCount}次显示 - {DateTime.Now:HH:mm:ss}");
            _debugOverlay?.LogEvent($"🎉 边缘窗口已显示 (第{_showCount}次)");
            
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
            _debugOverlay?.LogEvent($"❌ 显示边缘窗口错误: {ex.Message}");
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
            UpdateStatus("窗口已隐藏");
            _debugOverlay?.LogEvent("边缘窗口已隐藏");
        }
        catch (Exception ex)
        {
            _debugOverlay?.LogEvent($"❌ 隐藏边缘窗口错误: {ex.Message}");
        }
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
        
        _contentPanel = null;
        _debugOverlay?.LogEvent("简化边缘组件已清理");
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Threading;

namespace ConfigButtonDisplay;

public class DebugOverlay : Window
{
    private TextBlock? _debugText;
    private Timer? _updateTimer;
    private int _shiftPressCount = 0;
    private DateTime _lastShiftPress = DateTime.MinValue;
    
    public DebugOverlay()
    {
        InitializeComponent();
        StartDebugMonitoring();
    }
    
    private void InitializeComponent()
    {
        Title = "Debug Overlay";
        Width = 300;
        Height = 200;
        WindowStartupLocation = WindowStartupLocation.Manual;
        Position = new PixelPoint(100, 100);
        CanResize = false;
        ShowInTaskbar = false;
        Topmost = true;
        SystemDecorations = SystemDecorations.None;
        Background = new SolidColorBrush(Color.Parse("#CC000000"));
        
        var mainBorder = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#AA000000")),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16),
            Margin = new Thickness(10)
        };
        
        var stackPanel = new StackPanel { Spacing = 8 };
        
        var titleText = new TextBlock
        {
            Text = "调试信息",
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        _debugText = new TextBlock
        {
            Text = "等待输入...",
            FontSize = 12,
            Foreground = Brushes.White,
            TextWrapping = TextWrapping.Wrap
        };
        
        var closeButton = new Button
        {
            Content = "关闭",
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 0),
            Background = new SolidColorBrush(Color.Parse("#FF444444")),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8, 4)
        };
        closeButton.Click += (s, e) => Hide();
        
        stackPanel.Children.Add(titleText);
        stackPanel.Children.Add(_debugText);
        stackPanel.Children.Add(closeButton);
        
        mainBorder.Child = stackPanel;
        Content = mainBorder;
    }
    
    private void StartDebugMonitoring()
    {
        _updateTimer = new Timer(UpdateDebugInfo, null, 0, 1000);
        
        // 监听Shift键
        if (Application.Current?.ApplicationLifetime 
            is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow != null)
        {
            desktop.MainWindow.AddHandler(KeyDownEvent, OnDebugKeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        }
    }
    
    private void OnDebugKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
        {
            var now = DateTime.Now;
            var timeSinceLastPress = (now - _lastShiftPress).TotalMilliseconds;
            
            if (timeSinceLastPress < 300)
            {
                _shiftPressCount++;
                UpdateDebugInfo(null);
            }
            else
            {
                _shiftPressCount = 1;
            }
            
            _lastShiftPress = now;
        }
    }
    
    private void UpdateDebugInfo(object? state)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_debugText != null)
            {
                var windowCount = 0;
                if (Application.Current?.ApplicationLifetime 
                    is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                {
                    windowCount = desktop.Windows.Count;
                }
                
                _debugText.Text = $"""
                    Shift双击次数: {_shiftPressCount}
                    最后按键: {_lastShiftPress:HH:mm:ss}
                    文本选择检测: 运行中
                    边缘检测: 运行中
                    窗口数量: {windowCount}
                    """;
            }
        });
    }
    
    public void ShowDebug()
    {
        if (!IsVisible)
        {
            Show();
            Activate();
        }
    }
    
    public void HideDebug()
    {
        if (IsVisible)
        {
            Hide();
        }
    }
    
    public void LogEvent(string eventName)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_debugText != null)
            {
                _debugText.Text += $"\n[{DateTime.Now:HH:mm:ss}] {eventName}";
            }
        });
    }
    
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _updateTimer?.Dispose();
        
        if (Application.Current?.ApplicationLifetime 
            is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow != null)
        {
            desktop.MainWindow.RemoveHandler(KeyDownEvent, OnDebugKeyDown);
        }
    }
}
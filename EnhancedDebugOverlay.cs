using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConfigButtonDisplay;

public class EnhancedDebugOverlay : Window
{
    private TextBlock? _debugText;
    private StackPanel? _statusPanel;
    private Dictionary<string, TextBlock> _statusItems = new();
    private Timer? _statusUpdateTimer;
    
    // 状态跟踪
    private int _shiftPressCount = 0;
    private int _shiftDoubleClickCount = 0;
    private bool _lastShiftState = false;
    private DateTime _lastShiftPress = DateTime.MinValue;
    private bool _isChatWindowVisible = false;
    private int _edgeShowCount = 0;
    private int _textSelectCount = 0;
    private DateTime _startTime = DateTime.Now;
    
    public EnhancedDebugOverlay()
    {
        InitializeComponent();
        StartStatusUpdater();
    }
    
    private void InitializeComponent()
    {
        Title = "增强调试面板";
        Width = 400;
        Height = 500;
        WindowStartupLocation = WindowStartupLocation.Manual;
        Position = new PixelPoint(50, 50);
        CanResize = true;
        ShowInTaskbar = false;
        Topmost = true;
        SystemDecorations = SystemDecorations.BorderOnly;
        Background = new SolidColorBrush(Color.Parse("#CC000000"));
        
        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
            Margin = new Thickness(10)
        };
        
        // 标题区域
        var titleBorder = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#1890FF")),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 0, 0, 10)
        };
        
        var titleText = new TextBlock
        {
            Text = "🔧 增强调试面板",
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        
        titleBorder.Child = titleText;
        Grid.SetRow(titleBorder, 0);
        mainGrid.Children.Add(titleBorder);
        
        // 状态面板
        _statusPanel = new StackPanel
        {
            Spacing = 4,
            Margin = new Thickness(0, 0, 0, 10)
        };
        
        // 初始化状态项
        AddStatusItem("运行时间", "00:00:00");
        AddStatusItem("Shift按键", "0 次");
        AddStatusItem("Shift双击", "0 次");
        AddStatusItem("聊天窗口", "隐藏");
        AddStatusItem("边缘显示", "0 次");
        AddStatusItem("文本选择", "0 次");
        AddStatusItem("最后操作", "无");
        
        var statusBorder = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#F0F8FF")),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12),
            Child = _statusPanel
        };
        
        Grid.SetRow(statusBorder, 1);
        mainGrid.Children.Add(statusBorder);
        
        // 日志区域
        var logBorder = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#1A1A1A")),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 10, 0, 0)
        };
        
        _debugText = new TextBlock
        {
            Text = "增强调试系统已启动...\n",
            FontSize = 11,
            Foreground = Brushes.White,
            TextWrapping = TextWrapping.Wrap,
            MaxHeight = 200
        };
        
        var logScroll = new ScrollViewer
        {
            Content = _debugText,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        
        logBorder.Child = logScroll;
        Grid.SetRow(logBorder, 2);
        mainGrid.Children.Add(logBorder);
        
        Content = mainGrid;
        
        LogEvent("🚀 增强调试系统已初始化");
        UpdateStatus("运行时间", "00:00:00");
    }
    
    private void AddStatusItem(string key, string initialValue)
    {
        var itemPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8
        };
        
        var keyText = new TextBlock
        {
            Text = key + ":",
            FontSize = 12,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(Color.Parse("#1890FF")),
            Width = 80
        };
        
        var valueText = new TextBlock
        {
            Text = initialValue,
            FontSize = 12,
            Foreground = Brushes.White,
            FontWeight = FontWeight.Medium
        };
        
        itemPanel.Children.Add(keyText);
        itemPanel.Children.Add(valueText);
        
        _statusPanel?.Children.Add(itemPanel);
        _statusItems[key] = valueText;
    }
    
    private void StartStatusUpdater()
    {
        _statusUpdateTimer = new Timer(UpdateStatusDisplay, null, 0, 1000); // 每秒更新
    }
    
    private void UpdateStatusDisplay(object? state)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var elapsed = DateTime.Now - _startTime;
            UpdateStatus("运行时间", $"{elapsed.Hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}");
        });
    }
    
    public void UpdateStatus(string key, string value)
    {
        if (_statusItems.ContainsKey(key))
        {
            Dispatcher.UIThread.Post(() =>
            {
                _statusItems[key].Text = value;
                
                // 高亮重要状态变化
                if (value.Contains("成功") || value.Contains("显示") || value.Contains("打开"))
                {
                    _statusItems[key].Foreground = new SolidColorBrush(Color.Parse("#52C41A"));
                }
                else if (value.Contains("失败") || value.Contains("错误"))
                {
                    _statusItems[key].Foreground = new SolidColorBrush(Color.Parse("#FF4D4F"));
                }
                else if (value.Contains("隐藏") || value.Contains("关闭"))
                {
                    _statusItems[key].Foreground = new SolidColorBrush(Color.Parse("#FFA940"));
                }
                else
                {
                    _statusItems[key].Foreground = Brushes.White;
                }
            });
        }
    }
    
    public void LogShiftPress(Key key, double timeSinceLast)
    {
        _shiftPressCount++;
        UpdateStatus("Shift按键", $"{_shiftPressCount} 次");
        
        var keyName = key == Key.LeftShift ? "左Shift" : "右Shift";
        LogEvent($"🔄 {keyName} 按键 (距离上次: {timeSinceLast:F0}ms)");
        
        if (_lastShiftState && timeSinceLast < 300)
        {
            _shiftDoubleClickCount++;
            UpdateStatus("Shift双击", $"{_shiftDoubleClickCount} 次");
            LogEvent($"🎉 双击Shift检测成功！ (总计: {_shiftDoubleClickCount}次)");
        }
        
        _lastShiftState = true;
        _lastShiftPress = DateTime.Now;
    }
    
    public void LogChatWindowState(bool isVisible)
    {
        _isChatWindowVisible = isVisible;
        UpdateStatus("聊天窗口", isVisible ? "显示 ✅" : "隐藏 ❌");
        LogEvent($"💬 AI聊天窗口状态: {(isVisible ? "已显示" : "已隐藏")}");
    }
    
    public void LogEdgeWindowShown()
    {
        _edgeShowCount++;
        UpdateStatus("边缘显示", $"{_edgeShowCount} 次");
        LogEvent("🎯 边缘工具栏已显示");
    }
    
    public void LogEdgeWindowHidden()
    {
        UpdateStatus("边缘显示", $"{_edgeShowCount} 次 (已隐藏)");
        LogEvent("🎯 边缘工具栏已隐藏");
    }
    
    public void LogTextSelection(string text)
    {
        _textSelectCount++;
        UpdateStatus("文本选择", $"{_textSelectCount} 次");
        LogEvent($"📝 文本选择: {text.Substring(0, Math.Min(text.Length, 20))}...");
    }
    
    public void LogEvent(string eventName)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_debugText != null)
            {
                _debugText.Text += $"\n[{DateTime.Now:HH:mm:ss.fff}] {eventName}";
                
                // 自动滚动到底部
                if (_debugText.Parent is ScrollViewer scrollViewer)
                {
                    scrollViewer.ScrollToEnd();
                }
            }
        });
    }
    
    public void ShowEnhanced()
    {
        if (!IsVisible)
        {
            Show();
            Activate();
            LogEvent("🚀 增强调试面板已显示");
        }
    }
    
    public void HideEnhanced()
    {
        if (IsVisible)
        {
            Hide();
            LogEvent("🚀 增强调试面板已隐藏");
        }
    }
    
    public void ShowDebug()
    {
        ShowEnhanced();
    }
    
    public void HideDebug()
    {
        HideEnhanced();
    }
}
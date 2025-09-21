using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ConfigButtonDisplay;

public class EnhancedDebugOverlay : DebugOverlay
{
    private StackPanel? _statusPanel = new StackPanel();
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
        // 调用基类的InitializeComponent
        // 基类构造函数会调用InitializeComponent和StartDebugMonitoring
        StartStatusUpdater();
    }
    
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        // 确保窗口位置正确
        Position = new PixelPoint(50, 50);
    }
    
    private void StartStatusUpdater()
    {
        _statusUpdateTimer = new Timer(UpdateStatusDisplay, null, 0, 1000); // 每秒更新
    }
    
    private void UpdateStatusDisplay(object? state)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var uptime = DateTime.Now - _startTime;
            UpdateStatus("运行时间", $"{uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}");
        });
    }
    
    private void AddStatusItem(string key, string initialValue)
    {
        var itemPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
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
            Foreground = new SolidColorBrush(Color.Parse("#333333"))
        };
        
        itemPanel.Children.Add(keyText);
        itemPanel.Children.Add(valueText);
        
        _statusPanel?.Children.Add(itemPanel);
        _statusItems[key] = valueText;
    }
    
    private void UpdateStatus(string key, string value)
    {
        if (_statusItems.ContainsKey(key))
        {
            _statusItems[key].Text = value;
        }
    }
    
    public new void LogShiftPress(Key key, double timeSinceLast)
    {
        _shiftPressCount++;
        _shiftDoubleClickCount++; // 使用这个字段
        UpdateStatus("Shift按键", $"{_shiftPressCount} 次");
        
        var keyName = key == Key.LeftShift ? "左Shift" : "右Shift";
        _lastShiftState = true; // 使用这个字段
        LogEvent($"⌨️ {keyName}按键 (距离上次: {timeSinceLast:F0}ms) [双击计数: {_shiftDoubleClickCount}, 状态: {_lastShiftState}]");
    }
    
    public new void LogTextCopy(string text)
    {
        _textSelectCount++;
        UpdateStatus("文本选择", $"{_textSelectCount} 次");
        LogEvent($"📋 复制文本: {text.Substring(0, Math.Min(text.Length, 20))}...");
    }
    
    public new void LogEdgeWindowShown()
    {
        _edgeShowCount++;
        _isChatWindowVisible = true; // 使用这个字段
        UpdateStatus("边缘显示", $"{_edgeShowCount} 次");
        LogEvent($"🎯 边缘工具栏已显示 [窗口可见: {_isChatWindowVisible}]");
    }
    
    public new void LogEdgeWindowHidden()
    {
        _isChatWindowVisible = false; // 使用这个字段
        UpdateStatus("边缘显示", $"{_edgeShowCount} 次 (已隐藏)");
        LogEvent($"🎯 边缘工具栏已隐藏 [窗口可见: {_isChatWindowVisible}]");
    }
    
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _statusUpdateTimer?.Dispose();
    }
    
    public void ShowEnhanced()
    {
        if (!IsVisible)
        {
            Show();
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
    
    public new void ShowDebug()
    {
        ShowEnhanced();
    }
    
    public new void HideDebug()
    {
        HideEnhanced();
    }
}
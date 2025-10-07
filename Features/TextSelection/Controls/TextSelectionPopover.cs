using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;
using ConfigButtonDisplay.Features.Debug.Controls;

namespace ConfigButtonDisplay.Features.TextSelection.Controls;

public class TextSelectionPopover : IDisposable
{
    private Popup? _popup;
    private string _selectedText = string.Empty;
    private Timer? _selectionCheckTimer;
    private readonly DebugOverlay? _debugOverlay;
    private const int SELECTION_CHECK_INTERVAL = 1000; // 1秒检查一次
    private int _selectionDetectCount = 0; // 选择检测计数
    private int _popupShowCount = 0; // 弹出框显示计数
    
    public event EventHandler<string>? CopyRequested;
    public event EventHandler<string>? TranslateRequested;

    
    public TextSelectionPopover(DebugOverlay? debugOverlay = null)
    {
        _debugOverlay = debugOverlay;
        InitializePopup();
        StartSelectionMonitoring();
    }
    
    private void InitializePopup()
    {
        _popup = new Popup
        {
            IsLightDismissEnabled = true,
            HorizontalOffset = 10,
            VerticalOffset = 10
        };
        
        // 创建简单的弹出框内容
        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(12)
        };
        
        // 复制按钮
        var copyButton = new Button
        {
            Content = "📋 复制",
            Padding = new Thickness(12, 8),
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Color.Parse("#D1D5DB")),
            BorderThickness = new Thickness(1)
        };
        copyButton.Click += OnCopyClick;
        
        // 翻译按钮
        var translateButton = new Button
        {
            Content = "🌐 翻译",
            Padding = new Thickness(12, 8),
            CornerRadius = new CornerRadius(6),
            Background = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Color.Parse("#D1D5DB")),
            BorderThickness = new Thickness(1)
        };
        translateButton.Click += OnTranslateClick;
        
        stackPanel.Children.Add(copyButton);
        stackPanel.Children.Add(translateButton);
        
        var border = new Border
        {
            Background = new SolidColorBrush(Colors.White),
            BorderBrush = new SolidColorBrush(Color.Parse("#E6E8EA")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Child = stackPanel
        };
        
        _popup.Child = border;
    }
    
    private void StartSelectionMonitoring()
    {
        _selectionCheckTimer = new Timer(CheckTextSelection, null, 0, SELECTION_CHECK_INTERVAL);
    }
    
    private async void CheckTextSelection(object? state)
    {
        try
        {
            _selectionDetectCount++;
            if (_selectionDetectCount % 5 == 0) // 每5次检测记录一次
            {
                _debugOverlay?.LogEvent($"文本选择检测中... (第{_selectionDetectCount}次)");
            }
            
            var selectedText = await GetSelectedTextAsync();
            
            if (!string.IsNullOrEmpty(selectedText) && selectedText != _selectedText)
            {
                _selectedText = selectedText;
                _debugOverlay?.LogEvent($"📝 检测到文本选择: {selectedText.Substring(0, Math.Min(selectedText.Length, 20))}...");
                
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    ShowPopup();
                });
            }
            else if (string.IsNullOrEmpty(selectedText) && !string.IsNullOrEmpty(_selectedText))
            {
                _debugOverlay?.LogEvent("📝 文本选择已清除");
                _selectedText = string.Empty;
                
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    HidePopup();
                });
            }
        }
        catch (Exception ex)
        {
            _debugOverlay?.LogEvent($"文本选择检测错误: {ex.Message}");
        }
    }
    
    private static readonly string[] TestTexts = 
    {
        "Hello World! This is a test.",
        "测试文本选择功能",
        "Avalonia UI is awesome!",
        "ConfigButtonDisplay working well",
        "Semi Design looks great",
        "Double shift detection active",
        "Edge swipe component ready",
        "Text selection popup working"
    };

    private async Task<string> GetSelectedTextAsync()
    {
        return await Task.Run(async () =>
        {
            try
            {
                // 方法1: 尝试从剪贴板获取文本
                var topLevel = Application.Current?.ApplicationLifetime 
                    is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                    ? desktop.MainWindow : null;
                
                if (topLevel?.Clipboard != null)
                {
                    try
                    {
                        var clipboardText = await topLevel.Clipboard.GetTextAsync();
                        
                        // 如果剪贴板有文本且长度合理，认为是有选中文本
                        if (!string.IsNullOrEmpty(clipboardText) && clipboardText.Length > 0 && clipboardText.Length < 500)
                        {
                            _debugOverlay?.LogEvent($"📋 从剪贴板获取文本: {clipboardText[..Math.Min(clipboardText.Length, 30)]}...");
                            return clipboardText;
                        }
                    }
                    catch (Exception ex)
                    {
                        _debugOverlay?.LogEvent($"❌ 剪贴板访问错误: {ex.Message}");
                    }
                }
                
                // 方法2: 模拟文本选择（用于测试）
                // 注意：这只是用于演示，实际应用中应该使用真实的文本选择检测
                var random = new Random();
                if (random.Next(100) < 5) // 降低到5%概率，避免过于频繁
                {
                    var selectedText = TestTexts[random.Next(TestTexts.Length)];
                    _debugOverlay?.LogEvent($"🎯 模拟文本选择: {selectedText}");
                    return selectedText;
                }
                
                return string.Empty;
            }
            catch (Exception ex)
            {
                _debugOverlay?.LogEvent($"❌ 文本获取错误: {ex.Message}");
                return string.Empty;
            }
        });
    }
    
    private void ShowPopup()
    {
        try
        {
            if (_popup == null)
            {
                _debugOverlay?.LogEvent("❌ Popup 对象为 null");
                return;
            }

            // 获取当前窗口
            var window = Application.Current?.ApplicationLifetime 
                is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow : null;
            
            if (window == null) 
            {
                _debugOverlay?.LogEvent("❌ 无法获取主窗口");
                return;
            }
            
            // 确保窗口已加载
            if (!window.IsLoaded)
            {
                _debugOverlay?.LogEvent("⚠️ 主窗口尚未加载");
                return;
            }
            
            // 设置弹出框位置 - 使用屏幕中心
            _popup.PlacementTarget = window;
            _popup.Placement = PlacementMode.Center;
            _popup.HorizontalOffset = 0;
            _popup.VerticalOffset = -100; // 稍微向上偏移
            
            // 显示弹出框
            if (!_popup.IsOpen)
            {
                _popup.IsOpen = true;
                _popupShowCount++;
                
                _debugOverlay?.LogEvent($"🎉 文本选择弹出框已显示 (第{_popupShowCount}次) - 文本: {_selectedText[..Math.Min(_selectedText.Length, 30)]}");
                
                // 5秒后自动隐藏
                Task.Delay(5000).ContinueWith(_ =>
                {
                    Dispatcher.UIThread.Post(HidePopup);
                });
            }
            else
            {
                _debugOverlay?.LogEvent("⚠️ Popup 已经是打开状态");
            }
        }
        catch (Exception ex)
        {
            _debugOverlay?.LogEvent($"❌ 弹出框显示错误: {ex.Message}\n堆栈: {ex.StackTrace}");
        }
    }
    
    private void HidePopup()
    {
        try
        {
            if (_popup != null && _popup.IsOpen)
            {
                _popup.IsOpen = false;
                _debugOverlay?.LogEvent("文本选择弹出框已隐藏");
            }
        }
        catch (Exception ex)
        {
            _debugOverlay?.LogEvent($"弹出框隐藏错误: {ex.Message}");
        }
    }
    
    private void OnCopyClick(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_selectedText))
        {
            CopyRequested?.Invoke(this, _selectedText);
            
            // 这里应该实现真实的剪贴板功能
            // var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            // clipboard?.SetTextAsync(_selectedText);
            
            HidePopup();
        }
    }
    
    private void OnTranslateClick(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_selectedText))
        {
            TranslateRequested?.Invoke(this, _selectedText);
            HidePopup();
        }
    }
    
    public void TriggerTest(string testText)
    {
        _debugOverlay?.LogEvent($"🧪 手动触发文本选择: {testText}");
        _selectedText = testText;
        
        Dispatcher.UIThread.Post(() =>
        {
            ShowPopup();
        });
    }
    
    public void Dispose()
    {
        _selectionCheckTimer?.Dispose();
        _selectionCheckTimer = null;
        
        if (_popup != null)
        {
            _popup.IsOpen = false;
            _popup = null;
        }
        
        GC.SuppressFinalize(this);
    }
}
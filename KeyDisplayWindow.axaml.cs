using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using System;
using System.Threading;

namespace ConfigButtonDisplay;

public partial class KeyDisplayWindow : Window
{
    private Timer? _autoHideTimer;
    private const int AUTO_HIDE_DELAY = 2000; // 2秒自动隐藏
    private bool _isContentVisible = false;

    public KeyDisplayWindow()
    {
        InitializeComponent();
        this.Closing += (s, e) => 
        {
            // 阻止关闭窗口，只隐藏
            e.Cancel = true;
            HideWindow();
        };
        
        // 初始化窗口位置到屏幕底部居中
        this.Loaded += OnWindowLoaded;
    }
    
    private void OnWindowLoaded(object? sender, RoutedEventArgs e)
    {
        PositionWindowAtBottomCenter();
    }
    
    /// <summary>
    /// 将窗口定位到屏幕底部居中
    /// </summary>
    private void PositionWindowAtBottomCenter()
    {
        var screens = Screens?.All;
        if (screens != null && screens.Count > 0)
        {
            var primaryScreen = screens[0];
            var screenBounds = primaryScreen.Bounds;
            var workingArea = primaryScreen.WorkingArea;
            
            // 计算底部居中位置
            var windowWidth = (int)this.Width;
            var windowHeight = (int)this.Height;
            
            var x = screenBounds.X + (screenBounds.Width - windowWidth) / 2;
            var y = workingArea.Y + workingArea.Height - windowHeight - 60; // 距离底部60px
            
            this.Position = new PixelPoint(x, y);
        }
    }
    
    /// <summary>
    /// 根据内容动态调整窗口宽度
    /// </summary>
    private void AdjustWindowWidth(string text)
    {
        if (DisplayTextBlock != null && !string.IsNullOrEmpty(text))
        {
            // 根据文本长度估算宽度
            var baseWidth = 120; // 最小宽度
            var charWidth = 20; // 每个字符大约20px宽度
            var estimatedWidth = Math.Max(baseWidth, text.Length * charWidth + 48); // 48px为padding
            
            // 限制在最小和最大宽度之间
            var finalWidth = Math.Min(400, Math.Max(120, estimatedWidth));
            
            this.Width = finalWidth;
            
            // 重新定位到底部居中
            PositionWindowAtBottomCenter();
        }
    }
    
    public void UpdateContent(string text, Color backgroundColor, double fontSize)
    {
        if (DisplayTextBlock != null)
        {
            DisplayTextBlock.Text = text;
            DisplayTextBlock.FontSize = fontSize;
        }
        
        if (MainBorder != null)
        {
            if (!string.IsNullOrEmpty(text))
            {
                // 有内容时显示半透明背景
                MainBorder.Background = new SolidColorBrush(Color.Parse("#E6000000"));
                MainBorder.BorderBrush = new SolidColorBrush(Color.Parse("#33FFFFFF"));
                
                // 动态调整窗口宽度
                AdjustWindowWidth(text);
                
                // 显示窗口并启动自动隐藏计时器
                ShowWindow();
                StartAutoHideTimer();
                
                _isContentVisible = true;
            }
            else
            {
                // 空内容时隐藏窗口
                HideWindow();
                _isContentVisible = false;
            }
        }
    }
    
    /// <summary>
    /// 显示窗口
    /// </summary>
    private void ShowWindow()
    {
        if (!this.IsVisible)
        {
            this.Show();
        }
        
        // 确保窗口在最顶层
        this.Topmost = true;
        this.Activate();
    }
    
    /// <summary>
    /// 隐藏窗口
    /// </summary>
    private void HideWindow()
    {
        if (this.IsVisible)
        {
            this.Hide();
        }
        
        // 停止自动隐藏计时器
        StopAutoHideTimer();
    }
    
    /// <summary>
    /// 启动2秒自动隐藏计时器
    /// </summary>
    private void StartAutoHideTimer()
    {
        // 先停止现有计时器
        StopAutoHideTimer();
        
        _autoHideTimer = new Timer(_ =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (_isContentVisible)
                {
                    HideWindow();
                    _isContentVisible = false;
                    
                    // 清空内容
                    if (DisplayTextBlock != null)
                    {
                        DisplayTextBlock.Text = "";
                    }
                }
            });
        }, null, AUTO_HIDE_DELAY, Timeout.Infinite);
    }
    
    /// <summary>
    /// 停止自动隐藏计时器
    /// </summary>
    private void StopAutoHideTimer()
    {
        _autoHideTimer?.Dispose();
        _autoHideTimer = null;
    }
    
    /// <summary>
    /// 重新触发显示（用于再次按键时）
    /// </summary>
    public void RefreshDisplay()
    {
        if (_isContentVisible)
        {
            // 重新启动计时器
            StartAutoHideTimer();
        }
    }
    
    protected override void OnClosed(EventArgs e)
    {
        StopAutoHideTimer();
        base.OnClosed(e);
    }
}
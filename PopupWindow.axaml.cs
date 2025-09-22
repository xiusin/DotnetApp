using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace ConfigButtonDisplay;

public partial class PopupWindow : Window
{
    private bool _isAnimating = false;
    private DispatcherTimer? _autoHideTimer;
    
    public PopupWindow()
    {
        AvaloniaXamlLoader.Load(this);
        InitializeWindow();
    }
    
    private void InitializeWindow()
    {
        // 设置窗口初始状态
        Opacity = 0;
        IsVisible = false;
        
        // 初始化自动隐藏计时器
        _autoHideTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(10) // 10秒后自动隐藏
        };
        _autoHideTimer.Tick += AutoHideTimer_Tick;
        
        // 更新状态显示
        UpdateStatusDisplay();
    }
    
    /// <summary>
    /// 显示弹窗，带淡入动画
    /// </summary>
    /// <param name="anchorControl">锚点控件，用于计算显示位置</param>
    public async Task ShowPopupAsync(Control? anchorControl = null)
    {
        if (_isAnimating) return;
        
        try
        {
            _isAnimating = true;
            
            // 计算显示位置
            CalculatePosition(anchorControl);
            
            // 显示窗口
            IsVisible = true;
            Show();
            
            // 淡入动画
            await AnimateOpacityAsync(0, 1, TimeSpan.FromMilliseconds(200));
            
            // 启动自动隐藏计时器
            _autoHideTimer?.Start();
            
            // 更新状态
            UpdateFooterStatus("弹窗已显示");
        }
        finally
        {
            _isAnimating = false;
        }
    }
    
    /// <summary>
    /// 隐藏弹窗，带淡出动画
    /// </summary>
    public async Task HidePopupAsync()
    {
        if (_isAnimating || !IsVisible) return;
        
        try
        {
            _isAnimating = true;
            
            // 停止自动隐藏计时器
            _autoHideTimer?.Stop();
            
            // 淡出动画
            await AnimateOpacityAsync(1, 0, TimeSpan.FromMilliseconds(150));
            
            // 隐藏窗口
            Hide();
            IsVisible = false;
            
            // 更新状态
            UpdateFooterStatus("准备就绪");
        }
        finally
        {
            _isAnimating = false;
        }
    }
    
    /// <summary>
    /// 计算弹窗显示位置
    /// </summary>
    private void CalculatePosition(Control? anchorControl)
    {
        var screen = Screens.Primary;
        if (screen?.WorkingArea == null) return;
        
        var workingArea = screen.WorkingArea;
        var windowWidth = Width;
        var windowHeight = Height;
        
        if (anchorControl != null)
        {
            // 相对于锚点控件定位
            var anchorBounds = anchorControl.Bounds;
            var anchorPosition = anchorControl.PointToScreen(new Point(0, 0));
            
            // 计算最佳显示位置（避免超出屏幕边界）
            var x = anchorPosition.X;
            var y = anchorPosition.Y + anchorBounds.Height + 8; // 锚点下方8px
            
            // 边界检查和调整
            if (x + windowWidth > workingArea.X + workingArea.Width)
                x = (int)(workingArea.X + workingArea.Width - windowWidth - 16);
            if (x < workingArea.X)
                x = (int)(workingArea.X + 16);
            
            if (y + windowHeight > workingArea.Y + workingArea.Height)
                y = (int)(anchorPosition.Y - windowHeight - 8); // 显示在锚点上方
            
            Position = new PixelPoint((int)x, (int)y);
        }
        else
        {
            // 默认居中显示
            var x = (int)((workingArea.Width - windowWidth) / 2 + workingArea.X);
            var y = (int)((workingArea.Height - windowHeight) / 2 + workingArea.Y);
            Position = new PixelPoint(x, y);
        }
    }
    
    /// <summary>
    /// 透明度动画
    /// </summary>
    private async Task AnimateOpacityAsync(double from, double to, TimeSpan duration)
    {
        var animation = new Animation
        {
            Duration = duration,
            Children =
            {
                new KeyFrame
                {
                    Setters = { new Avalonia.Styling.Setter(OpacityProperty, from) },
                    Cue = new Cue(0d)
                },
                new KeyFrame
                {
                    Setters = { new Avalonia.Styling.Setter(OpacityProperty, to) },
                    Cue = new Cue(1d)
                }
            }
        };
        
        await animation.RunAsync(this);
    }
    
    /// <summary>
    /// 更新状态显示
    /// </summary>
    private void UpdateStatusDisplay()
    {
        KeyboardStatusText.Text = "已启用";
        KeyboardStatusText.Foreground = new SolidColorBrush(Color.Parse("#FF48BB78"));
        
        DisplayStatusText.Text = "就绪";
        DisplayStatusText.Foreground = new SolidColorBrush(Color.Parse("#FF3182CE"));
    }
    
    /// <summary>
    /// 更新底部状态文本
    /// </summary>
    private void UpdateFooterStatus(string status)
    {
        FooterStatusText.Text = status;
    }
    
    /// <summary>
    /// 添加动态内容到扩展区域
    /// </summary>
    public void AddDynamicContent(Control content)
    {
        DynamicContentContainer.Children.Add(content);
        DynamicContentArea.IsVisible = true;
    }
    
    /// <summary>
    /// 清空动态内容
    /// </summary>
    public void ClearDynamicContent()
    {
        DynamicContentContainer.Children.Clear();
        DynamicContentArea.IsVisible = false;
    }
    
    // 事件处理器
    private async void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        await HidePopupAsync();
    }
    
    private void AutoHideTimer_Tick(object? sender, EventArgs e)
    {
        _ = Task.Run(async () =>
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await HidePopupAsync();
            });
        });
    }
    
    // 快速操作按钮事件
    private void QuickAction1_Click(object? sender, RoutedEventArgs e)
    {
        UpdateFooterStatus("开始监听键盘输入...");
        // TODO: 实现开始监听逻辑
    }
    
    private void QuickAction2_Click(object? sender, RoutedEventArgs e)
    {
        UpdateFooterStatus("打开设置界面...");
        // TODO: 实现设置界面逻辑
    }
    
    private void QuickAction3_Click(object? sender, RoutedEventArgs e)
    {
        UpdateFooterStatus("执行测试功能...");
        // TODO: 实现测试功能逻辑
    }
    
    private void QuickAction4_Click(object? sender, RoutedEventArgs e)
    {
        UpdateFooterStatus("显示关于信息...");
        // TODO: 实现关于信息逻辑
    }
    
    private void RefreshButton_Click(object? sender, RoutedEventArgs e)
    {
        UpdateStatusDisplay();
        UpdateFooterStatus("状态已刷新");
    }
    
    private void SettingsButton_Click(object? sender, RoutedEventArgs e)
    {
        UpdateFooterStatus("打开高级设置...");
        // TODO: 实现高级设置逻辑
    }
    
    // 窗口事件
    protected override void OnPointerPressed(Avalonia.Input.PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        // 重置自动隐藏计时器
        _autoHideTimer?.Stop();
        _autoHideTimer?.Start();
    }
    
    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        
        // 失去焦点时启动快速隐藏
        _autoHideTimer?.Stop();
        _autoHideTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3) // 3秒后隐藏
        };
        _autoHideTimer.Tick += AutoHideTimer_Tick;
        _autoHideTimer.Start();
    }
    
    protected override void OnClosed(EventArgs e)
    {
        // 清理资源
        _autoHideTimer?.Stop();
        _autoHideTimer = null;
        
        base.OnClosed(e);
    }
}
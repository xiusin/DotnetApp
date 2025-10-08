using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using System;
using System.Threading;

namespace ConfigButtonDisplay.Features.KeyboardMonitoring.Controls;

public partial class KeyDisplayWindow : Window
{
    private Timer? _autoHideTimer;
    private const int AUTO_HIDE_DELAY = 2000; // 2秒自动隐藏
    private bool _isContentVisible = false;
    public bool ForceFullWidth = false; // 修复：默认不使用全屏宽度

    // 欢迎消息相关
    private bool _hasShownWelcome = false;
    private Core.Configuration.KeyboardMonitorSettings? _settings;

    public KeyDisplayWindow()
    {
        InitializeComponent();
        // 绑定到主窗口，确保子窗体显示不受其他窗口影响
        var desktop = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        if (desktop?.MainWindow != null)
        {
            this.Owner = desktop.MainWindow;
        }

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
        // 异步定位到底部居中，避免初次显示时尺寸尚未应用导致位置偏差
        Dispatcher.UIThread.Post(() => PositionWindowAtBottomCenter());
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
            var baseWidth = 200; // 最小宽度增加到200
            var charWidth = 15; // 每个字符约15px（更紧凑）
            var estimatedWidth = Math.Max(baseWidth, text.Length * charWidth + 60); // 60px为padding

            // 限制最大宽度为600px，避免超出屏幕
            var finalWidth = Math.Min(600, Math.Max(200, estimatedWidth));

            Width = finalWidth;

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
        Console.WriteLine("[KeyDisplayWindow] ShowWindow()");
        // 强制显示并确保不透明
        this.Show();
        this.Opacity = 1;
        this.WindowState = WindowState.Normal;
        PositionWindowAtBottomCenter();

        // 确保窗口在最顶层（不主动抢占焦点）并轻抖刷新Z序
        this.Topmost = true;
        this.Topmost = false;
        this.Topmost = true;
        Console.WriteLine($"[KeyDisplayWindow] Visible={this.IsVisible}, Opacity={this.Opacity}, Topmost={this.Topmost}, State={this.WindowState}, Size={this.Width}x{this.Height}, Position=({this.Position.X},{this.Position.Y})");
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

    /// <summary>
    /// 显示欢迎消息
    /// </summary>
    public async System.Threading.Tasks.Task ShowWelcomeMessageAsync()
    {
        if (_settings == null || !_settings.ShowWelcomeMessage || _hasShownWelcome)
            return;

        _hasShownWelcome = true;

        try
        {
            // 显示欢迎消息
            if (DisplayTextBlock != null)
            {
                DisplayTextBlock.Text = _settings.WelcomeMessage ?? "欢迎使用按键监控";
            }

            // 显示窗口
            ShowWindow();
            _isContentVisible = true;

            // 淡入动画
            await Infrastructure.Helpers.AnimationHelper.FadeIn(this, 300);

            // 等待显示时长
            var duration = (int)(_settings.WelcomeMessageDuration * 1000);
            await System.Threading.Tasks.Task.Delay(duration);

            // 淡出动画
            await Infrastructure.Helpers.AnimationHelper.FadeOut(this, 300);

            // 隐藏窗口
            HideWindow();
            _isContentVisible = false;

            // 清空文本
            if (DisplayTextBlock != null)
            {
                DisplayTextBlock.Text = "";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[KeyDisplayWindow] ShowWelcomeMessageAsync error: {ex.Message}");
        }
    }

    /// <summary>
    /// 重置欢迎消息标志
    /// </summary>
    public void ResetWelcomeMessage()
    {
        _hasShownWelcome = false;
    }

    /// <summary>
    /// 兜底：确保窗口可见并计时隐藏（用于按钮强制显示）
    /// </summary>
    public void EnsureShowVisible()
    {
        Console.WriteLine("[KeyDisplayWindow] EnsureShowVisible()");
        _isContentVisible = true;
        ShowWindow();
        StartAutoHideTimer();
    }

    /// <summary>
    /// 更新键盘监控设置
    /// </summary>
    public void UpdateSettings(Core.Configuration.KeyboardMonitorSettings settings)
    {
        if (settings == null)
        {
            return;
        }

        // 保存设置以供欢迎消息使用
        _settings = settings;

        try
        {
            Console.WriteLine($"Updating KeyDisplayWindow settings: Position={settings.DisplayPosition}, FontSize={settings.FontSize}");

            // 更新显示位置
            UpdateDisplayPosition(settings.DisplayPosition, settings.CustomDisplayX, settings.CustomDisplayY);

            // 更新自动隐藏时长（将在下次显示时生效）
            // AUTO_HIDE_DELAY 是常量，这里我们可以存储设置供后续使用

            Console.WriteLine("KeyDisplayWindow settings updated successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating KeyDisplayWindow settings: {ex.Message}");
        }
    }

    /// <summary>
    /// 更新显示位置
    /// </summary>
    private void UpdateDisplayPosition(string position, int? customX, int? customY)
    {
        var positionService = new Core.Services.WindowPositionService();
        var newPosition = positionService.CalculatePosition(position, (int)this.Width, (int)this.Height);

        // 如果是自定义位置且提供了坐标
        if (position == "Custom" && customX.HasValue && customY.HasValue)
        {
            newPosition = new PixelPoint(customX.Value, customY.Value);
        }

        this.Position = newPosition;
        Console.WriteLine($"KeyDisplayWindow positioned at: X={newPosition.X}, Y={newPosition.Y}");
    }

    protected override void OnClosed(EventArgs e)
    {
        StopAutoHideTimer();
        base.OnClosed(e);
    }
}
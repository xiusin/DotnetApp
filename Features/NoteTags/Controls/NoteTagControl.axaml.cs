using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;

namespace ConfigButtonDisplay.Features.NoteTags.Controls;

public partial class NoteTagControl : UserControl
{
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<NoteTagControl, string>(nameof(Text), "标签内容");

    public static readonly StyledProperty<IBrush> BackgroundColorProperty =
        AvaloniaProperty.Register<NoteTagControl, IBrush>(nameof(BackgroundColor), new SolidColorBrush(Color.Parse("#2C3E50")));

    public static readonly StyledProperty<IBrush> BorderColorProperty =
        AvaloniaProperty.Register<NoteTagControl, IBrush>(nameof(BorderColor), new SolidColorBrush(Color.Parse("#3498DB")));

    public static readonly StyledProperty<IBrush> ForegroundColorProperty =
        AvaloniaProperty.Register<NoteTagControl, IBrush>(nameof(ForegroundColor), new SolidColorBrush(Colors.White));

    public static readonly StyledProperty<IBrush> IconColorProperty =
        AvaloniaProperty.Register<NoteTagControl, IBrush>(nameof(IconColor), new SolidColorBrush(Color.Parse("#5DADE2")));

    // 事件定义
    public event EventHandler<PointerEventArgs>? TagPointerEnter;
    public event EventHandler<PointerEventArgs>? TagPointerLeave;
    public event EventHandler<PointerPressedEventArgs>? TagClicked;

    private const double HiddenOffsetX = 130; // 缩进距离（像素）
    private const double SlideAnimationDuration = 250; // 滑动动画时长（毫秒）
    private bool _isExpanded = false;

    public NoteTagControl()
    {
        InitializeComponent();
        SetupEvents();
        InitializeHiddenState();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void SetupEvents()
    {
        // 添加鼠标事件处理
        this.PointerEntered += OnPointerEntered;
        this.PointerExited += OnPointerExited;
        this.PointerPressed += OnPointerPressed;
    }

    /// <summary>
    /// 初始化为缩进状态
    /// </summary>
    private void InitializeHiddenState()
    {
        // 设置初始位置为缩进状态（向右偏移）
        this.RenderTransform = new TranslateTransform(HiddenOffsetX, 0);
    }

    private async void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (_isExpanded) return;
        
        _isExpanded = true;
        
        // 触发悬停进入效果
        ApplyHoverEffect(true);
        
        // 滑出动画：从缩进位置滑到完整显示
        await SlideToPosition(0, SlideAnimationDuration);
        
        // 轻微缩放效果
        await Infrastructure.Helpers.AnimationHelper.ScaleToTarget(this, 1.02, 150);
        
        TagPointerEnter?.Invoke(this, e);
    }

    private async void OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (!_isExpanded) return;
        
        _isExpanded = false;
        
        // 触发悬停离开效果
        ApplyHoverEffect(false);
        
        // 恢复原始大小
        await Infrastructure.Helpers.AnimationHelper.ScaleToTarget(this, 1.0, 150);
        
        // 滑回动画：从完整显示滑回缩进位置
        await SlideToPosition(HiddenOffsetX, SlideAnimationDuration);
        
        TagPointerLeave?.Invoke(this, e);
    }

    /// <summary>
    /// 滑动到指定位置
    /// </summary>
    private async System.Threading.Tasks.Task SlideToPosition(double targetX, double duration)
    {
        var transform = this.RenderTransform as TranslateTransform;
        if (transform == null)
        {
            transform = new TranslateTransform(HiddenOffsetX, 0);
            this.RenderTransform = transform;
        }

        var startX = transform.X;
        var distance = targetX - startX;
        var startTime = DateTime.Now;
        var durationMs = duration;

        while (true)
        {
            var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
            var progress = Math.Min(elapsed / durationMs, 1.0);
            
            // 使用 ease-out 缓动函数
            var easedProgress = 1 - Math.Pow(1 - progress, 3);
            
            transform.X = startX + distance * easedProgress;

            if (progress >= 1.0)
                break;

            await System.Threading.Tasks.Task.Delay(16); // ~60 FPS
        }

        transform.X = targetX;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        TagClicked?.Invoke(this, e);
    }

    private void ApplyHoverEffect(bool isHovered)
    {
        var border = this.FindControl<Border>("TagBorder");
        if (border != null)
        {
            if (isHovered)
            {
                // 悬停时的效果 - 撕裂便签的悬停效果
                border.Background = new SolidColorBrush(Color.Parse("#FFFFF0")); // 更亮的米黄色
                
                // 增强阴影效果，让便签看起来"浮起来"
                border.Effect = new Avalonia.Media.DropShadowEffect
                {
                    Color = Color.FromArgb(80, 0, 0, 0),
                    OffsetX = 3,
                    OffsetY = 5,
                    BlurRadius = 12
                };
            }
            else
            {
                // 恢复原始撕裂便签状态
                border.Background = new SolidColorBrush(Color.Parse("#FFFEF5")); // 米黄色便签纸
                
                // 恢复原始阴影
                border.Effect = new Avalonia.Media.DropShadowEffect
                {
                    Color = Color.FromArgb(64, 0, 0, 0),
                    OffsetX = 2,
                    OffsetY = 3,
                    BlurRadius = 8
                };
            }
        }
    }

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public IBrush BackgroundColor
    {
        get => GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    public IBrush BorderColor
    {
        get => GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }

    public IBrush ForegroundColor
    {
        get => GetValue(ForegroundColorProperty);
        set => SetValue(ForegroundColorProperty, value);
    }

    public IBrush IconColor
    {
        get => GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    // 添加缺失的方法
    public void SetText(string text)
    {
        Text = text;
        // 直接查找并更新TextBlock
        var textBlock = this.FindControl<TextBlock>("TagText");
        if (textBlock != null)
        {
            textBlock.Text = text;
        }
    }

    public void SetHoverState(bool isHovered)
    {
        ApplyHoverEffect(isHovered);
    }
    
    /// <summary>
    /// 显示标签（带滑入动画）
    /// </summary>
    public async System.Threading.Tasks.Task ShowAsync()
    {
        this.IsVisible = true;
        this.Opacity = 0;
        
        // 从更远的位置滑入到缩进位置
        var transform = new TranslateTransform(200, 0);
        this.RenderTransform = transform;
        
        // 淡入
        await Infrastructure.Helpers.AnimationHelperOptimized.FadeInOptimized(this, 200);
        
        // 滑动到缩进位置
        await SlideToPosition(HiddenOffsetX, 300);
    }
    
    /// <summary>
    /// 隐藏标签（带滑出动画）
    /// </summary>
    public async System.Threading.Tasks.Task HideAsync()
    {
        // 如果当前是展开状态，先滑回缩进位置
        if (_isExpanded)
        {
            await SlideToPosition(HiddenOffsetX, 200);
        }
        
        // 滑出到屏幕外并淡出
        var slideTask = SlideToPosition(200, 300);
        var fadeTask = Infrastructure.Helpers.AnimationHelperOptimized.FadeOutOptimized(this, 300);
        
        await System.Threading.Tasks.Task.WhenAll(slideTask, fadeTask);
        
        this.IsVisible = false;
        
        // 重置状态
        _isExpanded = false;
        this.RenderTransform = new TranslateTransform(HiddenOffsetX, 0);
        this.Opacity = 1.0;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TextProperty)
        {
            var textBlock = this.FindControl<TextBlock>("TagText");
            if (textBlock != null)
                textBlock.Text = Text;
        }
        else if (change.Property == BackgroundColorProperty)
        {
            var border = this.FindControl<Border>("TagBorder");
            if (border != null)
                border.Background = BackgroundColor;
        }
        else if (change.Property == BorderColorProperty)
        {
            var border = this.FindControl<Border>("TagBorder");
            if (border != null)
                border.BorderBrush = BorderColor;
        }
        else if (change.Property == ForegroundColorProperty)
        {
            var textBlock = this.FindControl<TextBlock>("TagText");
            if (textBlock != null)
                textBlock.Foreground = ForegroundColor;
        }
        else if (change.Property == IconColorProperty)
        {
            // 图标颜色设置暂时移除，因为我们简化了UI
            // if (TagIcon != null)
            //     TagIcon.Foreground = IconColor;
        }
    }
}
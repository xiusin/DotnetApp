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

    public NoteTagControl()
    {
        InitializeComponent();
        SetupEvents();
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

    private async void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        // 触发悬停进入效果
        ApplyHoverEffect(true);
        
        // 添加悬停缩放动画
        await Infrastructure.Helpers.AnimationHelper.ScaleToTarget(this, 1.05, 200);
        
        TagPointerEnter?.Invoke(this, e);
    }

    private async void OnPointerExited(object? sender, PointerEventArgs e)
    {
        // 触发悬停离开效果
        ApplyHoverEffect(false);
        
        // 恢复原始大小
        await Infrastructure.Helpers.AnimationHelper.ScaleToTarget(this, 1.0, 200);
        
        TagPointerLeave?.Invoke(this, e);
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
        await Infrastructure.Helpers.AnimationHelper.SlideInFromRightForControl(this, 300);
    }
    
    /// <summary>
    /// 隐藏标签（带滑出动画）
    /// </summary>
    public async System.Threading.Tasks.Task HideAsync()
    {
        await Infrastructure.Helpers.AnimationHelper.SlideOutToRight(this, 300);
        this.IsVisible = false;
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
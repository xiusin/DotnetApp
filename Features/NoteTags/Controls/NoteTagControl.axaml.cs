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
                // 悬停时的效果 - 增强圆角和视觉效果
                border.Background = new SolidColorBrush(Color.Parse("#FFFEF7")); // 更亮的背景色
                border.BorderBrush = new SolidColorBrush(Color.Parse("#FFC107"));
                border.BorderThickness = new Thickness(2); // 增加边框厚度
                border.CornerRadius = new CornerRadius(15); // 增大圆角半径
                
                // 添加缩放效果
                var scaleTransform = new ScaleTransform(1.03, 1.03); // 稍微缩放，更自然
                border.RenderTransform = scaleTransform;
                border.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
                
                // 添加阴影效果
                border.BoxShadow = new BoxShadows(new BoxShadow
                {
                    OffsetX = 0,
                    OffsetY = 4,
                    Blur = 12, // 增加模糊半径
                    Color = Color.FromUInt32(0x40000000),
                    Spread = 2 // 添加扩散效果
                });
            }
            else
            {
                // 恢复原始便签状态
                border.Background = new SolidColorBrush(Color.Parse("White")); // 改为白色背景
                border.BorderBrush = new SolidColorBrush(Color.Parse("#FFD54F"));
                border.BorderThickness = new Thickness(1); // 恢复原始边框厚度
                border.CornerRadius = new CornerRadius(12); // 恢复原始圆角
                border.RenderTransform = null;
                border.BoxShadow = new BoxShadows(); // 移除阴影
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
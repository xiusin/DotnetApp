using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using Avalonia.Animation;

namespace ConfigButtonDisplay.NoteTagComponent;

public class NoteTagManager
{
    private readonly Window _hostWindow;
    private NoteTagControl[] _tags = new NoteTagControl[3];
    private Window[] _tagWindows = new Window[3];
    private PixelPoint[] _positions = new PixelPoint[3];  // 显示位置
    private PixelPoint[] _hiddenPositions = new PixelPoint[3]; // 隐藏位置
    private bool _isAnimating = false;
    private DispatcherTimer? _hoverTimer;
    private const int SLIDE_DISTANCE = 100; // 滑出距离
    private const int ANIMATION_DURATION = 300; // 动画持续时间（毫秒）
    private const int HOVER_DELAY = 200; // 悬停延迟（毫秒）
    private int _debugCounter = 0; // 调试计数器
    private bool[] _tagHovered = new bool[3]; // 记录每个标签的悬停状态
    private bool[] _tagSlidOut = new bool[3]; // 记录每个标签是否已滑出

    public NoteTagManager(Window hostWindow)
    {
        _hostWindow = hostWindow;
        System.Console.WriteLine($"[NoteTagManager] 初始化开始，主机窗口: {hostWindow?.GetType().Name}");
        InitializeTags();
        SetupHoverDetection();
        System.Console.WriteLine($"[NoteTagManager] 初始化完成");
    }

    private void InitializeTags()
    {
        System.Console.WriteLine($"[NoteTagManager] 开始创建标签...");
        
        try
        {
            // 创建三个具有圆角断边效果的标签
            for (int i = 0; i < 3; i++)
            {
                System.Console.WriteLine($"[NoteTagManager] 创建标签 {i + 1}...");
                
                // 创建NoteTagControl实例
                var noteTagControl = new NoteTagControl();
                noteTagControl.SetText($"便签 {i + 1}");
                _tags[i] = noteTagControl;
                
                // 为每个便签创建独立的窗口
                var window = new Window
                {
                    Width = 170,
                    Height = 90,
                    SystemDecorations = SystemDecorations.None,
                    ShowInTaskbar = false,
                    Topmost = true,
                    CanResize = false,
                    Background = Brushes.Transparent,
                    Content = noteTagControl,
                    IsVisible = false // 初始隐藏，等待ShowTags调用
                };

                _tagWindows[i] = window;
                
                System.Console.WriteLine($"[NoteTagManager] 标签 {i + 1} 窗口创建完成");
            }
            
            System.Console.WriteLine($"[NoteTagManager] 所有标签窗口创建完成");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[NoteTagManager] 创建标签失败: {ex.Message}");
            System.Console.WriteLine($"[NoteTagManager] 异常堆栈: {ex.StackTrace}");
        }
    }

    private void SetupHoverDetection()
    {
        // 设置鼠标移动事件监听
        if (_hostWindow is Window window)
        {
            window.PointerMoved += OnHostWindowPointerMoved;
        }

        // 初始化悬停计时器
        _hoverTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(HOVER_DELAY)
        };
        _hoverTimer.Tick += OnHoverTimerTick;

        // 为每个标签窗口设置鼠标事件
        for (int i = 0; i < _tagWindows.Length; i++)
        {
            if (_tagWindows[i] != null)
            {
                int tagIndex = i; // 捕获循环变量
                _tagWindows[i].PointerEntered += (s, e) => OnTagPointerEnter(tagIndex);
                _tagWindows[i].PointerExited += (s, e) => OnTagPointerLeave(tagIndex);
            }
        }
    }

    private void OnHostWindowPointerMoved(object? sender, PointerEventArgs e)
    {
        var screens = _hostWindow.Screens?.All;
        if (screens == null || screens.Count == 0) return;

        var primaryScreen = screens[0];
        var screenBounds = primaryScreen.Bounds;
        var mousePosition = e.GetPosition(_hostWindow);
        
        // 转换为屏幕坐标
        var windowPosition = _hostWindow.Position;
        var screenMouseX = windowPosition.X + mousePosition.X;
        var screenMouseY = windowPosition.Y + mousePosition.Y;

        // 检测鼠标是否在标签区域附近（更智能的检测）
        bool isNearAnyTag = false;
        int hoveredTagIndex = -1;
        
        // 检查鼠标是否靠近任何标签的当前位置
        for (int i = 0; i < _tagWindows.Length; i++)
        {
            if (_tagWindows[i] != null && _tagWindows[i].IsVisible)
            {
                var tagPos = _tagWindows[i].Position;
                var windowWidth = 170; // 窗口宽度
                var windowHeight = 90; // 窗口高度
                
                // 检测范围：只检测标签的可见部分（10px + 一些扩展区域）
                // 标签当前位置可能只显示了左侧10px
                var visibleLeft = tagPos.X; // 可见区域的左边界
                var visibleRight = Math.Min(tagPos.X + 20, tagPos.X + windowWidth); // 可见区域的右边界（10px显示 + 10px扩展）
                var visibleTop = tagPos.Y - 10; // 稍微扩大顶部检测范围
                var visibleBottom = tagPos.Y + windowHeight + 10; // 稍微扩大底部检测范围
                
                if (screenMouseX >= visibleLeft && screenMouseX <= visibleRight && 
                    screenMouseY >= visibleTop && screenMouseY <= visibleBottom)
                {
                    isNearAnyTag = true;
                    hoveredTagIndex = i;
                    break;
                }
            }
        }
        
        // 调试输出（每10次输出一次，避免过多日志）
        _debugCounter++;
        if (_debugCounter % 10 == 0)
        {
            System.Console.WriteLine($"[NoteTagManager] 鼠标移动: 屏幕坐标({screenMouseX}, {screenMouseY}), 靠近标签: {isNearAnyTag}, 动画状态: {_isAnimating}");
        }
        
        // 简化鼠标移动逻辑 - 主要用于调试输出
        // 实际的标签滑出/收回和焦点效果由标签窗口的 PointerEntered/PointerExited 事件处理
        if (_debugCounter % 10 == 0 && isNearAnyTag)
        {
            System.Console.WriteLine($"[NoteTagManager] 鼠标靠近标签区域: 标签索引 {hoveredTagIndex}");
        }
    }

    private void OnHoverTimerTick(object? sender, EventArgs e)
    {
        // 计时器逻辑现在由鼠标移动事件处理，这里不再需要
        _hoverTimer?.Stop();
        // 移除 SlideTagsIn() 调用，避免所有标签一起滑出
    }

    private async void SlideTagIn(int tagIndex)
    {
        if (_isAnimating || tagIndex < 0 || tagIndex >= _tagWindows.Length) return;
        if (_tagWindows[tagIndex] == null || _tagSlidOut[tagIndex]) return; // 如果已经滑出，不再重复滑出
        
        _isAnimating = true;

        try
        {
            System.Console.WriteLine($"[NoteTagManager] 开始滑出标签 {tagIndex + 1}");
            
            // 只滑出指定的标签
            await AnimateTagSlideWithBounce(_tagWindows[tagIndex], _hiddenPositions[tagIndex], _positions[tagIndex], 400);
            _tagSlidOut[tagIndex] = true; // 标记为已滑出
            
            System.Console.WriteLine($"[NoteTagManager] 标签 {tagIndex + 1} 滑出完成");
        }
        finally
        {
            _isAnimating = false;
        }
    }

    private async void SlideTagOut(int tagIndex)
    {
        if (_isAnimating || tagIndex < 0 || tagIndex >= _tagWindows.Length) return;
        if (_tagWindows[tagIndex] == null || !_tagSlidOut[tagIndex]) return; // 如果已经收回，不再重复收回
        
        _isAnimating = true;

        try
        {
            System.Console.WriteLine($"[NoteTagManager] 开始收回标签 {tagIndex + 1}");
            
            // 只收回指定的标签
            await AnimateTagSlide(_tagWindows[tagIndex], _positions[tagIndex], _hiddenPositions[tagIndex], 300);
            _tagSlidOut[tagIndex] = false; // 标记为已收回
            
            System.Console.WriteLine($"[NoteTagManager] 标签 {tagIndex + 1} 收回完成");
        }
        finally
        {
            _isAnimating = false;
        }
    }

    private async void SlideTagsIn()
    {
        if (_isAnimating) return;
        _isAnimating = true;

        try
        {
            System.Console.WriteLine($"[NoteTagManager] 开始抽出动画");
            
            // 并行启动所有标签的动画，但添加递增延迟创造波浪效果
            var animationTasks = new Task[_tagWindows.Length];
            for (int i = 0; i < _tagWindows.Length; i++)
            {
                if (_tagWindows[i] != null)
                {
                    int index = i;
                    animationTasks[i] = Task.Run(async () =>
                    {
                        await Task.Delay(index * 100); // 递增延迟
                        await AnimateTagSlideWithBounce(_tagWindows[index], _hiddenPositions[index], _positions[index], 400);
                    });
                }
            }
            
            // 等待所有动画完成
            await Task.WhenAll(animationTasks.Where(t => t != null)!);
            
            System.Console.WriteLine($"[NoteTagManager] 抽出动画完成");
        }
        finally
        {
            _isAnimating = false;
        }
    }

    private async void SlideTagsOut()
    {
        if (_isAnimating) return;
        _isAnimating = true;

        try
        {
            System.Console.WriteLine($"[NoteTagManager] 开始收回所有滑出的标签");
            
            // 收回所有当前已滑出的标签
            var animationTasks = new List<Task>();
            for (int i = 0; i < _tagWindows.Length; i++)
            {
                if (_tagWindows[i] != null && _tagSlidOut[i])
                {
                    int index = i;
                    animationTasks.Add(Task.Run(async () =>
                    {
                        await AnimateTagSlide(_tagWindows[index], _positions[index], _hiddenPositions[index], 300);
                        _tagSlidOut[index] = false; // 标记为已收回
                    }));
                }
            }
            
            // 等待所有动画完成  
            if (animationTasks.Count > 0)
            {
                await Task.WhenAll(animationTasks);
            }
            
            System.Console.WriteLine($"[NoteTagManager] 收回动画完成");
        }
        finally
        {
            _isAnimating = false;
        }
    }

    private async Task AnimateTagSlide(Window window, PixelPoint startPos, PixelPoint endPos, int duration)
    {
        var startTime = DateTime.Now;
        var endTime = startTime.AddMilliseconds(duration);

        while (DateTime.Now < endTime)
        {
            var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
            var progress = Math.Min(elapsed / duration, 1.0);
            
            // 使用缓动函数（ease-out）让动画更自然
            var easedProgress = 1 - Math.Pow(1 - progress, 3);
            
            var currentX = (int)(startPos.X + (endPos.X - startPos.X) * easedProgress);
            var currentY = (int)(startPos.Y + (endPos.Y - startPos.Y) * easedProgress);
            
            window.Position = new PixelPoint(currentX, currentY);
            
            await Task.Delay(16); // 约60fps
        }

        window.Position = endPos;
    }

    // 新增：带弹跳效果的滑动动画
    private async Task AnimateTagSlideWithBounce(Window window, PixelPoint startPos, PixelPoint endPos, int duration)
    {
        var startTime = DateTime.Now;
        var endTime = startTime.AddMilliseconds(duration);

        while (DateTime.Now < endTime)
        {
            var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
            var progress = Math.Min(elapsed / duration, 1.0);
            
            // 使用弹跳缓动函数
            var easedProgress = BounceEaseOut(progress);
            
            var currentX = (int)(startPos.X + (endPos.X - startPos.X) * easedProgress);
            var currentY = (int)(startPos.Y + (endPos.Y - startPos.Y) * easedProgress);
            
            window.Position = new PixelPoint(currentX, currentY);
            
            await Task.Delay(16); // 约60fps
        }

        window.Position = endPos;
    }

    // 弹跳缓动函数
    private double BounceEaseOut(double t)
    {
        if (t < 1 / 2.75)
        {
            return 7.5625 * t * t;
        }
        else if (t < 2 / 2.75)
        {
            return 7.5625 * (t -= 1.5 / 2.75) * t + 0.75;
        }
        else if (t < 2.5 / 2.75)
        {
            return 7.5625 * (t -= 2.25 / 2.75) * t + 0.9375;
        }
        else
        {
            return 7.5625 * (t -= 2.625 / 2.75) * t + 0.984375;
        }
    }

    public void ShowTags()
    {
        System.Console.WriteLine($"[NoteTagManager] ShowTags() 被调用");
        
        try
        {
            // 获取屏幕信息
            var screens = _hostWindow.Screens?.All;
            if (screens == null || screens.Count == 0)
            {
                System.Console.WriteLine($"[NoteTagManager] 错误：无法获取屏幕信息");
                return;
            }

            var primaryScreen = screens[0];
            var screenBounds = primaryScreen.Bounds;
            System.Console.WriteLine($"[NoteTagManager] 屏幕信息: 左上角({screenBounds.X}, {screenBounds.Y}), 尺寸({screenBounds.Width}x{screenBounds.Height})");

            // 计算便签位置 - 将便签垂直排列在屏幕左侧，只显示10px边缘
            var totalHeight = 3 * 90 + 2 * 10; // 3个便签窗口 + 间距
            var startY = screenBounds.Y + (screenBounds.Height - totalHeight) / 2;
            
            for (int i = 0; i < _tagWindows.Length; i++)
            {
                // 初始位置：只显示10px在屏幕内（其余160px隐藏在屏幕外）
                _hiddenPositions[i] = new PixelPoint(screenBounds.X - 150, startY + i * 100);
                // 完全显示位置：完整显示便签
                _positions[i] = new PixelPoint(screenBounds.X + 10, startY + i * 100);
                
                var window = _tagWindows[i];
                if (window != null)
                {
                    window.Position = _hiddenPositions[i]; // 初始位置为只显示10px
                    window.IsVisible = true;
                    System.Console.WriteLine($"[NoteTagManager] 初始化标签 {i + 1} 在部分显示位置 ({_hiddenPositions[i].X}, {_hiddenPositions[i].Y}) - 显示10px");
                }
            }
            
            System.Console.WriteLine($"[NoteTagManager] ShowTags() 完成");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[NoteTagManager] ShowTags() 异常: {ex.Message}");
            System.Console.WriteLine($"[NoteTagManager] 异常堆栈: {ex.StackTrace}");
        }
    }

    // 新方法：强制显示标签（用于调试）
    public void ForceShowTags()
    {
        for (int i = 0; i < _tagWindows.Length; i++)
        {
            if (_tagWindows[i] != null)
            {
                _tagWindows[i].Position = _positions[i];
                _tagWindows[i].IsVisible = true;
                System.Console.WriteLine($"[NoteTagManager] 强制显示标签 {i + 1}，位置: ({_positions[i].X}, {_positions[i].Y})");
            }
        }
    }

    // 新方法：强制隐藏标签
    public void ForceHideTags()
    {
        for (int i = 0; i < _tagWindows.Length; i++)
        {
            if (_tagWindows[i] != null)
            {
                _tagWindows[i].Position = _hiddenPositions[i];
                System.Console.WriteLine($"[NoteTagManager] 强制隐藏标签 {i + 1}，位置: ({_hiddenPositions[i].X}, {_hiddenPositions[i].Y})");
            }
        }
    }

    // 新方法：获取标签状态信息
    public string GetTagStatus()
    {
        var status = new System.Text.StringBuilder();
        for (int i = 0; i < _tagWindows.Length; i++)
        {
            if (_tagWindows[i] != null)
            {
                status.AppendLine($"标签 {i + 1}: 可见={_tagWindows[i].IsVisible}, 位置=({_tagWindows[i].Position.X}, {_tagWindows[i].Position.Y})");
            }
        }
        return status.ToString();
    }

    public void HideTags()
    {
        for (int i = 0; i < _tagWindows.Length; i++)
        {
            if (_tagWindows[i].IsVisible)
            {
                _tagWindows[i].Hide();
            }
        }
    }

    public void SetTagText(int index, string text)
    {
        System.Console.WriteLine($"[NoteTagManager] SetTagText({index}, \"{text}\") 被调用");
        if (index >= 0 && index < _tags.Length && _tags[index] != null)
        {
            _tags[index].SetText(text);
            System.Console.WriteLine($"[NoteTagManager] 标签 {index} 文本设置为: {text}");
        }
        else
        {
            System.Console.WriteLine($"[NoteTagManager] 无法设置标签 {index} 文本，标签为空或索引超出范围");
        }
    }

    public void SetTagColors(int index, IBrush background, IBrush border, IBrush foreground, IBrush icon)
    {
        if (index >= 0 && index < _tags.Length)
        {
            _tags[index].BackgroundColor = background;
            _tags[index].BorderColor = border;
            _tags[index].ForegroundColor = foreground;
            _tags[index].IconColor = icon;
        }
    }

    private void OnTagPointerEnter(int tagIndex)
    {
        if (tagIndex >= 0 && tagIndex < _tags.Length && _tags[tagIndex] != null && !_isAnimating)
        {
            _tagHovered[tagIndex] = true;
            System.Console.WriteLine($"[NoteTagManager] 鼠标进入标签 {tagIndex + 1}");
            
            // 标签悬停效果（圆角效果）
            _tags[tagIndex].SetHoverState(true);
            
            // 滑出标签
            SlideTagIn(tagIndex);
        }
    }

    private void OnTagPointerLeave(int tagIndex)
    {
        if (tagIndex >= 0 && tagIndex < _tags.Length && _tags[tagIndex] != null && !_isAnimating)
        {
            _tagHovered[tagIndex] = false;
            System.Console.WriteLine($"[NoteTagManager] 鼠标离开标签 {tagIndex + 1}");
            
            // 恢复标签原始状态（移除圆角效果）
            _tags[tagIndex].SetHoverState(false);
            
            // 收回标签
            SlideTagOut(tagIndex);
        }
    }

    public void Dispose()
    {
        // 移除事件监听
        if (_hostWindow is Window window)
        {
            window.PointerMoved -= OnHostWindowPointerMoved;
        }

        // 停止计时器
        if (_hoverTimer != null)
        {
            _hoverTimer.Stop();
            _hoverTimer.Tick -= OnHoverTimerTick;
        }

        // 关闭所有窗口
        for (int i = 0; i < _tagWindows.Length; i++)
        {
            _tagWindows[i]?.Close();
        }
    }
}
using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;

namespace ConfigButtonDisplay.Infrastructure.Helpers;

/// <summary>
/// 动画辅助类，提供常用的动画效果
/// </summary>
public static class AnimationHelper
{
    /// <summary>
    /// 从右侧滑入动画
    /// </summary>
    /// <param name="window">窗口对象</param>
    /// <param name="duration">动画时长（毫秒），默认 300ms</param>
    public static async Task SlideInFromRight(Window window, int duration = 300)
    {
        if (window == null)
        {
            return;
        }

        try
        {
            // 保存目标位置
            var targetPosition = window.Position;
            
            // 设置起始位置（向右偏移 300px）
            var startPosition = new PixelPoint(targetPosition.X + 300, targetPosition.Y);
            window.Position = startPosition;
            
            // 确保窗口可见
            window.Opacity = 0;
            await Task.Delay(10);  // 短暂延迟确保位置已应用
            window.Opacity = 1;
            
            // 执行滑入动画
            var startTime = DateTime.Now;
            var totalDuration = TimeSpan.FromMilliseconds(duration);
            
            while (DateTime.Now - startTime < totalDuration)
            {
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                var progress = Math.Min(1.0, elapsed / duration);
                
                // 使用 ease-out 缓动函数
                var easedProgress = EaseOut(progress);
                
                // 计算当前位置
                var currentX = (int)(startPosition.X + (targetPosition.X - startPosition.X) * easedProgress);
                
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    window.Position = new PixelPoint(currentX, targetPosition.Y);
                });
                
                await Task.Delay(16);  // 约 60 FPS
            }
            
            // 确保最终位置准确
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                window.Position = targetPosition;
            });
            
            Console.WriteLine($"Slide-in animation completed: X={targetPosition.X}, Y={targetPosition.Y}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in slide-in animation: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Ease-out 缓动函数
    /// </summary>
    private static double EaseOut(double t)
    {
        return 1 - Math.Pow(1 - t, 3);
    }
    
    /// <summary>
    /// 悬停缩放动画
    /// </summary>
    /// <param name="control">控件对象</param>
    /// <param name="scale">缩放比例，默认 1.02</param>
    /// <param name="duration">动画时长（毫秒），默认 200ms</param>
    public static async Task ScaleOnHover(Control control, double scale = 1.02, int duration = 200)
    {
        if (control == null)
        {
            return;
        }

        try
        {
            var startTime = DateTime.Now;
            var totalDuration = TimeSpan.FromMilliseconds(duration);
            
            while (DateTime.Now - startTime < totalDuration)
            {
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                var progress = Math.Min(1.0, elapsed / duration);
                
                // 使用 ease-in-out 缓动函数
                var easedProgress = EaseInOut(progress);
                
                // 计算当前缩放值
                var currentScale = 1.0 + (scale - 1.0) * easedProgress;
                
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    control.RenderTransform = new ScaleTransform(currentScale, currentScale);
                });
                
                await Task.Delay(16);  // 约 60 FPS
            }
            
            // 确保最终缩放值准确
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                control.RenderTransform = new ScaleTransform(scale, scale);
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in scale animation: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Ease-in-out 缓动函数
    /// </summary>
    private static double EaseInOut(double t)
    {
        return t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;
    }
    
    /// <summary>
    /// 淡入动画
    /// </summary>
    /// <param name="control">控件对象</param>
    /// <param name="duration">动画时长（毫秒），默认 200ms</param>
    public static async Task FadeIn(Control control, int duration = 200)
    {
        if (control == null)
        {
            return;
        }

        try
        {
            control.Opacity = 0;
            
            var startTime = DateTime.Now;
            var totalDuration = TimeSpan.FromMilliseconds(duration);
            
            while (DateTime.Now - startTime < totalDuration)
            {
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                var progress = Math.Min(1.0, elapsed / duration);
                
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    control.Opacity = progress;
                });
                
                await Task.Delay(16);  // 约 60 FPS
            }
            
            // 确保最终透明度为 1
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                control.Opacity = 1;
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in fade-in animation: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 淡出动画
    /// </summary>
    /// <param name="control">控件对象</param>
    /// <param name="duration">动画时长（毫秒），默认 300ms</param>
    public static async Task FadeOut(Control control, int duration = 300)
    {
        if (control == null)
        {
            return;
        }

        try
        {
            var startOpacity = control.Opacity;
            
            var startTime = DateTime.Now;
            var totalDuration = TimeSpan.FromMilliseconds(duration);
            
            while (DateTime.Now - startTime < totalDuration)
            {
                var elapsed = (DateTime.Now - startTime).TotalMilliseconds;
                var progress = Math.Min(1.0, elapsed / duration);
                
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    control.Opacity = startOpacity * (1 - progress);
                });
                
                await Task.Delay(16);  // 约 60 FPS
            }
            
            // 确保最终透明度为 0
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                control.Opacity = 0;
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in fade-out animation: {ex.Message}");
        }
    }
}

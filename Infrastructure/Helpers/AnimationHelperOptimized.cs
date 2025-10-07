using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace ConfigButtonDisplay.Infrastructure.Helpers;

/// <summary>
/// 优化的动画辅助类 - 减少内存分配和提高性能
/// </summary>
public static class AnimationHelperOptimized
{
    private const int TargetFps = 60;
    private const int FrameDelayMs = 16; // 约 60 FPS
    
    /// <summary>
    /// 优化的淡入动画 - 使用 UtcNow 减少开销
    /// </summary>
    public static async Task FadeInOptimized(Control control, int duration = 200)
    {
        if (control == null) return;

        try
        {
            control.Opacity = 0;
            var startTime = DateTime.UtcNow;
            var durationMs = (double)duration;
            
            while (true)
            {
                var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
                if (elapsed >= durationMs)
                {
                    control.Opacity = 1;
                    break;
                }
                
                var progress = elapsed / durationMs;
                control.Opacity = progress;
                await Task.Delay(FrameDelayMs);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in fade-in animation: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 优化的淡出动画
    /// </summary>
    public static async Task FadeOutOptimized(Control control, int duration = 300)
    {
        if (control == null) return;

        try
        {
            var startOpacity = control.Opacity;
            var startTime = DateTime.UtcNow;
            var durationMs = (double)duration;
            
            while (true)
            {
                var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
                if (elapsed >= durationMs)
                {
                    control.Opacity = 0;
                    break;
                }
                
                var progress = elapsed / durationMs;
                control.Opacity = startOpacity * (1 - progress);
                await Task.Delay(FrameDelayMs);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in fade-out animation: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 优化的缩放动画 - 减少对象分配
    /// </summary>
    public static async Task ScaleOptimized(Control control, double targetScale, int duration = 200)
    {
        if (control == null) return;

        try
        {
            var currentTransform = control.RenderTransform as ScaleTransform;
            var startScale = currentTransform?.ScaleX ?? 1.0;
            var startTime = DateTime.UtcNow;
            var durationMs = (double)duration;
            var deltaScale = targetScale - startScale;
            
            // 重用 ScaleTransform 对象
            var scaleTransform = currentTransform ?? new ScaleTransform();
            control.RenderTransform = scaleTransform;
            control.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
            
            while (true)
            {
                var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
                if (elapsed >= durationMs)
                {
                    scaleTransform.ScaleX = targetScale;
                    scaleTransform.ScaleY = targetScale;
                    break;
                }
                
                var progress = elapsed / durationMs;
                var easedProgress = EaseInOut(progress);
                var currentScale = startScale + deltaScale * easedProgress;
                
                scaleTransform.ScaleX = currentScale;
                scaleTransform.ScaleY = currentScale;
                
                await Task.Delay(FrameDelayMs);
            }
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
}

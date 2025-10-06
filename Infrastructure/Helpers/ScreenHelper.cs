using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;

namespace ConfigButtonDisplay.Infrastructure.Helpers;

/// <summary>
/// 屏幕辅助类，提供屏幕相关的工具方法
/// </summary>
public static class ScreenHelper
{
    /// <summary>
    /// 获取主屏幕
    /// </summary>
    /// <returns>主屏幕对象，如果没有则返回 null</returns>
    public static Screen? GetPrimaryScreen()
    {
        var screens = GetScreens();
        return screens?.Primary;
    }
    
    /// <summary>
    /// 获取所有屏幕
    /// </summary>
    /// <returns>屏幕集合</returns>
    public static Screens? GetScreens()
    {
        // 尝试从当前窗口获取屏幕信息
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = desktop.MainWindow;
            if (mainWindow?.Screens != null)
            {
                return mainWindow.Screens;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 确保位置在屏幕范围内
    /// </summary>
    /// <param name="position">原始位置</param>
    /// <param name="windowWidth">窗口宽度</param>
    /// <param name="windowHeight">窗口高度</param>
    /// <returns>调整后的位置</returns>
    public static PixelPoint EnsureOnScreen(PixelPoint position, int windowWidth, int windowHeight)
    {
        var screen = GetPrimaryScreen();
        if (screen == null)
        {
            return position;
        }
        
        var bounds = screen.WorkingArea;
        
        // 确保 X 坐标在屏幕范围内
        var x = Math.Max(bounds.X, Math.Min(position.X, bounds.Right - windowWidth));
        
        // 确保 Y 坐标在屏幕范围内
        var y = Math.Max(bounds.Y, Math.Min(position.Y, bounds.Bottom - windowHeight));
        
        return new PixelPoint(x, y);
    }
}

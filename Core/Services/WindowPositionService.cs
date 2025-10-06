using System;
using Avalonia;
using Avalonia.Controls;
using ConfigButtonDisplay.Core.Interfaces;
using ConfigButtonDisplay.Infrastructure.Helpers;

namespace ConfigButtonDisplay.Core.Services;

/// <summary>
/// 窗口定位服务实现
/// </summary>
public class WindowPositionService : IWindowPositionService
{
    private PixelPoint? _savedPosition;
    
    /// <summary>
    /// 计算右侧边缘位置
    /// </summary>
    public PixelPoint CalculateRightEdgePosition(int windowWidth, int windowHeight)
    {
        var screen = ScreenHelper.GetPrimaryScreen();
        if (screen == null)
        {
            // 如果无法获取屏幕信息，返回默认位置
            return new PixelPoint(100, 100);
        }
        
        var workingArea = screen.WorkingArea;
        
        // 计算右侧边缘位置：屏幕右边缘 - 窗口宽度 - 边距
        var margin = 16;
        var x = Math.Max(0, workingArea.Right - windowWidth - margin);
        
        // 垂直居中
        var y = Math.Max(0, workingArea.Y + (workingArea.Height - windowHeight) / 2);
        
        return new PixelPoint(x, y);
    }
    
    /// <summary>
    /// 根据位置名称计算窗口位置
    /// </summary>
    public PixelPoint CalculatePosition(string position, int width, int height)
    {
        var screen = ScreenHelper.GetPrimaryScreen();
        if (screen == null)
        {
            return new PixelPoint(100, 100);
        }
        
        var workingArea = screen.WorkingArea;
        var margin = 50;
        
        return position switch
        {
            "RightEdge" => CalculateRightEdgePosition(width, height),
            
            "LeftEdge" => new PixelPoint(
                workingArea.X + margin,
                workingArea.Y + (workingArea.Height - height) / 2
            ),
            
            "TopLeft" => new PixelPoint(
                workingArea.X + margin,
                workingArea.Y + margin
            ),
            
            "TopCenter" => new PixelPoint(
                workingArea.X + (workingArea.Width - width) / 2,
                workingArea.Y + margin
            ),
            
            "TopRight" => new PixelPoint(
                workingArea.Right - width - margin,
                workingArea.Y + margin
            ),
            
            "BottomLeft" => new PixelPoint(
                workingArea.X + margin,
                workingArea.Bottom - height - margin
            ),
            
            "BottomCenter" => new PixelPoint(
                workingArea.X + (workingArea.Width - width) / 2,
                workingArea.Bottom - height - 60  // 距离底部 60px
            ),
            
            "BottomRight" => new PixelPoint(
                workingArea.Right - width - margin,
                workingArea.Bottom - height - margin
            ),
            
            _ => CalculateRightEdgePosition(width, height)  // 默认使用右侧边缘
        };
    }
    
    /// <summary>
    /// 保存窗口位置
    /// </summary>
    public void SavePosition(Window window)
    {
        if (window == null)
        {
            return;
        }
        
        _savedPosition = window.Position;
        Console.WriteLine($"窗口位置已保存: X={_savedPosition.Value.X}, Y={_savedPosition.Value.Y}");
    }
    
    /// <summary>
    /// 加载保存的窗口位置
    /// </summary>
    public PixelPoint? LoadSavedPosition()
    {
        return _savedPosition;
    }
}

using Avalonia;
using Avalonia.Controls;

namespace ConfigButtonDisplay.Core.Interfaces;

/// <summary>
/// 窗口定位服务接口
/// </summary>
public interface IWindowPositionService
{
    /// <summary>
    /// 计算右侧边缘位置
    /// </summary>
    /// <param name="windowWidth">窗口宽度</param>
    /// <param name="windowHeight">窗口高度</param>
    /// <returns>窗口位置</returns>
    PixelPoint CalculateRightEdgePosition(int windowWidth, int windowHeight);
    
    /// <summary>
    /// 根据位置名称计算窗口位置
    /// </summary>
    /// <param name="position">位置名称（RightEdge, LeftEdge, TopLeft, BottomCenter 等）</param>
    /// <param name="width">窗口宽度</param>
    /// <param name="height">窗口高度</param>
    /// <returns>窗口位置</returns>
    PixelPoint CalculatePosition(string position, int width, int height);
    
    /// <summary>
    /// 保存窗口位置
    /// </summary>
    /// <param name="window">窗口对象</param>
    void SavePosition(Window window);
    
    /// <summary>
    /// 加载保存的窗口位置
    /// </summary>
    /// <returns>保存的位置，如果没有则返回 null</returns>
    PixelPoint? LoadSavedPosition();
}

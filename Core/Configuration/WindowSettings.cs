namespace ConfigButtonDisplay.Core.Configuration;

/// <summary>
/// 窗口配置
/// </summary>
public class WindowSettings
{
    /// <summary>
    /// 窗口位置：RightEdge（右侧边缘）、LeftEdge（左侧边缘）、Custom（自定义）
    /// </summary>
    public string Position { get; set; } = "RightEdge";
    
    /// <summary>
    /// 自定义 X 坐标（当 Position 为 Custom 时使用）
    /// </summary>
    public int? CustomX { get; set; }
    
    /// <summary>
    /// 自定义 Y 坐标（当 Position 为 Custom 时使用）
    /// </summary>
    public int? CustomY { get; set; }
    
    /// <summary>
    /// 是否记住窗口位置
    /// </summary>
    public bool RememberPosition { get; set; } = true;
    
    /// <summary>
    /// 窗口透明度（0.0 - 1.0）
    /// </summary>
    public double Opacity { get; set; } = 0.95;
    
    /// <summary>
    /// 是否始终置顶
    /// </summary>
    public bool AlwaysOnTop { get; set; } = true;
}

namespace ConfigButtonDisplay.Core.Configuration;

/// <summary>
/// 键盘监控配置
/// </summary>
public class KeyboardMonitorSettings
{
    /// <summary>
    /// 是否启用键盘监控
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// 显示位置：TopLeft、TopCenter、TopRight、BottomLeft、BottomCenter、BottomRight、Custom
    /// </summary>
    public string DisplayPosition { get; set; } = "BottomCenter";
    
    /// <summary>
    /// 自定义显示位置 X 坐标
    /// </summary>
    public int? CustomDisplayX { get; set; }
    
    /// <summary>
    /// 自定义显示位置 Y 坐标
    /// </summary>
    public int? CustomDisplayY { get; set; }
    
    /// <summary>
    /// 背景颜色（Hex 格式，如 #3182CE）
    /// </summary>
    public string BackgroundColor { get; set; } = "#3182CE";
    
    /// <summary>
    /// 背景透明度（0.0 - 1.0）
    /// </summary>
    public double Opacity { get; set; } = 0.9;
    
    /// <summary>
    /// 字体大小（12 - 48）
    /// </summary>
    public int FontSize { get; set; } = 28;
    
    /// <summary>
    /// 字体颜色（Hex 格式，如 #FFFFFF）
    /// </summary>
    public string FontColor { get; set; } = "#FFFFFF";
    
    /// <summary>
    /// 显示时长（秒，1.0 - 10.0）
    /// </summary>
    public double DisplayDuration { get; set; } = 2.0;
    
    /// <summary>
    /// 淡入动画时长（秒，0.1 - 1.0）
    /// </summary>
    public double FadeInDuration { get; set; } = 0.2;
    
    /// <summary>
    /// 淡出动画时长（秒，0.1 - 1.0）
    /// </summary>
    public double FadeOutDuration { get; set; } = 0.3;
    
    /// <summary>
    /// 是否显示修饰键（Ctrl、Alt、Shift、Win）
    /// </summary>
    public bool ShowModifiers { get; set; } = true;
    
    /// <summary>
    /// 是否显示功能键（F1-F12）
    /// </summary>
    public bool ShowFunctionKeys { get; set; } = true;
    
    /// <summary>
    /// 是否显示字母数字键（A-Z、0-9）
    /// </summary>
    public bool ShowAlphaNumeric { get; set; } = true;
    
    /// <summary>
    /// 是否显示导航键（方向键、Home、End 等）
    /// </summary>
    public bool ShowNavigation { get; set; } = true;
}

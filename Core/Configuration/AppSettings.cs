using System.Collections.Generic;

namespace ConfigButtonDisplay.Core.Configuration;

/// <summary>
/// 应用程序配置根对象
/// </summary>
public class AppSettings
{
    /// <summary>
    /// 配置文件版本号，用于配置迁移
    /// </summary>
    public int Version { get; set; } = 1;
    
    /// <summary>
    /// 窗口配置
    /// </summary>
    public WindowSettings Window { get; set; } = new();
    
    /// <summary>
    /// 键盘监控配置
    /// </summary>
    public KeyboardMonitorSettings KeyboardMonitor { get; set; } = new();
    
    /// <summary>
    /// 其他模块配置（动态扩展）
    /// </summary>
    public Dictionary<string, object> Modules { get; set; } = new();
}

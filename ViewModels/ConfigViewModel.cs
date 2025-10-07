using System;
using System.Threading.Tasks;
using ConfigButtonDisplay.Core.Configuration;
using ConfigButtonDisplay.Core.Interfaces;

namespace ConfigButtonDisplay.ViewModels;

/// <summary>
/// 配置窗口 ViewModel
/// </summary>
public class ConfigViewModel : ViewModelBase
{
    private readonly IConfigurationService _configurationService;
    private AppSettings _appSettings;

    public ConfigViewModel(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
        _appSettings = new AppSettings();
    }

    public AppSettings AppSettings
    {
        get => _appSettings;
        set
        {
            if (SetProperty(ref _appSettings, value))
            {
                // 通知所有相关属性更改
                OnPropertyChanged(nameof(WindowSettings));
                OnPropertyChanged(nameof(KeyboardMonitorSettings));
                
                // 订阅 WindowSettings 的属性更改事件
                if (_appSettings?.Window != null)
                {
                    _appSettings.Window.PropertyChanged += (s, e) => 
                    {
                        OnPropertyChanged(nameof(WindowSettings));
                    };
                }
            }
        }
    }

    /// <summary>
    /// 窗口设置的便捷访问属性
    /// </summary>
    public WindowSettings WindowSettings => AppSettings.Window;
    
    /// <summary>
    /// 键盘监控设置的便捷访问属性
    /// </summary>
    public KeyboardMonitorSettings KeyboardMonitorSettings => AppSettings.KeyboardMonitor;

    /// <summary>
    /// 加载配置
    /// </summary>
    public async Task LoadAsync()
    {
        try
        {
            AppSettings = await _configurationService.LoadAsync();
            Console.WriteLine("Configuration loaded in ViewModel");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading configuration: {ex.Message}");
        }
    }

    /// <summary>
    /// 保存配置
    /// </summary>
    public async Task SaveAsync()
    {
        try
        {
            // 验证配置
            if (!ValidateConfiguration())
            {
                throw new InvalidOperationException("配置验证失败");
            }

            await _configurationService.SaveAsync(AppSettings);
            Console.WriteLine("Configuration saved from ViewModel");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving configuration: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 验证配置
    /// </summary>
    private bool ValidateConfiguration()
    {
        // 验证窗口设置
        if (!ValidateWindowSettings())
            return false;

        // 验证键盘监控设置
        if (!ValidateKeyboardMonitorSettings())
            return false;

        LastValidationError = null;
        return true;
    }

    /// <summary>
    /// 验证窗口设置
    /// </summary>
    private bool ValidateWindowSettings()
    {
        // 验证窗口透明度范围
        if (AppSettings.Window.Opacity < 0.5 || AppSettings.Window.Opacity > 1.0)
        {
            LastValidationError = "窗口透明度必须在 0.5 到 1.0 之间";
            Console.WriteLine($"[验证失败] {LastValidationError}");
            return false;
        }

        // 验证窗口位置
        if (!IsValidPosition(AppSettings.Window.Position))
        {
            LastValidationError = "窗口位置必须是 RightEdge、LeftEdge 或 Custom";
            Console.WriteLine($"[验证失败] {LastValidationError}");
            return false;
        }

        // 验证自定义位置
        if (AppSettings.Window.Position == "Custom")
        {
            if (!AppSettings.Window.CustomX.HasValue || !AppSettings.Window.CustomY.HasValue)
            {
                LastValidationError = "选择自定义位置时必须指定 X 和 Y 坐标";
                Console.WriteLine($"[验证失败] {LastValidationError}");
                return false;
            }

            if (AppSettings.Window.CustomX < 0 || AppSettings.Window.CustomY < 0)
            {
                LastValidationError = "自定义位置坐标必须为正数";
                Console.WriteLine($"[验证失败] {LastValidationError}");
                return false;
            }

            // 验证位置在屏幕范围内
            if (!IsCoordinateInScreenBounds(AppSettings.Window.CustomX.Value, AppSettings.Window.CustomY.Value))
            {
                LastValidationError = "自定义位置坐标超出屏幕范围（最大 10000x10000）";
                Console.WriteLine($"[验证失败] {LastValidationError}");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 验证键盘监控设置
    /// </summary>
    private bool ValidateKeyboardMonitorSettings()
    {
        // 验证背景颜色格式
        if (!IsValidHexColor(AppSettings.KeyboardMonitor.BackgroundColor))
        {
            LastValidationError = $"背景颜色格式无效: {AppSettings.KeyboardMonitor.BackgroundColor}。请使用 Hex 格式（如 #3182CE 或 #FF3182CE）";
            Console.WriteLine($"[验证失败] {LastValidationError}");
            return false;
        }

        // 验证字体颜色格式
        if (!IsValidHexColor(AppSettings.KeyboardMonitor.FontColor))
        {
            LastValidationError = $"字体颜色格式无效: {AppSettings.KeyboardMonitor.FontColor}。请使用 Hex 格式（如 #FFFFFF）";
            Console.WriteLine($"[验证失败] {LastValidationError}");
            return false;
        }

        // 验证字体大小
        if (AppSettings.KeyboardMonitor.FontSize < 12 || AppSettings.KeyboardMonitor.FontSize > 48)
        {
            LastValidationError = $"字体大小必须在 12 到 48 之间，当前值: {AppSettings.KeyboardMonitor.FontSize}";
            Console.WriteLine($"[验证失败] {LastValidationError}");
            return false;
        }

        // 验证透明度
        if (AppSettings.KeyboardMonitor.Opacity < 0.1 || AppSettings.KeyboardMonitor.Opacity > 1.0)
        {
            LastValidationError = $"键盘监控透明度必须在 0.1 到 1.0 之间，当前值: {AppSettings.KeyboardMonitor.Opacity:F2}";
            Console.WriteLine($"[验证失败] {LastValidationError}");
            return false;
        }

        // 验证显示时长
        if (AppSettings.KeyboardMonitor.DisplayDuration < 1 || AppSettings.KeyboardMonitor.DisplayDuration > 10)
        {
            LastValidationError = $"显示时长必须在 1 到 10 秒之间，当前值: {AppSettings.KeyboardMonitor.DisplayDuration}";
            Console.WriteLine($"[验证失败] {LastValidationError}");
            return false;
        }

        // 验证淡入动画时长
        if (AppSettings.KeyboardMonitor.FadeInDuration < 0.1 || AppSettings.KeyboardMonitor.FadeInDuration > 1.0)
        {
            LastValidationError = $"淡入动画时长必须在 0.1 到 1.0 秒之间，当前值: {AppSettings.KeyboardMonitor.FadeInDuration:F2}";
            Console.WriteLine($"[验证失败] {LastValidationError}");
            return false;
        }

        // 验证淡出动画时长
        if (AppSettings.KeyboardMonitor.FadeOutDuration < 0.1 || AppSettings.KeyboardMonitor.FadeOutDuration > 1.0)
        {
            LastValidationError = $"淡出动画时长必须在 0.1 到 1.0 秒之间，当前值: {AppSettings.KeyboardMonitor.FadeOutDuration:F2}";
            Console.WriteLine($"[验证失败] {LastValidationError}");
            return false;
        }

        // 验证显示位置
        if (!IsValidDisplayPosition(AppSettings.KeyboardMonitor.DisplayPosition))
        {
            LastValidationError = "显示位置必须是 TopLeft、TopCenter、TopRight、BottomLeft、BottomCenter、BottomRight 或 Custom";
            Console.WriteLine($"[验证失败] {LastValidationError}");
            return false;
        }

        // 验证自定义显示位置
        if (AppSettings.KeyboardMonitor.DisplayPosition == "Custom")
        {
            if (!AppSettings.KeyboardMonitor.CustomDisplayX.HasValue || !AppSettings.KeyboardMonitor.CustomDisplayY.HasValue)
            {
                LastValidationError = "选择自定义显示位置时必须指定 X 和 Y 坐标";
                Console.WriteLine($"[验证失败] {LastValidationError}");
                return false;
            }

            if (AppSettings.KeyboardMonitor.CustomDisplayX < 0 || AppSettings.KeyboardMonitor.CustomDisplayY < 0)
            {
                LastValidationError = "自定义显示位置坐标必须为正数";
                Console.WriteLine($"[验证失败] {LastValidationError}");
                return false;
            }

            if (!IsCoordinateInScreenBounds(AppSettings.KeyboardMonitor.CustomDisplayX.Value, AppSettings.KeyboardMonitor.CustomDisplayY.Value))
            {
                LastValidationError = "自定义显示位置坐标超出屏幕范围（最大 10000x10000）";
                Console.WriteLine($"[验证失败] {LastValidationError}");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 验证位置字符串是否有效
    /// </summary>
    private static bool IsValidPosition(string position)
    {
        return position == "RightEdge" || position == "LeftEdge" || position == "Custom";
    }

    /// <summary>
    /// 验证显示位置字符串是否有效
    /// </summary>
    private static bool IsValidDisplayPosition(string position)
    {
        return position == "TopLeft" || position == "TopCenter" || position == "TopRight" ||
               position == "BottomLeft" || position == "BottomCenter" || position == "BottomRight" ||
               position == "Custom";
    }

    /// <summary>
    /// 验证坐标是否在屏幕范围内
    /// </summary>
    private static bool IsCoordinateInScreenBounds(int x, int y)
    {
        // 简化验证，假设最大屏幕尺寸为 10000x10000
        return x >= 0 && x <= 10000 && y >= 0 && y <= 10000;
    }

    /// <summary>
    /// 验证 Hex 颜色格式
    /// </summary>
    private static bool IsValidHexColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return false;

        // 支持 #RGB, #RRGGBB, #AARRGGBB 格式
        if (!color.StartsWith('#'))
            return false;

        var hex = color[1..];
        if (hex.Length != 3 && hex.Length != 6 && hex.Length != 8)
            return false;

        // 验证是否为有效的十六进制字符
        foreach (char c in hex)
        {
            if (!IsHexDigit(c))
                return false;
        }

        return true;
    }

    /// <summary>
    /// 检查字符是否为十六进制数字
    /// </summary>
    private static bool IsHexDigit(char c)
    {
        return (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
    }

    /// <summary>
    /// 最后一次验证错误消息
    /// </summary>
    public string? LastValidationError { get; private set; }

    /// <summary>
    /// 重置到默认值
    /// </summary>
    public void Reset()
    {
        AppSettings = new AppSettings();
        Console.WriteLine("Configuration reset to defaults");
    }
}

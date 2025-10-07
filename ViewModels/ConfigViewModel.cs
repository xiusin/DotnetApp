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
        // 验证窗口透明度范围
        if (AppSettings.Window.Opacity < 0.5 || AppSettings.Window.Opacity > 1.0)
        {
            LastValidationError = "窗口透明度必须在 0.5 到 1.0 之间";
            Console.WriteLine(LastValidationError);
            return false;
        }

        // 验证自定义位置
        if (AppSettings.Window.Position == "Custom")
        {
            if (!AppSettings.Window.CustomX.HasValue || !AppSettings.Window.CustomY.HasValue)
            {
                LastValidationError = "选择自定义位置时必须指定 X 和 Y 坐标";
                Console.WriteLine(LastValidationError);
                return false;
            }

            if (AppSettings.Window.CustomX < 0 || AppSettings.Window.CustomY < 0)
            {
                LastValidationError = "自定义位置坐标必须为正数";
                Console.WriteLine(LastValidationError);
                return false;
            }

            // 验证位置在屏幕范围内（简化验证，假设最大屏幕尺寸为 10000x10000）
            if (AppSettings.Window.CustomX > 10000 || AppSettings.Window.CustomY > 10000)
            {
                LastValidationError = "自定义位置坐标超出屏幕范围";
                Console.WriteLine(LastValidationError);
                return false;
            }
        }

        // 验证键盘监控背景颜色格式
        if (!IsValidHexColor(AppSettings.KeyboardMonitor.BackgroundColor))
        {
            LastValidationError = $"背景颜色格式无效: {AppSettings.KeyboardMonitor.BackgroundColor}。请使用 Hex 格式（如 #3182CE）";
            Console.WriteLine(LastValidationError);
            return false;
        }

        // 验证键盘监控字体颜色格式
        if (!IsValidHexColor(AppSettings.KeyboardMonitor.FontColor))
        {
            LastValidationError = $"字体颜色格式无效: {AppSettings.KeyboardMonitor.FontColor}。请使用 Hex 格式（如 #FFFFFF）";
            Console.WriteLine(LastValidationError);
            return false;
        }

        // 验证键盘监控字体大小
        if (AppSettings.KeyboardMonitor.FontSize < 12 || AppSettings.KeyboardMonitor.FontSize > 48)
        {
            LastValidationError = "字体大小必须在 12 到 48 之间";
            Console.WriteLine(LastValidationError);
            return false;
        }

        // 验证键盘监控透明度
        if (AppSettings.KeyboardMonitor.Opacity < 0.1 || AppSettings.KeyboardMonitor.Opacity > 1.0)
        {
            LastValidationError = "键盘监控透明度必须在 0.1 到 1.0 之间";
            Console.WriteLine(LastValidationError);
            return false;
        }

        // 验证显示时长
        if (AppSettings.KeyboardMonitor.DisplayDuration < 1 || AppSettings.KeyboardMonitor.DisplayDuration > 10)
        {
            LastValidationError = "显示时长必须在 1 到 10 秒之间";
            Console.WriteLine(LastValidationError);
            return false;
        }

        // 验证淡入动画时长
        if (AppSettings.KeyboardMonitor.FadeInDuration < 0.1 || AppSettings.KeyboardMonitor.FadeInDuration > 1.0)
        {
            LastValidationError = "淡入动画时长必须在 0.1 到 1.0 秒之间";
            Console.WriteLine(LastValidationError);
            return false;
        }

        // 验证淡出动画时长
        if (AppSettings.KeyboardMonitor.FadeOutDuration < 0.1 || AppSettings.KeyboardMonitor.FadeOutDuration > 1.0)
        {
            LastValidationError = "淡出动画时长必须在 0.1 到 1.0 秒之间";
            Console.WriteLine(LastValidationError);
            return false;
        }

        // 验证键盘监控自定义位置
        if (AppSettings.KeyboardMonitor.DisplayPosition == "Custom")
        {
            if (!AppSettings.KeyboardMonitor.CustomDisplayX.HasValue || !AppSettings.KeyboardMonitor.CustomDisplayY.HasValue)
            {
                LastValidationError = "选择自定义显示位置时必须指定 X 和 Y 坐标";
                Console.WriteLine(LastValidationError);
                return false;
            }

            if (AppSettings.KeyboardMonitor.CustomDisplayX < 0 || AppSettings.KeyboardMonitor.CustomDisplayY < 0)
            {
                LastValidationError = "自定义显示位置坐标必须为正数";
                Console.WriteLine(LastValidationError);
                return false;
            }

            if (AppSettings.KeyboardMonitor.CustomDisplayX > 10000 || AppSettings.KeyboardMonitor.CustomDisplayY > 10000)
            {
                LastValidationError = "自定义显示位置坐标超出屏幕范围";
                Console.WriteLine(LastValidationError);
                return false;
            }
        }

        LastValidationError = null;
        return true;
    }

    /// <summary>
    /// 验证 Hex 颜色格式
    /// </summary>
    private bool IsValidHexColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return false;

        // 支持 #RGB, #RRGGBB, #AARRGGBB 格式
        if (!color.StartsWith("#"))
            return false;

        var hex = color.Substring(1);
        if (hex.Length != 3 && hex.Length != 6 && hex.Length != 8)
            return false;

        // 验证是否为有效的十六进制字符
        foreach (char c in hex)
        {
            if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')))
                return false;
        }

        return true;
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

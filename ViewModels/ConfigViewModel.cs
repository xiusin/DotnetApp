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
            }
        }
    }

    /// <summary>
    /// 窗口设置的便捷访问属性
    /// </summary>
    public WindowSettings WindowSettings => AppSettings.Window;

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
            Console.WriteLine("窗口透明度必须在 0.5 到 1.0 之间");
            return false;
        }

        // 验证自定义位置
        if (AppSettings.Window.Position == "Custom")
        {
            if (!AppSettings.Window.CustomX.HasValue || !AppSettings.Window.CustomY.HasValue)
            {
                Console.WriteLine("选择自定义位置时必须指定 X 和 Y 坐标");
                return false;
            }

            if (AppSettings.Window.CustomX < 0 || AppSettings.Window.CustomY < 0)
            {
                Console.WriteLine("自定义位置坐标必须为正数");
                return false;
            }
        }

        // 验证键盘监控设置
        if (AppSettings.KeyboardMonitor.FontSize < 12 || AppSettings.KeyboardMonitor.FontSize > 48)
        {
            Console.WriteLine("字体大小必须在 12 到 48 之间");
            return false;
        }

        if (AppSettings.KeyboardMonitor.Opacity < 0.1 || AppSettings.KeyboardMonitor.Opacity > 1.0)
        {
            Console.WriteLine("键盘监控透明度必须在 0.1 到 1.0 之间");
            return false;
        }

        if (AppSettings.KeyboardMonitor.DisplayDuration < 1 || AppSettings.KeyboardMonitor.DisplayDuration > 10)
        {
            Console.WriteLine("显示时长必须在 1 到 10 秒之间");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 重置到默认值
    /// </summary>
    public void Reset()
    {
        AppSettings = new AppSettings();
        Console.WriteLine("Configuration reset to defaults");
    }
}

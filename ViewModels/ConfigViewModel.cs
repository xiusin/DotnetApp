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
        set => SetProperty(ref _appSettings, value);
    }

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
    /// 重置到默认值
    /// </summary>
    public void Reset()
    {
        AppSettings = new AppSettings();
        Console.WriteLine("Configuration reset to defaults");
    }
}

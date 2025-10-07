using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using System;
using ConfigButtonDisplay.Core.Interfaces;
using ConfigButtonDisplay.Core.Services;
using ConfigButtonDisplay.ViewModels;

namespace ConfigButtonDisplay;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        ConfigureServices();
    }

    /// <summary>
    /// 配置依赖注入容器
    /// </summary>
    private void ConfigureServices()
    {
        var services = new ServiceCollection();

        // 注册核心服务为单例
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IWindowPositionService, WindowPositionService>();

        // 注册 ViewModels 为瞬态
        services.AddTransient<ConfigViewModel>();

        // 注册窗口为瞬态（每次请求创建新实例）
        services.AddTransient<MainWindow>();
        services.AddTransient<Views.ConfigWindow>();

        // 构建 ServiceProvider
        _serviceProvider = services.BuildServiceProvider();

        Console.WriteLine("依赖注入容器已配置");
    }

    /// <summary>
    /// 获取服务实例
    /// </summary>
    public T? GetService<T>() where T : class
    {
        return _serviceProvider?.GetService<T>();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            
            // 从依赖注入容器获取 MainWindow
            desktop.MainWindow = _serviceProvider?.GetRequiredService<MainWindow>() 
                ?? throw new InvalidOperationException("无法创建 MainWindow");
            
            Console.WriteLine("MainWindow 已从依赖注入容器创建");
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove = BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
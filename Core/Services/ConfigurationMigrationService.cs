using System;
using System.Collections.Generic;
using ConfigButtonDisplay.Core.Configuration;

namespace ConfigButtonDisplay.Core.Services;

/// <summary>
/// 配置迁移服务 - 处理不同版本配置的升级
/// </summary>
public class ConfigurationMigrationService
{
    private const int CurrentVersion = 1;
    private readonly Dictionary<int, Func<AppSettings, AppSettings>> _migrations;

    public ConfigurationMigrationService()
    {
        _migrations = new Dictionary<int, Func<AppSettings, AppSettings>>
        {
            // 未来版本的迁移函数可以在这里添加
            // { 2, MigrateFromV1ToV2 },
            // { 3, MigrateFromV2ToV3 },
        };
    }

    /// <summary>
    /// 检查配置是否需要迁移
    /// </summary>
    public bool NeedsMigration(AppSettings settings)
    {
        return settings.Version < CurrentVersion;
    }

    /// <summary>
    /// 迁移配置到最新版本
    /// </summary>
    public AppSettings MigrateToLatest(AppSettings settings)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        var currentSettings = settings;
        var startVersion = settings.Version;

        Console.WriteLine($"开始配置迁移: 版本 {startVersion} -> {CurrentVersion}");

        // 逐步迁移到最新版本
        for (int version = startVersion + 1; version <= CurrentVersion; version++)
        {
            if (_migrations.TryGetValue(version, out var migrationFunc))
            {
                Console.WriteLine($"执行迁移: 版本 {version - 1} -> {version}");
                currentSettings = migrationFunc(currentSettings);
                currentSettings.Version = version;
            }
        }

        // 确保版本号正确
        currentSettings.Version = CurrentVersion;

        if (startVersion != CurrentVersion)
        {
            Console.WriteLine($"配置迁移完成: 版本 {startVersion} -> {CurrentVersion}");
        }

        return currentSettings;
    }

    /// <summary>
    /// 验证配置完整性
    /// </summary>
    public (bool IsValid, List<string> Issues) ValidateConfiguration(AppSettings settings)
    {
        var issues = new List<string>();

        if (settings == null)
        {
            issues.Add("配置对象为空");
            return (false, issues);
        }

        // 验证版本
        if (settings.Version < 1 || settings.Version > CurrentVersion)
        {
            issues.Add($"配置版本无效: {settings.Version}");
        }

        // 验证窗口配置
        if (settings.Window == null)
        {
            issues.Add("窗口配置缺失");
        }
        else
        {
            if (settings.Window.Opacity < 0.5 || settings.Window.Opacity > 1.0)
            {
                issues.Add($"窗口透明度超出范围: {settings.Window.Opacity}");
            }

            if (settings.Window.Position == "Custom")
            {
                if (!settings.Window.CustomX.HasValue || !settings.Window.CustomY.HasValue)
                {
                    issues.Add("自定义位置缺少坐标");
                }
            }
        }

        // 验证键盘监控配置
        if (settings.KeyboardMonitor == null)
        {
            issues.Add("键盘监控配置缺失");
        }
        else
        {
            if (settings.KeyboardMonitor.FontSize < 12 || settings.KeyboardMonitor.FontSize > 48)
            {
                issues.Add($"字体大小超出范围: {settings.KeyboardMonitor.FontSize}");
            }

            if (settings.KeyboardMonitor.Opacity < 0.1 || settings.KeyboardMonitor.Opacity > 1.0)
            {
                issues.Add($"键盘监控透明度超出范围: {settings.KeyboardMonitor.Opacity}");
            }

            if (settings.KeyboardMonitor.DisplayDuration < 1 || settings.KeyboardMonitor.DisplayDuration > 10)
            {
                issues.Add($"显示时长超出范围: {settings.KeyboardMonitor.DisplayDuration}");
            }
        }

        return (issues.Count == 0, issues);
    }

    /// <summary>
    /// 修复配置问题
    /// </summary>
    public AppSettings RepairConfiguration(AppSettings settings)
    {
        if (settings == null)
            return new AppSettings();

        // 修复版本
        if (settings.Version < 1 || settings.Version > CurrentVersion)
        {
            settings.Version = CurrentVersion;
        }

        // 修复窗口配置
        if (settings.Window == null)
        {
            settings.Window = new WindowSettings();
        }
        else
        {
            if (settings.Window.Opacity < 0.5 || settings.Window.Opacity > 1.0)
            {
                settings.Window.Opacity = 0.95;
            }
        }

        // 修复键盘监控配置
        if (settings.KeyboardMonitor == null)
        {
            settings.KeyboardMonitor = new KeyboardMonitorSettings();
        }
        else
        {
            if (settings.KeyboardMonitor.FontSize < 12 || settings.KeyboardMonitor.FontSize > 48)
            {
                settings.KeyboardMonitor.FontSize = 28;
            }

            if (settings.KeyboardMonitor.Opacity < 0.1 || settings.KeyboardMonitor.Opacity > 1.0)
            {
                settings.KeyboardMonitor.Opacity = 0.9;
            }

            if (settings.KeyboardMonitor.DisplayDuration < 1 || settings.KeyboardMonitor.DisplayDuration > 10)
            {
                settings.KeyboardMonitor.DisplayDuration = 2.0;
            }
        }

        Console.WriteLine("配置已修复");
        return settings;
    }

    /// <summary>
    /// 获取当前配置版本
    /// </summary>
    public int GetCurrentVersion() => CurrentVersion;

    // 未来版本的迁移函数示例
    // private AppSettings MigrateFromV1ToV2(AppSettings settings)
    // {
    //     // 添加新字段的默认值
    //     // 转换旧字段格式
    //     // 等等
    //     return settings;
    // }
}

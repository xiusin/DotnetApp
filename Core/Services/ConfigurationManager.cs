using System;
using System.Threading.Tasks;
using ConfigButtonDisplay.Core.Configuration;
using ConfigButtonDisplay.Core.Interfaces;

namespace ConfigButtonDisplay.Core.Services;

/// <summary>
/// 配置管理器 - 统一的配置管理接口
/// </summary>
public class ConfigurationManager
{
    private readonly IConfigurationService _configService;
    private readonly ConfigurationImportExportService _importExportService;
    private readonly ConfigurationBackupService _backupService;
    private readonly ConfigurationMigrationService _migrationService;
    private readonly ConfigurationValidator _validator;

    public ConfigurationManager(IConfigurationService configService)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _importExportService = new ConfigurationImportExportService();
        _backupService = new ConfigurationBackupService();
        _migrationService = new ConfigurationMigrationService();
        _validator = new ConfigurationValidator();
    }

    /// <summary>
    /// 加载配置（带验证和迁移）
    /// </summary>
    public async Task<AppSettings> LoadConfigurationAsync()
    {
        var settings = await _configService.LoadAsync();

        // 检查是否需要迁移
        if (_migrationService.NeedsMigration(settings))
        {
            Console.WriteLine("检测到旧版本配置，开始迁移...");
            
            // 创建备份
            await _backupService.CreateBackupAsync(settings);
            
            // 执行迁移
            settings = _migrationService.MigrateToLatest(settings);
            
            // 保存迁移后的配置
            await _configService.SaveAsync(settings);
        }

        // 验证配置
        var validationResult = _validator.Validate(settings);
        if (!validationResult.IsValid)
        {
            Console.WriteLine("配置验证失败:");
            Console.WriteLine(validationResult.GetSummary());
            
            // 尝试修复
            settings = _migrationService.RepairConfiguration(settings);
            await _configService.SaveAsync(settings);
        }
        else if (validationResult.HasWarnings)
        {
            Console.WriteLine("配置验证警告:");
            Console.WriteLine(validationResult.GetSummary());
        }

        return settings;
    }

    /// <summary>
    /// 保存配置（带验证和备份）
    /// </summary>
    public async Task<bool> SaveConfigurationAsync(AppSettings settings, bool createBackup = true)
    {
        try
        {
            // 验证配置
            var validationResult = _validator.Validate(settings);
            if (!validationResult.IsValid)
            {
                Console.WriteLine("配置验证失败，无法保存:");
                Console.WriteLine(validationResult.GetSummary());
                return false;
            }

            // 创建备份
            if (createBackup)
            {
                var currentSettings = await _configService.LoadAsync();
                await _backupService.CreateBackupAsync(currentSettings);
            }

            // 保存配置
            await _configService.SaveAsync(settings);
            
            Console.WriteLine("配置已成功保存");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"保存配置失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 导出配置
    /// </summary>
    public async Task<bool> ExportConfigurationAsync(string filePath)
    {
        var settings = await _configService.LoadAsync();
        return await _importExportService.ExportConfigurationAsync(settings, filePath);
    }

    /// <summary>
    /// 导入配置
    /// </summary>
    public async Task<bool> ImportConfigurationAsync(string filePath)
    {
        var (success, settings, error) = await _importExportService.ImportConfigurationAsync(filePath);
        
        if (!success || settings == null)
        {
            Console.WriteLine($"导入失败: {error}");
            return false;
        }

        // 创建当前配置的备份
        var currentSettings = await _configService.LoadAsync();
        await _backupService.CreateBackupAsync(currentSettings);

        // 保存导入的配置
        return await SaveConfigurationAsync(settings, createBackup: false);
    }

    /// <summary>
    /// 恢复备份
    /// </summary>
    public async Task<bool> RestoreBackupAsync(string backupFilePath)
    {
        var settings = await _backupService.RestoreFromBackupAsync(backupFilePath);
        if (settings == null)
            return false;

        return await SaveConfigurationAsync(settings, createBackup: false);
    }

    /// <summary>
    /// 获取所有备份
    /// </summary>
    public System.Collections.Generic.List<BackupInfo> GetAllBackups()
    {
        return _backupService.GetAllBackups();
    }

    /// <summary>
    /// 删除备份
    /// </summary>
    public bool DeleteBackup(string backupFilePath)
    {
        return _backupService.DeleteBackup(backupFilePath);
    }

    /// <summary>
    /// 验证配置
    /// </summary>
    public ValidationResult ValidateConfiguration(AppSettings settings)
    {
        return _validator.Validate(settings);
    }

    /// <summary>
    /// 重置为默认配置
    /// </summary>
    public async Task<bool> ResetToDefaultAsync()
    {
        try
        {
            // 创建当前配置的备份
            var currentSettings = await _configService.LoadAsync();
            await _backupService.CreateBackupAsync(currentSettings);

            // 创建默认配置
            var defaultSettings = new AppSettings();
            
            // 保存默认配置
            await _configService.SaveAsync(defaultSettings);
            
            Console.WriteLine("配置已重置为默认值");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"重置配置失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 获取配置信息
    /// </summary>
    public async Task<ConfigurationInfo> GetConfigurationInfoAsync()
    {
        var settings = await _configService.LoadAsync();
        var validationResult = _validator.Validate(settings);
        var backups = _backupService.GetAllBackups();

        return new ConfigurationInfo
        {
            Version = settings.Version,
            IsValid = validationResult.IsValid,
            ErrorCount = validationResult.Errors.Count,
            WarningCount = validationResult.Warnings.Count,
            BackupCount = backups.Count,
            NeedsMigration = _migrationService.NeedsMigration(settings)
        };
    }
}

/// <summary>
/// 配置信息
/// </summary>
public class ConfigurationInfo
{
    public int Version { get; set; }
    public bool IsValid { get; set; }
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int BackupCount { get; set; }
    public bool NeedsMigration { get; set; }

    public string GetStatusText()
    {
        if (!IsValid)
            return $"❌ 配置无效 ({ErrorCount} 个错误)";
        
        if (WarningCount > 0)
            return $"⚠️ 配置有效 ({WarningCount} 个警告)";
        
        return "✓ 配置正常";
    }
}

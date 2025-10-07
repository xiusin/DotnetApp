using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ConfigButtonDisplay.Core.Configuration;

namespace ConfigButtonDisplay.Core.Services;

/// <summary>
/// 配置导入导出服务
/// </summary>
public class ConfigurationImportExportService
{
    private readonly JsonSerializerOptions _jsonOptions;

    public ConfigurationImportExportService()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// 导出配置到文件
    /// </summary>
    public async Task<bool> ExportConfigurationAsync(AppSettings settings, string filePath)
    {
        try
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("文件路径不能为空", nameof(filePath));

            // 确保目录存在
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 创建导出包装对象，包含元数据
            var exportData = new ConfigurationExportData
            {
                Version = settings.Version,
                ExportDate = DateTime.UtcNow,
                ExportedBy = Environment.UserName,
                MachineName = Environment.MachineName,
                Settings = settings
            };

            var json = JsonSerializer.Serialize(exportData, _jsonOptions);
            await File.WriteAllTextAsync(filePath, json);

            Console.WriteLine($"配置已成功导出到: {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"导出配置失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 从文件导入配置
    /// </summary>
    public async Task<(bool Success, AppSettings? Settings, string? Error)> ImportConfigurationAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return (false, null, "文件路径不能为空");

            if (!File.Exists(filePath))
                return (false, null, "文件不存在");

            var json = await File.ReadAllTextAsync(filePath);
            
            // 尝试解析为导出数据格式
            ConfigurationExportData? exportData = null;
            try
            {
                exportData = JsonSerializer.Deserialize<ConfigurationExportData>(json, _jsonOptions);
            }
            catch
            {
                // 如果失败，尝试直接解析为 AppSettings
            }

            AppSettings? settings = null;
            if (exportData?.Settings != null)
            {
                settings = exportData.Settings;
                Console.WriteLine($"导入配置: 版本 {exportData.Version}, 导出时间 {exportData.ExportDate}");
            }
            else
            {
                // 直接解析为 AppSettings
                settings = JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);
            }

            if (settings == null)
                return (false, null, "配置文件格式无效");

            // 验证配置
            var validationResult = ValidateConfiguration(settings);
            if (!validationResult.IsValid)
                return (false, null, $"配置验证失败: {validationResult.Error}");

            Console.WriteLine($"配置已成功从 {filePath} 导入");
            return (true, settings, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"导入配置失败: {ex.Message}");
            return (false, null, $"导入失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 验证配置
    /// </summary>
    private (bool IsValid, string? Error) ValidateConfiguration(AppSettings settings)
    {
        if (settings == null)
            return (false, "配置对象为空");

        if (settings.Version < 1)
            return (false, "配置版本无效");

        if (settings.Window == null)
            return (false, "窗口配置缺失");

        if (settings.KeyboardMonitor == null)
            return (false, "键盘监控配置缺失");

        return (true, null);
    }

    /// <summary>
    /// 导出配置到 JSON 字符串
    /// </summary>
    public string ExportToJson(AppSettings settings)
    {
        return JsonSerializer.Serialize(settings, _jsonOptions);
    }

    /// <summary>
    /// 从 JSON 字符串导入配置
    /// </summary>
    public AppSettings? ImportFromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions);
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// 配置导出数据包装类
/// </summary>
public class ConfigurationExportData
{
    public int Version { get; set; }
    public DateTime ExportDate { get; set; }
    public string ExportedBy { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public AppSettings Settings { get; set; } = new();
}

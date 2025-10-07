using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ConfigButtonDisplay.Core.Configuration;

namespace ConfigButtonDisplay.Core.Services;

/// <summary>
/// 配置备份服务
/// </summary>
public class ConfigurationBackupService
{
    private readonly string _backupDirectory;
    private const int MaxBackupCount = 10;
    private const string BackupFilePattern = "backup_*.json";

    public ConfigurationBackupService(string? backupDirectory = null)
    {
        _backupDirectory = backupDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ConfigButtonDisplay",
            "Backups"
        );

        EnsureBackupDirectoryExists();
    }

    /// <summary>
    /// 创建配置备份
    /// </summary>
    public async Task<string?> CreateBackupAsync(AppSettings settings)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"backup_{timestamp}.json";
            var backupFilePath = Path.Combine(_backupDirectory, backupFileName);

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(backupFilePath, json);
            Console.WriteLine($"配置备份已创建: {backupFilePath}");

            // 清理旧备份
            await CleanupOldBackupsAsync();

            return backupFilePath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"创建备份失败: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 获取所有备份
    /// </summary>
    public List<BackupInfo> GetAllBackups()
    {
        try
        {
            var backupFiles = Directory.GetFiles(_backupDirectory, BackupFilePattern)
                .OrderByDescending(f => File.GetCreationTime(f))
                .Select(f => new BackupInfo
                {
                    FilePath = f,
                    FileName = Path.GetFileName(f),
                    CreatedDate = File.GetCreationTime(f),
                    Size = new FileInfo(f).Length
                })
                .ToList();

            return backupFiles;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取备份列表失败: {ex.Message}");
            return new List<BackupInfo>();
        }
    }

    /// <summary>
    /// 从备份恢复配置
    /// </summary>
    public async Task<AppSettings?> RestoreFromBackupAsync(string backupFilePath)
    {
        try
        {
            if (!File.Exists(backupFilePath))
            {
                Console.WriteLine($"备份文件不存在: {backupFilePath}");
                return null;
            }

            var json = await File.ReadAllTextAsync(backupFilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);

            if (settings != null)
            {
                Console.WriteLine($"配置已从备份恢复: {backupFilePath}");
            }

            return settings;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"从备份恢复失败: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 删除备份
    /// </summary>
    public bool DeleteBackup(string backupFilePath)
    {
        try
        {
            if (File.Exists(backupFilePath))
            {
                File.Delete(backupFilePath);
                Console.WriteLine($"备份已删除: {backupFilePath}");
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"删除备份失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 清理旧备份，保留最新的 N 个
    /// </summary>
    private async Task CleanupOldBackupsAsync()
    {
        try
        {
            var backups = GetAllBackups();
            if (backups.Count <= MaxBackupCount)
                return;

            var backupsToDelete = backups.Skip(MaxBackupCount).ToList();
            foreach (var backup in backupsToDelete)
            {
                DeleteBackup(backup.FilePath);
            }

            Console.WriteLine($"已清理 {backupsToDelete.Count} 个旧备份");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"清理旧备份失败: {ex.Message}");
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// 确保备份目录存在
    /// </summary>
    private void EnsureBackupDirectoryExists()
    {
        if (!Directory.Exists(_backupDirectory))
        {
            Directory.CreateDirectory(_backupDirectory);
            Console.WriteLine($"备份目录已创建: {_backupDirectory}");
        }
    }

    /// <summary>
    /// 获取备份目录路径
    /// </summary>
    public string GetBackupDirectory() => _backupDirectory;
}

/// <summary>
/// 备份信息
/// </summary>
public class BackupInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public long Size { get; set; }

    public string FormattedSize => FormatBytes(Size);
    public string FormattedDate => CreatedDate.ToString("yyyy-MM-dd HH:mm:ss");

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

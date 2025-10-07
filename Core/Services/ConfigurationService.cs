using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ConfigButtonDisplay.Core.Configuration;
using ConfigButtonDisplay.Core.Interfaces;

namespace ConfigButtonDisplay.Core.Services;

/// <summary>
/// 配置服务实现，负责配置的加载、保存和管理
/// </summary>
public class ConfigurationService : IConfigurationService, IDisposable
{
    private readonly string _configDirectory;
    private readonly string _configFilePath;
    private AppSettings? _cachedSettings;
    private FileSystemWatcher? _fileWatcher;
    private Timer? _debounceTimer;
    private bool _isInternalChange = false;
    private const int DebounceDelayMs = 500;

    /// <summary>
    /// 配置文件更改事件
    /// </summary>
    public event EventHandler<AppSettings>? ConfigurationChanged;

    public ConfigurationService()
    {
        // 配置文件存储在用户的 AppData 目录
        _configDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ConfigButtonDisplay"
        );
        _configFilePath = Path.Combine(_configDirectory, "appsettings.json");
    }

    /// <summary>
    /// 异步加载应用配置
    /// </summary>
    public async Task<AppSettings> LoadAsync()
    {
        try
        {
            // 如果已有缓存，直接返回
            if (_cachedSettings != null)
            {
                return _cachedSettings;
            }

            // 确保配置目录存在
            if (!Directory.Exists(_configDirectory))
            {
                Directory.CreateDirectory(_configDirectory);
            }

            // 如果配置文件不存在，创建默认配置
            if (!File.Exists(_configFilePath))
            {
                var defaultSettings = new AppSettings();
                await SaveAsync(defaultSettings);
                _cachedSettings = defaultSettings;
                return defaultSettings;
            }

            // 读取配置文件
            var json = await File.ReadAllTextAsync(_configFilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);

            if (settings == null)
            {
                // 反序列化失败，使用默认配置
                var defaultSettings = new AppSettings();
                _cachedSettings = defaultSettings;
                return defaultSettings;
            }

            _cachedSettings = settings;
            return settings;
        }
        catch (JsonException ex)
        {
            // JSON 格式错误，备份损坏的文件并使用默认配置
            Console.WriteLine($"配置文件格式错误: {ex.Message}");
            
            if (File.Exists(_configFilePath))
            {
                var backupPath = $"{_configFilePath}.bak";
                File.Copy(_configFilePath, backupPath, true);
                Console.WriteLine($"已备份损坏的配置文件到: {backupPath}");
            }

            var defaultSettings = new AppSettings();
            _cachedSettings = defaultSettings;
            return defaultSettings;
        }
        catch (Exception ex)
        {
            // 其他错误，使用默认配置
            Console.WriteLine($"加载配置文件时出错: {ex.Message}");
            var defaultSettings = new AppSettings();
            _cachedSettings = defaultSettings;
            return defaultSettings;
        }
    }

    /// <summary>
    /// 异步保存应用配置
    /// </summary>
    public async Task SaveAsync(AppSettings settings)
    {
        try
        {
            // 标记为内部更改，避免触发文件监听事件
            _isInternalChange = true;

            // 确保配置目录存在
            if (!Directory.Exists(_configDirectory))
            {
                Directory.CreateDirectory(_configDirectory);
            }

            // 序列化配置对象
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,  // 格式化输出，便于阅读
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(settings, options);

            // 写入文件
            await File.WriteAllTextAsync(_configFilePath, json);

            // 更新缓存
            _cachedSettings = settings;

            Console.WriteLine($"配置已保存到: {_configFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"保存配置文件时出错: {ex.Message}");
            _isInternalChange = false;
            throw;
        }
    }

    /// <summary>
    /// 异步获取指定模块的配置
    /// </summary>
    public async Task<T?> GetModuleAsync<T>(string moduleName) where T : class
    {
        try
        {
            var settings = await LoadAsync();

            if (settings.Modules.TryGetValue(moduleName, out var moduleConfig))
            {
                // 将 object 转换为 JSON 字符串，再反序列化为目标类型
                var json = JsonSerializer.Serialize(moduleConfig);
                return JsonSerializer.Deserialize<T>(json);
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"获取模块配置时出错 ({moduleName}): {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 异步保存指定模块的配置
    /// </summary>
    public async Task SaveModuleAsync<T>(string moduleName, T settings) where T : class
    {
        try
        {
            var appSettings = await LoadAsync();

            // 更新或添加模块配置
            appSettings.Modules[moduleName] = settings;

            // 保存整个配置
            await SaveAsync(appSettings);

            Console.WriteLine($"模块配置已保存: {moduleName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"保存模块配置时出错 ({moduleName}): {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 启动配置文件监听
    /// </summary>
    public void StartWatching()
    {
        if (_fileWatcher != null)
        {
            Console.WriteLine("配置文件监听已经启动");
            return;
        }

        try
        {
            // 确保配置目录存在
            if (!Directory.Exists(_configDirectory))
            {
                Directory.CreateDirectory(_configDirectory);
            }

            // 创建文件监听器
            _fileWatcher = new FileSystemWatcher(_configDirectory)
            {
                Filter = "appsettings.json",
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            // 订阅文件更改事件
            _fileWatcher.Changed += OnConfigFileChanged;

            Console.WriteLine($"开始监听配置文件: {_configFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"启动配置文件监听时出错: {ex.Message}");
        }
    }

    /// <summary>
    /// 停止配置文件监听
    /// </summary>
    public void StopWatching()
    {
        if (_fileWatcher != null)
        {
            _fileWatcher.Changed -= OnConfigFileChanged;
            _fileWatcher.Dispose();
            _fileWatcher = null;
            Console.WriteLine("已停止监听配置文件");
        }

        if (_debounceTimer != null)
        {
            _debounceTimer.Dispose();
            _debounceTimer = null;
        }
    }

    /// <summary>
    /// 配置文件更改事件处理
    /// </summary>
    private void OnConfigFileChanged(object sender, FileSystemEventArgs e)
    {
        // 如果是内部保存触发的更改，忽略
        if (_isInternalChange)
        {
            _isInternalChange = false;
            return;
        }

        Console.WriteLine($"检测到配置文件更改: {e.FullPath}");

        // 使用防抖动机制，避免频繁重新加载
        _debounceTimer?.Dispose();
        _debounceTimer = new Timer(async _ =>
        {
            try
            {
                // 等待文件写入完成
                await Task.Delay(100);

                // 重新加载配置
                _cachedSettings = null;
                var newSettings = await LoadAsync();

                // 触发配置更改事件
                ConfigurationChanged?.Invoke(this, newSettings);

                Console.WriteLine("配置已重新加载");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"重新加载配置时出错: {ex.Message}");
            }
        }, null, DebounceDelayMs, Timeout.Infinite);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        StopWatching();
    }
}

using System;
using System.Collections.Concurrent;
using ConfigButtonDisplay.Core.Configuration;

namespace ConfigButtonDisplay.Core.Services;

/// <summary>
/// 配置缓存优化器 - 减少重复的配置对象创建
/// </summary>
public class ConfigurationCacheOptimizer
{
    private readonly ConcurrentDictionary<string, object> _moduleCache = new();
    private AppSettings? _cachedSettings;
    private DateTime _lastCacheTime = DateTime.MinValue;
    private const int CacheExpirationSeconds = 60;

    /// <summary>
    /// 获取或创建缓存的配置
    /// </summary>
    public AppSettings GetOrCreateSettings(Func<AppSettings> factory)
    {
        var now = DateTime.UtcNow;
        
        if (_cachedSettings != null && 
            (now - _lastCacheTime).TotalSeconds < CacheExpirationSeconds)
        {
            return _cachedSettings;
        }

        _cachedSettings = factory();
        _lastCacheTime = now;
        return _cachedSettings;
    }

    /// <summary>
    /// 获取或创建模块配置缓存
    /// </summary>
    public T GetOrCreateModule<T>(string moduleName, Func<T> factory) where T : class
    {
        if (_moduleCache.TryGetValue(moduleName, out var cached))
        {
            return (T)cached;
        }

        var module = factory();
        _moduleCache[moduleName] = module;
        return module;
    }

    /// <summary>
    /// 清除缓存
    /// </summary>
    public void ClearCache()
    {
        _cachedSettings = null;
        _moduleCache.Clear();
        _lastCacheTime = DateTime.MinValue;
    }

    /// <summary>
    /// 更新缓存
    /// </summary>
    public void UpdateCache(AppSettings settings)
    {
        _cachedSettings = settings;
        _lastCacheTime = DateTime.UtcNow;
        _moduleCache.Clear();
    }
}

using System;
using System.Threading.Tasks;
using ConfigButtonDisplay.Core.Configuration;

namespace ConfigButtonDisplay.Core.Interfaces;

/// <summary>
/// 配置服务接口，负责应用配置的加载、保存和管理
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// 配置文件更改事件
    /// </summary>
    event EventHandler<AppSettings>? ConfigurationChanged;
    
    /// <summary>
    /// 异步加载应用配置
    /// </summary>
    /// <returns>应用配置对象</returns>
    Task<AppSettings> LoadAsync();
    
    /// <summary>
    /// 异步保存应用配置
    /// </summary>
    /// <param name="settings">要保存的配置对象</param>
    Task SaveAsync(AppSettings settings);
    
    /// <summary>
    /// 异步获取指定模块的配置
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="moduleName">模块名称</param>
    /// <returns>模块配置对象</returns>
    Task<T?> GetModuleAsync<T>(string moduleName) where T : class;
    
    /// <summary>
    /// 异步保存指定模块的配置
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="moduleName">模块名称</param>
    /// <param name="settings">模块配置对象</param>
    Task SaveModuleAsync<T>(string moduleName, T settings) where T : class;
    
    /// <summary>
    /// 启动配置文件监听
    /// </summary>
    void StartWatching();
    
    /// <summary>
    /// 停止配置文件监听
    /// </summary>
    void StopWatching();
}

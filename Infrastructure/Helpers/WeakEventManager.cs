using System;
using System.Collections.Generic;
using System.Linq;

namespace ConfigButtonDisplay.Infrastructure.Helpers;

/// <summary>
/// 弱引用事件管理器 - 防止事件订阅导致的内存泄漏
/// </summary>
public class WeakEventManager<TEventArgs> where TEventArgs : EventArgs
{
    private readonly List<WeakReference<EventHandler<TEventArgs>>> _handlers = new();
    private readonly object _lock = new();

    /// <summary>
    /// 添加事件处理器
    /// </summary>
    public void AddHandler(EventHandler<TEventArgs> handler)
    {
        if (handler == null) return;

        lock (_lock)
        {
            _handlers.Add(new WeakReference<EventHandler<TEventArgs>>(handler));
        }
    }

    /// <summary>
    /// 移除事件处理器
    /// </summary>
    public void RemoveHandler(EventHandler<TEventArgs> handler)
    {
        if (handler == null) return;

        lock (_lock)
        {
            _handlers.RemoveAll(wr =>
            {
                if (!wr.TryGetTarget(out var target))
                    return true;
                return target == handler;
            });
        }
    }

    /// <summary>
    /// 触发事件
    /// </summary>
    public void RaiseEvent(object sender, TEventArgs args)
    {
        List<EventHandler<TEventArgs>> handlersToInvoke;

        lock (_lock)
        {
            // 清理已失效的弱引用
            _handlers.RemoveAll(wr => !wr.TryGetTarget(out _));

            // 获取所有有效的处理器
            handlersToInvoke = _handlers
                .Select(wr =>
                {
                    wr.TryGetTarget(out var handler);
                    return handler;
                })
                .Where(h => h != null)
                .ToList()!;
        }

        // 在锁外调用处理器
        foreach (var handler in handlersToInvoke)
        {
            try
            {
                handler?.Invoke(sender, args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in event handler: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 清除所有处理器
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _handlers.Clear();
        }
    }
}

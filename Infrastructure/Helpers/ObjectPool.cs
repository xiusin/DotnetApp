using System;
using System.Collections.Concurrent;

namespace ConfigButtonDisplay.Infrastructure.Helpers;

/// <summary>
/// 简单的对象池实现 - 减少频繁的对象分配和 GC 压力
/// </summary>
public class ObjectPool<T> where T : class, new()
{
    private readonly ConcurrentBag<T> _objects = new();
    private readonly Func<T> _objectGenerator;
    private readonly Action<T>? _resetAction;
    private readonly int _maxSize;

    public ObjectPool(int maxSize = 100, Func<T>? objectGenerator = null, Action<T>? resetAction = null)
    {
        _maxSize = maxSize;
        _objectGenerator = objectGenerator ?? (() => new T());
        _resetAction = resetAction;
    }

    /// <summary>
    /// 从池中获取对象
    /// </summary>
    public T Get()
    {
        if (_objects.TryTake(out var item))
        {
            return item;
        }

        return _objectGenerator();
    }

    /// <summary>
    /// 将对象返回到池中
    /// </summary>
    public void Return(T item)
    {
        if (item == null) return;

        // 重置对象状态
        _resetAction?.Invoke(item);

        // 如果池未满，则返回对象
        if (_objects.Count < _maxSize)
        {
            _objects.Add(item);
        }
    }

    /// <summary>
    /// 清空对象池
    /// </summary>
    public void Clear()
    {
        _objects.Clear();
    }

    /// <summary>
    /// 获取当前池中的对象数量
    /// </summary>
    public int Count => _objects.Count;
}

/// <summary>
/// 可释放的池化对象包装器
/// </summary>
public struct PooledObject<T> : IDisposable where T : class, new()
{
    private readonly ObjectPool<T> _pool;
    private readonly T _object;

    public PooledObject(ObjectPool<T> pool, T obj)
    {
        _pool = pool;
        _object = obj;
    }

    public T Object => _object;

    public void Dispose()
    {
        _pool.Return(_object);
    }
}

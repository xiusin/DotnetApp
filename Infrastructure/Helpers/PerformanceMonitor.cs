using System;
using System.Diagnostics;

namespace ConfigButtonDisplay.Infrastructure.Helpers;

/// <summary>
/// 性能监控工具 - 用于测量和优化代码性能
/// </summary>
public class PerformanceMonitor : IDisposable
{
    private readonly Stopwatch _stopwatch;
    private readonly string _operationName;
    private readonly bool _logToConsole;

    public PerformanceMonitor(string operationName, bool logToConsole = true)
    {
        _operationName = operationName;
        _logToConsole = logToConsole;
        _stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        _stopwatch.Stop();
        
        if (_logToConsole)
        {
            var elapsed = _stopwatch.ElapsedMilliseconds;
            if (elapsed > 100)
            {
                Console.WriteLine($"⚠️ Performance: {_operationName} took {elapsed}ms");
            }
            else if (elapsed > 16)
            {
                Console.WriteLine($"ℹ️ Performance: {_operationName} took {elapsed}ms");
            }
        }
    }

    public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;

    /// <summary>
    /// 测量操作性能的便捷方法
    /// </summary>
    public static void Measure(string operationName, Action action)
    {
        using var monitor = new PerformanceMonitor(operationName);
        action();
    }

    /// <summary>
    /// 测量异步操作性能的便捷方法
    /// </summary>
    public static async System.Threading.Tasks.Task MeasureAsync(string operationName, Func<System.Threading.Tasks.Task> action)
    {
        using var monitor = new PerformanceMonitor(operationName);
        await action();
    }
}

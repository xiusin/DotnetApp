using System;
using Avalonia.Input;

namespace ConfigButtonDisplay.Infrastructure.Helpers;

/// <summary>
/// 优化的双击 Shift 检测器 - 使用 UtcNow 减少性能开销
/// </summary>
public class DoubleShiftDetector
{
    private long _lastShiftPressTicks = 0;
    private int _intervalMs = 500;

    public int Interval
    {
        get => _intervalMs;
        set => _intervalMs = Math.Max(100, Math.Min(2000, value));
    }

    public bool OnKeyDown(Key key)
    {
        if (key != Key.LeftShift && key != Key.RightShift)
            return false;

        var nowTicks = DateTime.UtcNow.Ticks;
        var elapsedMs = (nowTicks - _lastShiftPressTicks) / TimeSpan.TicksPerMillisecond;

        if (elapsedMs < _intervalMs && elapsedMs > 0)
        {
            _lastShiftPressTicks = 0;
            return true;
        }

        _lastShiftPressTicks = nowTicks;
        return false;
    }

    public void Reset()
    {
        _lastShiftPressTicks = 0;
    }
}

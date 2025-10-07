using System;
using Avalonia.Input;

namespace ConfigButtonDisplay.Infrastructure.Helpers;

public class DoubleShiftDetector
{
    private DateTime _lastShiftPress = DateTime.MinValue;
    private int _interval = 500;  // ms

    public int Interval
    {
        get => _interval;
        set => _interval = Math.Max(100, Math.Min(2000, value));  // Clamp between 100-2000ms
    }

    public bool OnKeyDown(Key key)
    {
        if (key != Key.LeftShift && key != Key.RightShift)
            return false;

        var now = DateTime.Now;
        var elapsed = (now - _lastShiftPress).TotalMilliseconds;

        if (elapsed < _interval && elapsed > 0)
        {
            _lastShiftPress = DateTime.MinValue;  // Reset
            return true;  // Double-shift detected
        }

        _lastShiftPress = now;
        return false;
    }

    public void Reset()
    {
        _lastShiftPress = DateTime.MinValue;
    }
}

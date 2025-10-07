using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ConfigButtonDisplay.Core.Configuration;

/// <summary>
/// 窗口配置
/// </summary>
public class WindowSettings : INotifyPropertyChanged
{
    private string _position = "RightEdge";
    private int? _customX;
    private int? _customY;
    private bool _rememberPosition = true;
    private double _opacity = 0.95;
    private bool _alwaysOnTop = true;

    /// <summary>
    /// 窗口位置：RightEdge（右侧边缘）、LeftEdge（左侧边缘）、Custom（自定义）
    /// </summary>
    public string Position
    {
        get => _position;
        set
        {
            if (_position != value)
            {
                _position = value;
                OnPropertyChanged();
            }
        }
    }
    
    /// <summary>
    /// 自定义 X 坐标（当 Position 为 Custom 时使用）
    /// </summary>
    public int? CustomX
    {
        get => _customX;
        set
        {
            if (_customX != value)
            {
                _customX = value;
                OnPropertyChanged();
            }
        }
    }
    
    /// <summary>
    /// 自定义 Y 坐标（当 Position 为 Custom 时使用）
    /// </summary>
    public int? CustomY
    {
        get => _customY;
        set
        {
            if (_customY != value)
            {
                _customY = value;
                OnPropertyChanged();
            }
        }
    }
    
    /// <summary>
    /// 是否记住窗口位置
    /// </summary>
    public bool RememberPosition
    {
        get => _rememberPosition;
        set
        {
            if (_rememberPosition != value)
            {
                _rememberPosition = value;
                OnPropertyChanged();
            }
        }
    }
    
    /// <summary>
    /// 窗口透明度（0.0 - 1.0）
    /// </summary>
    public double Opacity
    {
        get => _opacity;
        set
        {
            // 限制范围在 0.5 到 1.0 之间
            var clampedValue = Math.Max(0.5, Math.Min(1.0, value));
            if (Math.Abs(_opacity - clampedValue) > 0.001)
            {
                _opacity = clampedValue;
                OnPropertyChanged();
            }
        }
    }
    
    /// <summary>
    /// 是否始终置顶
    /// </summary>
    public bool AlwaysOnTop
    {
        get => _alwaysOnTop;
        set
        {
            if (_alwaysOnTop != value)
            {
                _alwaysOnTop = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

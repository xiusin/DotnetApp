using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ConfigButtonDisplay.Core.Configuration;

/// <summary>
/// 键盘监控配置
/// </summary>
public class KeyboardMonitorSettings : INotifyPropertyChanged
{
    private bool _enabled = true;
    private string _displayPosition = "BottomCenter";
    private int? _customDisplayX;
    private int? _customDisplayY;
    private string _backgroundColor = "#3182CE";
    private double _opacity = 0.9;
    private int _fontSize = 28;
    private string _fontColor = "#FFFFFF";
    private double _displayDuration = 2.0;
    private double _fadeInDuration = 0.2;
    private double _fadeOutDuration = 0.3;
    private bool _showModifiers = true;
    private bool _showFunctionKeys = true;
    private bool _showAlphaNumeric = true;
    private bool _showNavigation = true;

    /// <summary>
    /// 是否启用键盘监控
    /// </summary>
    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled != value)
            {
                _enabled = value;
                OnPropertyChanged();
            }
        }
    }
    
    /// <summary>
    /// 显示位置：TopLeft、TopCenter、TopRight、BottomLeft、BottomCenter、BottomRight、Custom
    /// </summary>
    public string DisplayPosition
    {
        get => _displayPosition;
        set
        {
            if (_displayPosition != value)
            {
                _displayPosition = value;
                OnPropertyChanged();
            }
        }
    }
    
    /// <summary>
    /// 自定义显示位置 X 坐标
    /// </summary>
    public int? CustomDisplayX
    {
        get => _customDisplayX;
        set
        {
            if (_customDisplayX != value)
            {
                _customDisplayX = value;
                OnPropertyChanged();
            }
        }
    }
    
    /// <summary>
    /// 自定义显示位置 Y 坐标
    /// </summary>
    public int? CustomDisplayY
    {
        get => _customDisplayY;
        set
        {
            if (_customDisplayY != value)
            {
                _customDisplayY = value;
                OnPropertyChanged();
            }
        }
    }
    
    /// <summary>
    /// 背景颜色（Hex 格式，如 #3182CE）
    /// </summary>
    public string BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            if (_backgroundColor != value)
            {
                _backgroundColor = value;
                OnPropertyChanged();
            }
        }
    }
    
    /// <summary>
    /// 背景透明度（0.0 - 1.0）
    /// </summary>
    public double Opacity
    {
        get => _opacity;
        set
        {
            // 限制范围在 0.1 到 1.0 之间
            var clampedValue = Math.Max(0.1, Math.Min(1.0, value));
            if (Math.Abs(_opacity - clampedValue) > 0.001)
            {
                _opacity = clampedValue;
                OnPropertyChanged();
            }
        }
    }
    
    /// <summary>
    /// 字体大小（12 - 48）
    /// </summary>
    public int FontSize
    {
        get => _fontSize;
        set
        {
            // 限制范围在 12 到 48 之间
            var clampedValue = Math.Max(12, Math.Min(48, value));
            if (_fontSize != clampedValue)
            {
                _fontSize = clampedValue;
                OnPropertyChanged();
            }
        }
    }
    
    /// <summary>
    /// 字体颜色（Hex 格式，如 #FFFFFF）
    /// </summary>
    public string FontColor
    {
        get => _fontColor;
        set
        {
            if (_fontColor != value)
            {
                _fontColor = value;
                OnPropertyChanged();
            }
        }
    }
    
    /// <summary>
    /// 显示时长（秒，1.0 - 10.0）
    /// </summary>
    public double DisplayDuration
    {
        get => _displayDuration;
        set
        {
            // 限制范围在 1.0 到 10.0 之间
            var clampedValue = Math.Max(1.0, Math.Min(10.0, value));
            if (Math.Abs(_displayDuration - clampedValue) > 0.001)
            {
                _displayDuration = clampedValue;
                OnPropertyChanged();
            }
        }
    }
    
    /// <summary>
    /// 淡入动画时长（秒，0.1 - 1.0）
    /// </summary>
    public double FadeInDuration
    {
        get => _fadeInDuration;
        set
        {
            // 限制范围在 0.1 到 1.0 之间
            var clampedValue = Math.Max(0.1, Math.Min(1.0, value));
            if (Math.Abs(_fadeInDuration - clampedValue) > 0.001)
            {
                _fadeInDuration = clampedValue;
                OnPropertyChanged();
            }
        }
    }
    
    /// <summary>
    /// 淡出动画时长（秒，0.1 - 1.0）
    /// </summary>
    public double FadeOutDuration
    {
        get => _fadeOutDuration;
        set
        {
            // 限制范围在 0.1 到 1.0 之间
            var clampedValue = Math.Max(0.1, Math.Min(1.0, value));
            if (Math.Abs(_fadeOutDuration - clampedValue) > 0.001)
            {
                _fadeOutDuration = clampedValue;
                OnPropertyChanged();
            }
        }
    }
    
    /// <summary>
    /// 是否显示修饰键（Ctrl、Alt、Shift、Win）
    /// </summary>
    public bool ShowModifiers
    {
        get => _showModifiers;
        set
        {
            if (_showModifiers != value)
            {
                _showModifiers = value;
                OnPropertyChanged();
            }
        }
    }
    
    /// <summary>
    /// 是否显示功能键（F1-F12）
    /// </summary>
    public bool ShowFunctionKeys
    {
        get => _showFunctionKeys;
        set
        {
            if (_showFunctionKeys != value)
            {
                _showFunctionKeys = value;
                OnPropertyChanged();
            }
        }
    }
    
    /// <summary>
    /// 是否显示字母数字键（A-Z、0-9）
    /// </summary>
    public bool ShowAlphaNumeric
    {
        get => _showAlphaNumeric;
        set
        {
            if (_showAlphaNumeric != value)
            {
                _showAlphaNumeric = value;
                OnPropertyChanged();
            }
        }
    }
    
    /// <summary>
    /// 是否显示导航键（方向键、Home、End 等）
    /// </summary>
    public bool ShowNavigation
    {
        get => _showNavigation;
        set
        {
            if (_showNavigation != value)
            {
                _showNavigation = value;
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

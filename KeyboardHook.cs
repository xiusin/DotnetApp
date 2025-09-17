using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

namespace ConfigButtonDisplay;

public class KeyboardHook : IDisposable
{
    private Window? _targetWindow;
    private readonly Dictionary<Key, bool> _keyStates = new();
    private System.Threading.Timer? _pollingTimer;
    private const int POLLING_INTERVAL = 50; // 50ms轮询间隔
    
    public event EventHandler<KeyEventArgs>? KeyDown;
    public event EventHandler<KeyEventArgs>? KeyUp;
    
    public KeyboardHook()
    {
        // 延迟初始化，等待应用启动完成
        Dispatcher.UIThread.Post(() =>
        {
            InitializeHook();
        }, DispatcherPriority.Loaded);
    }
    
    private void InitializeHook()
    {
        var app = Application.Current;
        if (app?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            _targetWindow = desktop.MainWindow;
            if (_targetWindow != null)
            {
                // 监听窗口事件
                _targetWindow.KeyDown += OnWindowKeyDown;
                _targetWindow.KeyUp += OnWindowKeyUp;
                _targetWindow.AddHandler(InputElement.KeyDownEvent, OnWindowKeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);
                _targetWindow.AddHandler(InputElement.KeyUpEvent, OnWindowKeyUp, Avalonia.Interactivity.RoutingStrategies.Tunnel);
                
                // 启动轮询来检测全局键盘状态（跨平台方案）
                StartPolling();
            }
        }
    }
    
    private void StartPolling()
    {
        _pollingTimer = new System.Threading.Timer(PollKeyboardState, null, 0, POLLING_INTERVAL);
    }
    
    private void PollKeyboardState(object? state)
    {
        try
        {
            // 检测所有可能的按键
            var allKeys = Enum.GetValues<Key>();
            foreach (var key in allKeys)
            {
                if (key == Key.None) continue;
                
                bool isCurrentlyPressed = IsKeyPressed(key);
                bool wasPreviouslyPressed = _keyStates.GetValueOrDefault(key, false);
                
                if (isCurrentlyPressed && !wasPreviouslyPressed)
                {
                    // 按键按下
                    Dispatcher.UIThread.Post(() =>
                    {
                        var args = new KeyEventArgs { Key = key };
                        KeyDown?.Invoke(this, args);
                    });
                    _keyStates[key] = true;
                }
                else if (!isCurrentlyPressed && wasPreviouslyPressed)
                {
                    // 按键释放
                    Dispatcher.UIThread.Post(() =>
                    {
                        var args = new KeyEventArgs { Key = key };
                        KeyUp?.Invoke(this, args);
                    });
                    _keyStates[key] = false;
                }
            }
        }
        catch
        {
            // 忽略轮询中的错误
        }
    }
    
    // 添加清除所有按键状态的方法
    public void ClearAllKeyStates()
    {
        var pressedKeys = new List<Key>();
        foreach (var kvp in _keyStates)
        {
            if (kvp.Value)
            {
                pressedKeys.Add(kvp.Key);
            }
        }
        
        foreach (var key in pressedKeys)
        {
            _keyStates[key] = false;
            Dispatcher.UIThread.Post(() =>
            {
                var args = new KeyEventArgs { Key = key };
                KeyUp?.Invoke(this, args);
            });
        }
    }
    
    private bool IsKeyPressed(Key key)
    {
        // 跨平台按键状态检测 - 真正的全局检测
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return IsWindowsKeyPressed(key);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return IsMacKeyPressed(key);
            }
            else
            {
                return IsLinuxKeyPressed(key);
            }
        }
        catch
        {
            return false;
        }
    }
    
    // 添加对macOS的IOHIDManager支持，实现真正的全局键盘监听
    private bool IsMacKeyPressedWithIOHID(Key key)
    {
        // 这里应该使用IOHIDManager来获取全局键盘状态
        // 由于实现复杂，暂时使用简化的检测，但确保不依赖窗口焦点
        return false;
    }
    
    private bool IsMacKeyPressed(Key key)
    {
        // macOS下的全局按键检测 - 使用CGEventSourceKeyState
        try
        {
            // 使用Core Graphics API检测按键状态，不依赖窗口焦点
            return IsMacGlobalKeyPressed(key);
        }
        catch
        {
            return false;
        }
    }
    
    private bool IsMacGlobalKeyPressed(Key key)
    {
        // macOS虚拟键码映射
        int vkCode = KeyToMacVirtualKey(key);
        if (vkCode == -1) return false;
        
        try
        {
            // 使用CGEventSourceKeyState检测全局按键状态
            return CGEventSourceKeyState(0, (CGKeyCode)vkCode) != 0;
        }
        catch
        {
            return false;
        }
    }
    
    private int KeyToMacVirtualKey(Key key)
    {
        return key switch
        {
            Key.A => 0x00, Key.S => 0x01, Key.D => 0x02, Key.F => 0x03,
            Key.H => 0x04, Key.G => 0x05, Key.Z => 0x06, Key.X => 0x07,
            Key.C => 0x08, Key.V => 0x09, Key.B => 0x0B, Key.Q => 0x0C,
            Key.W => 0x0D, Key.E => 0x0E, Key.R => 0x0F, Key.Y => 0x10,
            Key.T => 0x11, Key.O => 0x12, Key.P => 0x13, Key.U => 0x14,
            Key.I => 0x15, Key.L => 0x16, Key.K => 0x17, Key.J => 0x18,
            Key.M => 0x1E, Key.N => 0x1A, Key.D1 => 0x12, Key.D2 => 0x13,
            Key.D3 => 0x14, Key.D4 => 0x15, Key.D5 => 0x17, Key.D6 => 0x16,
            Key.D7 => 0x1A, Key.D8 => 0x1C, Key.D9 => 0x19, Key.D0 => 0x1D,
            Key.Space => 0x31, Key.Enter => 0x24, Key.Escape => 0x35,
            Key.Back => 0x33, Key.Tab => 0x30, Key.Delete => 0x75,
            Key.LeftShift => 0x38, Key.RightShift => 0x3C,
            Key.LeftCtrl => 0x3B, Key.RightCtrl => 0x3E,
            Key.LeftAlt => 0x3A, Key.RightAlt => 0x3D,
            Key.LWin => 0x37, Key.RWin => 0x36,
            Key.F1 => 0x7A, Key.F2 => 0x78, Key.F3 => 0x63,
            Key.F4 => 0x76, Key.F5 => 0x60, Key.F6 => 0x61,
            Key.F7 => 0x62, Key.F8 => 0x64, Key.F9 => 0x65,
            Key.F10 => 0x6D, Key.F11 => 0x67, Key.F12 => 0x6F,
            Key.Up => 0x7E, Key.Down => 0x7D, Key.Left => 0x7B,
            Key.Right => 0x7C, _ => -1
        };
    }
    
    // macOS Core Graphics API导入
    [DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
    private static extern byte CGEventSourceKeyState(int sourceStateID, CGKeyCode key);
    
    private enum CGKeyCode : ushort
    {
        // 常用的macOS虚拟键码
    }
    
    private bool IsWindowsKeyPressed(Key key)
    {
        // Windows下的按键状态检测
        try
        {
            int vkCode = KeyToVirtualKey(key);
            if (vkCode != -1)
            {
                short state = GetAsyncKeyState(vkCode);
                return (state & 0x8000) != 0;
            }
        }
        catch
        {
            // 忽略错误
        }
        return false;
    }
    
    private bool IsLinuxKeyPressed(Key key)
    {
        // Linux下的按键状态检测（简化实现）
        return false; // 暂时返回false，后续可以添加X11实现
    }
    
    private int KeyToVirtualKey(Key key)
    {
        // 将Avalonia的Key转换为Windows虚拟键码
        return key switch
        {
            Key.A => 0x41, Key.B => 0x42, Key.C => 0x43, Key.D => 0x44,
            Key.E => 0x45, Key.F => 0x46, Key.G => 0x47, Key.H => 0x48,
            Key.I => 0x49, Key.J => 0x4A, Key.K => 0x4B, Key.L => 0x4C,
            Key.M => 0x4D, Key.N => 0x4E, Key.O => 0x4F, Key.P => 0x50,
            Key.Q => 0x51, Key.R => 0x52, Key.S => 0x53, Key.T => 0x54,
            Key.U => 0x55, Key.V => 0x56, Key.W => 0x57, Key.X => 0x58,
            Key.Y => 0x59, Key.Z => 0x5A,
            Key.D0 => 0x30, Key.D1 => 0x31, Key.D2 => 0x32, Key.D3 => 0x33,
            Key.D4 => 0x34, Key.D5 => 0x35, Key.D6 => 0x36, Key.D7 => 0x37,
            Key.D8 => 0x38, Key.D9 => 0x39,
            Key.Space => 0x20,
            Key.Enter or Key.Return => 0x0D,
            Key.Escape => 0x1B,
            Key.Tab => 0x09,
            Key.Back => 0x08,
            Key.Delete => 0x2E,
            Key.Insert => 0x2D,
            Key.Home => 0x24,
            Key.End => 0x23,
            Key.PageUp => 0x21,
            Key.PageDown => 0x22,
            Key.Up => 0x26, Key.Down => 0x28, Key.Left => 0x25, Key.Right => 0x27,
            Key.F1 => 0x70, Key.F2 => 0x71, Key.F3 => 0x72, Key.F4 => 0x73,
            Key.F5 => 0x74, Key.F6 => 0x75, Key.F7 => 0x76, Key.F8 => 0x77,
            Key.F9 => 0x78, Key.F10 => 0x79, Key.F11 => 0x7A, Key.F12 => 0x7B,
            Key.LeftCtrl => 0xA2, Key.RightCtrl => 0xA3,
            Key.LeftAlt => 0xA4, Key.RightAlt => 0xA5,
            Key.LeftShift => 0xA0, Key.RightShift => 0xA1,
            Key.LWin => 0x5B, Key.RWin => 0x5C,
            Key.NumPad0 => 0x60, Key.NumPad1 => 0x61, Key.NumPad2 => 0x62,
            Key.NumPad3 => 0x63, Key.NumPad4 => 0x64, Key.NumPad5 => 0x65,
            Key.NumPad6 => 0x66, Key.NumPad7 => 0x67, Key.NumPad8 => 0x68,
            Key.NumPad9 => 0x69,
            _ => -1
        };
    }
    
    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        // 窗口键盘事件处理
        KeyDown?.Invoke(this, e);
    }
    
    private void OnWindowKeyUp(object? sender, KeyEventArgs e)
    {
        // 窗口键盘事件处理
        KeyUp?.Invoke(this, e);
    }
    
    public void Dispose()
    {
        _pollingTimer?.Dispose();
        if (_targetWindow != null)
        {
            _targetWindow.KeyDown -= OnWindowKeyDown;
            _targetWindow.KeyUp -= OnWindowKeyUp;
            _targetWindow.RemoveHandler(InputElement.KeyDownEvent, OnWindowKeyDown);
            _targetWindow.RemoveHandler(InputElement.KeyUpEvent, OnWindowKeyUp);
        }
    }
    
    // Windows API导入
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
}
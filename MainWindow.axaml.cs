using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;
using System;
using Avalonia;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Avalonia.Markup.Xaml;

namespace ConfigButtonDisplay;

public partial class MainWindow : Window
{
    private KeyboardHook? _keyboardHook;
    private bool _isListening = false;
    private KeyDisplayWindow? _keyDisplayWindow;
    private System.Threading.Timer? _hideTimer;
    
    // 当前按下的键
    private readonly HashSet<Key> _pressedKeys = new();
    
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
        InitializeComponent();
        InitializeKeyboardHook();
        InitializeSystemTray();
        UpdatePreview();
    }
    
    private void InitializeKeyboardHook()
    {
        try
        {
            _keyboardHook = new KeyboardHook();
            _keyboardHook.KeyDown += OnKeyDown;
            _keyboardHook.KeyUp += OnKeyUp;
            
            // 程序启动后立即开始监听
            _isListening = true;
            if (ListeningToggle != null)
                ListeningToggle.IsChecked = true;
            if (StatusText != null)
                StatusText.Text = "监听中...";
        }
        catch (Exception ex)
        {
            if (StatusText != null)
                StatusText.Text = $"键盘钩子初始化失败: {ex.Message}";
        }
    }
    
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!_isListening) return;
        
        Dispatcher.UIThread.Post(() =>
        {
            _pressedKeys.Add(e.Key);
            UpdateKeyDisplay();
        });
    }
    
    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        if (!_isListening) return;
        
        Dispatcher.UIThread.Post(() =>
        {
            _pressedKeys.Remove(e.Key);
            UpdateKeyDisplay();
        });
    }
    
    private void UpdateKeyDisplay()
    {
        if (_pressedKeys.Count == 0)
        {
            HideKeyDisplay();
            return;
        }
        
        var keyText = FormatKeyCombination(_pressedKeys);
        if (string.IsNullOrEmpty(keyText))
        {
            HideKeyDisplay();
            return;
        }
        
        ShowKeyDisplay(keyText);
        
        // 立即更新显示，不延迟隐藏
        CancelHideTimer();
    }
    
    private string FormatKeyCombination(HashSet<Key> keys)
    {
        var parts = new List<string>();
        
        // 只显示有实际按键的组合，避免只显示修饰键
        var normalKeys = keys.Where(k => !IsModifierKey(k)).ToList();
        if (normalKeys.Count == 0)
        {
            return ""; // 没有实际按键时不显示任何内容
        }
        
        // 检查修饰键（只在有实际按键时显示）
        if (keys.Contains(Key.LeftCtrl) || keys.Contains(Key.RightCtrl))
            parts.Add("Ctrl");
        if (keys.Contains(Key.LeftAlt) || keys.Contains(Key.RightAlt))
            parts.Add("Alt");
        if (keys.Contains(Key.LeftShift) || keys.Contains(Key.RightShift))
            parts.Add("Shift");
        if (keys.Contains(Key.LWin) || keys.Contains(Key.RWin))
            parts.Add("Win");
        
        // 添加普通按键
        foreach (var key in normalKeys)
        {
            parts.Add(FormatKeyName(key));
        }
        
        return string.Join(" + ", parts);
    }
    
    private bool IsModifierKey(Key key)
    {
        return key is Key.LeftCtrl or Key.RightCtrl or
               Key.LeftAlt or Key.RightAlt or
               Key.LeftShift or Key.RightShift or
               Key.LWin or Key.RWin;
    }
    
    private string FormatKeyName(Key key)
    {
        return key switch
        {
            Key.Space => "Space",
            Key.Enter or Key.Return => "Enter",
            Key.Escape => "Esc",
            Key.Tab => "Tab",
            Key.Back => "⌫",
            Key.Delete => "Del",
            Key.Insert => "Ins",
            Key.Home => "Home",
            Key.End => "End",
            Key.PageUp => "PgUp",
            Key.PageDown => "PgDn",
            Key.Up => "↑",
            Key.Down => "↓",
            Key.Left => "←",
            Key.Right => "→",
            Key.OemComma => ",",
            Key.OemPeriod => ".",
            Key.OemQuestion => "/",
            Key.OemSemicolon => ";",
            Key.OemQuotes => "\"",
            Key.OemTilde => "`",
            Key.OemOpenBrackets => "[",
            Key.OemCloseBrackets => "]",
            Key.OemPipe => "\\",
            Key.OemPlus => "+",
            Key.OemMinus => "-",
            >= Key.F1 and <= Key.F12 => key.ToString(),
            >= Key.D0 and <= Key.D9 => key.ToString().Substring(1), // 移除D前缀
            >= Key.A and <= Key.Z => key.ToString().ToUpper(),
            _ => key.ToString()
        };
    }
    
    private void ShowKeyDisplay(string keyText)
    {
        CancelHideTimer();
        
        if (_keyDisplayWindow == null)
        {
            _keyDisplayWindow = new KeyDisplayWindow();
            
            // 设置位置为屏幕左下角，稍微调整位置
            var screens = Screens?.All;
            if (screens != null && screens.Count > 0)
            {
                var primaryScreen = screens[0];
                var screenBounds = primaryScreen.Bounds;
                
                _keyDisplayWindow.Position = new PixelPoint(
                    screenBounds.X + 50,
                    screenBounds.Y + screenBounds.Height - 120
                );
            }
            
            _keyDisplayWindow.Show();
        }
        
        // 更新内容
        _keyDisplayWindow.UpdateContent(keyText, GetSelectedColor(), GetFontSize());
        
        // 确保窗口可见
        if (!_keyDisplayWindow.IsVisible)
        {
            _keyDisplayWindow.Show();
        }
        
        // 更新预览
        if (PreviewText != null)
            PreviewText.Text = keyText;
        if (StatusText != null)
            StatusText.Text = $"显示按键: {keyText}";
    }
    
    private void HideKeyDisplay()
    {
        // 清空显示内容但保持窗口存在
        if (_keyDisplayWindow != null)
        {
            _keyDisplayWindow.UpdateContent("", GetSelectedColor(), GetFontSize());
        }
        if (StatusText != null)
            StatusText.Text = "监听中...";
    }
    
    private void StartHideTimer()
    {
        CancelHideTimer();
        var duration = (int)(GetDisplayDuration() * 1000);
        _hideTimer = new System.Threading.Timer(_ =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                HideKeyDisplay();
            });
        }, null, duration, Timeout.Infinite);
    }
    
    private void CancelHideTimer()
    {
        _hideTimer?.Dispose();
        _hideTimer = null;
    }
    
    private void ListeningToggle_Toggled(object? sender, RoutedEventArgs e)
    {
        _isListening = ListeningToggle?.IsChecked ?? false;
        
        if (_isListening)
        {
            if (StatusText != null)
                StatusText.Text = "监听中...";
            _pressedKeys.Clear();
            HideKeyDisplay();
        }
        else
        {
            if (StatusText != null)
                StatusText.Text = "监听已停止";
            _pressedKeys.Clear();
            HideKeyDisplay();
            CancelHideTimer();
        }
    }
    
    private void TestButton_Click(object? sender, RoutedEventArgs e)
    {
        ShowKeyDisplay("Ctrl + Shift + A");
    }
    
    private void HideButton_Click(object? sender, RoutedEventArgs e)
    {
        HideToBackground();
    }
    
    private void BackgroundColorCombo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdatePreview();
    }
    
    private void DisplayDurationSlider_ValueChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        // 可以在这里添加实时更新逻辑
    }
    
    private void FontSizeSlider_ValueChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        UpdatePreview();
    }
    
    private void UpdatePreview()
    {
        if (PreviewText != null && PreviewBorder != null)
        {
            PreviewText.FontSize = GetFontSize();
            PreviewBorder.Background = new SolidColorBrush(GetSelectedColor());
        }
    }
    
    private Color GetSelectedColor()
    {
        var selectedIndex = BackgroundColorCombo?.SelectedIndex ?? 0;
        return selectedIndex switch
        {
            0 => Color.Parse("#0078D4"), // 蓝色
            1 => Color.Parse("#107C10"), // 绿色
            2 => Color.Parse("#D13438"), // 红色
            3 => Color.Parse("#8764B8"), // 紫色
            4 => Color.Parse("#FF8C00"), // 橙色
            _ => Color.Parse("#0078D4")
        };
    }
    
    private double GetDisplayDuration()
    {
        return DisplayDurationSlider?.Value ?? 2.0;
    }
    
    private double GetFontSize()
    {
        return FontSizeSlider?.Value ?? 24.0;
    }
    
    private void InitializeSystemTray()
    {
        // 简化实现：支持最小化到任务栏并隐藏窗口
        // 键盘监听将在后台继续运行
    }
    
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _keyboardHook?.Dispose();
        _keyDisplayWindow?.Close();
        CancelHideTimer();
    }
    
    // 添加隐藏窗口但保持监听的功能
    public void HideToBackground()
    {
        Hide();
        // 键盘监听会继续在后台运行
    }
    
    public void ShowFromBackground()
    {
        Show();
        Activate();
    }
}
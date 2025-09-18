using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using System;

namespace ConfigButtonDisplay;

public class ConfigPopover : IDisposable
{
    private Popup? _configPopup;
    private Window? _parentWindow;
    
    // 配置项
    private ToggleSwitch? _autoStartSwitch;
    private ToggleSwitch? _minimizeToTraySwitch;
    private ToggleSwitch? _showNotificationsSwitch;
    
    // 事件
    public event EventHandler<bool>? AutoStartChanged;
    public event EventHandler<bool>? MinimizeToTrayChanged;
    public event EventHandler<bool>? ShowNotificationsChanged;
    
    public ConfigPopover(Window parentWindow)
    {
        _parentWindow = parentWindow;
        InitializePopup();
    }
    
    private void InitializePopup()
    {
        _configPopup = new Popup
        {
            IsLightDismissEnabled = true,
            HorizontalOffset = -10,
            VerticalOffset = 10,
            MaxWidth = 320
        };
        
        // 主容器
        var mainBorder = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#FFFFFF")),
            BorderBrush = new SolidColorBrush(Color.Parse("#E6E8EA")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(20)
        };
        
        var contentStack = new StackPanel
        {
            Spacing = 16
        };
        
        // 标题
        var titleText = new TextBlock
        {
            Text = "配置设置",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#1C1E21")),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8)
        };
        
        contentStack.Children.Add(titleText);
        
        // 基本设置
        AddSettingsGroup(contentStack, "基本设置");
        AddToggleSetting(contentStack, "开机自启", "系统启动时自动运行", true, OnAutoStartChanged, out _autoStartSwitch);
        AddToggleSetting(contentStack, "最小化到托盘", "关闭窗口时最小化到系统托盘", true, OnMinimizeToTrayChanged, out _minimizeToTraySwitch);
        AddToggleSetting(contentStack, "显示通知", "显示按键监听状态通知", false, OnShowNotificationsChanged, out _showNotificationsSwitch);
        
        // 关于信息
        var separator = new Border
        {
            Height = 1,
            Background = new SolidColorBrush(Color.Parse("#E9ECEF")),
            Margin = new Thickness(0, 8, 0, 8)
        };
        contentStack.Children.Add(separator);
        
        var aboutText = new TextBlock
        {
            Text = "键盘按键显示器 v1.0.0\n基于 Semi Design 和 Avalonia UI",
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.Parse("#6C757D")),
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        };
        contentStack.Children.Add(aboutText);
        
        mainBorder.Child = contentStack;
        _configPopup.Child = mainBorder;
    }
    
    private void AddSettingsGroup(StackPanel parent, string groupName)
    {
        var groupText = new TextBlock
        {
            Text = groupName,
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(Color.Parse("#495057")),
            Margin = new Thickness(0, 8, 0, 4)
        };
        parent.Children.Add(groupText);
    }
    
    private void AddToggleSetting(StackPanel parent, string title, string description, bool defaultValue, 
        EventHandler<RoutedEventArgs> handler, out ToggleSwitch toggleSwitch)
    {
        var settingGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto")
        };
        
        var textStack = new StackPanel
        {
            Spacing = 2
        };
        
        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 13,
            FontWeight = FontWeight.Medium,
            Foreground = new SolidColorBrush(Color.Parse("#1C1E21"))
        };
        
        var descText = new TextBlock
        {
            Text = description,
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.Parse("#6C757D")),
            TextWrapping = TextWrapping.Wrap
        };
        
        textStack.Children.Add(titleText);
        textStack.Children.Add(descText);
        
        toggleSwitch = new ToggleSwitch
        {
            IsChecked = defaultValue,
            OnContent = "开启",
            OffContent = "关闭",
            Width = 50
        };
        toggleSwitch.IsCheckedChanged += handler;
        
        Grid.SetColumn(textStack, 0);
        Grid.SetColumn(toggleSwitch, 1);
        
        settingGrid.Children.Add(textStack);
        settingGrid.Children.Add(toggleSwitch);
        
        parent.Children.Add(settingGrid);
    }
    
    // 事件处理程序
    private void OnAutoStartChanged(object? sender, RoutedEventArgs e)
    {
        if (_autoStartSwitch != null)
        {
            AutoStartChanged?.Invoke(this, _autoStartSwitch.IsChecked ?? false);
        }
    }
    
    private void OnMinimizeToTrayChanged(object? sender, RoutedEventArgs e)
    {
        if (_minimizeToTraySwitch != null)
        {
            MinimizeToTrayChanged?.Invoke(this, _minimizeToTraySwitch.IsChecked ?? false);
        }
    }
    
    private void OnShowNotificationsChanged(object? sender, RoutedEventArgs e)
    {
        if (_showNotificationsSwitch != null)
        {
            ShowNotificationsChanged?.Invoke(this, _showNotificationsSwitch.IsChecked ?? false);
        }
    }
    
    public void ShowConfig(Point position)
    {
        if (_configPopup != null && _parentWindow != null)
        {
            _configPopup.PlacementTarget = _parentWindow;
            _configPopup.HorizontalOffset = position.X;
            _configPopup.VerticalOffset = position.Y;
            _configPopup.IsOpen = true;
        }
    }
    
    public void HideConfig()
    {
        if (_configPopup != null)
        {
            _configPopup.IsOpen = false;
        }
    }
    
    public void Dispose()
    {
        HideConfig();
        _configPopup = null;
        _parentWindow = null;
    }
}
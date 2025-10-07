using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace ConfigButtonDisplay.Views;

public partial class ConfigWindow : Window
{
    public ConfigWindow()
    {
        AvaloniaXamlLoader.Load(this);
        
        // 初始化窗口拖拽
        InitializeWindowDrag();
        
        // 订阅 DataContext 变化以加载设置
        this.DataContextChanged += OnDataContextChanged;
    }
    
    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (this.DataContext is ViewModels.ConfigViewModel viewModel)
        {
            // 加载键盘监控设置到面板
            var keyboardPanel = this.FindControl<Panels.KeyboardMonitorPanel>("KeyboardMonitorPanel");
            if (keyboardPanel != null && viewModel.AppSettings?.KeyboardMonitor != null)
            {
                keyboardPanel.SetSettings(viewModel.AppSettings.KeyboardMonitor);
                Console.WriteLine("Keyboard monitor settings loaded to panel");
            }
        }
    }
    
    /// <summary>
    /// 初始化窗口拖拽功能
    /// </summary>
    private void InitializeWindowDrag()
    {
        // 为标题栏区域添加拖拽功能
        this.PointerPressed += (sender, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                this.BeginMoveDrag(e);
            }
        };
    }
    
    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
    
    /// <summary>
    /// 保存按钮点击事件
    /// </summary>
    private async void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            Console.WriteLine("Save button clicked");
            
            // 获取 ViewModel
            if (this.DataContext is ViewModels.ConfigViewModel viewModel)
            {
                // 获取键盘监控面板的设置
                var keyboardPanel = this.FindControl<Panels.KeyboardMonitorPanel>("KeyboardMonitorPanel");
                if (keyboardPanel != null)
                {
                    var settings = keyboardPanel.GetSettings();
                    viewModel.AppSettings.KeyboardMonitor = settings;
                    Console.WriteLine($"Keyboard monitor settings updated: Position={settings.DisplayPosition}, FontSize={settings.FontSize}");
                }
                
                // 保存配置
                await viewModel.SaveAsync();
                Console.WriteLine("Configuration saved successfully");
            }
            
            this.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving configuration: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 取消按钮点击事件
    /// </summary>
    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
}

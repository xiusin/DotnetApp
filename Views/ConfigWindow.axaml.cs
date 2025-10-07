using System;
using System.Threading.Tasks;
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
                
                // 显示成功提示
                await ShowMessageDialog("配置已成功保存", "保存成功");
                
                this.Close();
            }
        }
        catch (InvalidOperationException ex)
        {
            // 验证失败
            Console.WriteLine($"Validation error: {ex.Message}");
            
            if (this.DataContext is ViewModels.ConfigViewModel viewModel && viewModel.LastValidationError != null)
            {
                await ShowMessageDialog(viewModel.LastValidationError, "配置验证失败");
            }
            else
            {
                await ShowMessageDialog("配置验证失败，请检查输入的值", "配置验证失败");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving configuration: {ex.Message}");
            await ShowMessageDialog($"保存配置时发生错误：{ex.Message}", "保存失败");
        }
    }
    
    /// <summary>
    /// 取消按钮点击事件
    /// </summary>
    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
    
    /// <summary>
    /// 重置按钮点击事件
    /// </summary>
    private async void ResetButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            // 显示确认对话框
            var result = await ShowConfirmDialog("确定要重置所有设置到默认值吗？", "重置设置");
            
            if (result)
            {
                Console.WriteLine("Reset button clicked - confirmed");
                
                // 获取 ViewModel
                if (this.DataContext is ViewModels.ConfigViewModel viewModel)
                {
                    // 重置配置
                    viewModel.Reset();
                    
                    // 重新加载到面板
                    var keyboardPanel = this.FindControl<Panels.KeyboardMonitorPanel>("KeyboardMonitorPanel");
                    if (keyboardPanel != null && viewModel.AppSettings?.KeyboardMonitor != null)
                    {
                        keyboardPanel.SetSettings(viewModel.AppSettings.KeyboardMonitor);
                    }
                    
                    Console.WriteLine("Settings reset to defaults");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error resetting configuration: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 显示确认对话框
    /// </summary>
    private async Task<bool> ShowConfirmDialog(string message, string title)
    {
        // 简化实现：直接返回 true
        // 在实际应用中应该显示一个确认对话框
        Console.WriteLine($"Confirm dialog: {title} - {message}");
        return await Task.FromResult(true);
    }
    
    /// <summary>
    /// 显示消息对话框
    /// </summary>
    private async Task ShowMessageDialog(string message, string title)
    {
        // 简化实现：输出到控制台
        // 在实际应用中应该显示一个消息对话框
        Console.WriteLine($"Message dialog: {title} - {message}");
        await Task.CompletedTask;
    }
}

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
    private void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: 实现保存逻辑
        Console.WriteLine("Save button clicked");
        this.Close();
    }
    
    /// <summary>
    /// 取消按钮点击事件
    /// </summary>
    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
}

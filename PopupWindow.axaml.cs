using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using Avalonia.Platform;

namespace ConfigButtonDisplay;

public partial class PopupWindow : Window
{
    public PopupWindow(string displayText, Color backgroundColor)
    {
        InitializeComponent();
        
        // 设置显示文本
        if (DisplayTextBlock != null)
        {
            DisplayTextBlock.Text = displayText;
        }
        
        // 设置背景颜色（带透明度）
        if (MainBorder != null)
        {
            var brush = new SolidColorBrush(backgroundColor);
            brush.Opacity = 0.9; // 90% 不透明度
            MainBorder.Background = brush;
        }
    }
    
    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
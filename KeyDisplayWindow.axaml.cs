using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace ConfigButtonDisplay;

public partial class KeyDisplayWindow : Window
{
    public KeyDisplayWindow()
    {
        InitializeComponent();
        this.Closing += (s, e) => 
        {
            // 阻止关闭窗口
            e.Cancel = true;
        };
    }
    
    public void UpdateContent(string text, Color backgroundColor, double fontSize)
    {
        if (DisplayTextBlock != null)
        {
            DisplayTextBlock.Text = text;
            DisplayTextBlock.FontSize = fontSize;
        }
        
        // 保持固定的透明黑色背景
        if (MainBorder != null && !string.IsNullOrEmpty(text))
        {
            MainBorder.Background = new SolidColorBrush(Color.Parse("#AA000000"));
        }
        else if (MainBorder != null)
        {
            // 空内容时完全透明
            MainBorder.Background = new SolidColorBrush(Colors.Transparent);
        }
    }
}
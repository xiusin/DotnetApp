using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ConfigButtonDisplay.Core.Configuration;

namespace ConfigButtonDisplay.Views.Panels;

public partial class KeyboardMonitorPanel : UserControl
{
    public KeyboardMonitorPanel()
    {
        AvaloniaXamlLoader.Load(this);
        
        // 订阅控件事件以实现实时预览
        InitializePreview();
    }
    
    private void InitializePreview()
    {
        try
        {
            // 背景颜色变化
            var bgCombo = this.FindControl<ComboBox>("BackgroundColorComboBox");
            if (bgCombo != null)
            {
                bgCombo.SelectionChanged += (s, e) => UpdatePreview();
            }
            
            // 透明度变化
            var opacitySlider = this.FindControl<Slider>("OpacitySlider");
            if (opacitySlider != null)
            {
                opacitySlider.PropertyChanged += (s, e) =>
                {
                    if (e.Property.Name == "Value")
                    {
                        UpdatePreview();
                    }
                };
            }
            
            // 字体大小变化
            var fontSizeSlider = this.FindControl<Slider>("FontSizeSlider");
            if (fontSizeSlider != null)
            {
                fontSizeSlider.PropertyChanged += (s, e) =>
                {
                    if (e.Property.Name == "Value")
                    {
                        UpdatePreview();
                    }
                };
            }
            
            // 字体颜色变化
            var fontColorCombo = this.FindControl<ComboBox>("FontColorComboBox");
            if (fontColorCombo != null)
            {
                fontColorCombo.SelectionChanged += (s, e) => UpdatePreview();
            }
            
            // 初始预览
            UpdatePreview();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing preview: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 更新实时预览
    /// </summary>
    private void UpdatePreview()
    {
        try
        {
            var previewBorder = this.FindControl<Border>("PreviewBorder");
            var previewText = this.FindControl<TextBlock>("PreviewText");
            
            if (previewBorder == null || previewText == null)
            {
                return;
            }
            
            // 更新背景颜色
            var bgColor = GetSelectedBackgroundColor();
            previewBorder.Background = new SolidColorBrush(bgColor);
            
            // 更新透明度
            var opacity = this.FindControl<Slider>("OpacitySlider")?.Value ?? 0.9;
            previewBorder.Opacity = opacity;
            
            // 更新字体大小
            var fontSize = this.FindControl<Slider>("FontSizeSlider")?.Value ?? 28;
            previewText.FontSize = fontSize;
            
            // 更新字体颜色
            var fontColor = GetSelectedFontColor();
            previewText.Foreground = new SolidColorBrush(fontColor);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating preview: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 获取选中的背景颜色
    /// </summary>
    private Color GetSelectedBackgroundColor()
    {
        var combo = this.FindControl<ComboBox>("BackgroundColorComboBox");
        var index = combo?.SelectedIndex ?? 0;
        
        return index switch
        {
            0 => Color.Parse("#3182CE"), // 蓝色
            1 => Color.Parse("#38A169"), // 绿色
            2 => Color.Parse("#E53E3E"), // 红色
            3 => Color.Parse("#805AD5"), // 紫色
            4 => Color.Parse("#DD6B20"), // 橙色
            _ => Color.Parse("#3182CE")
        };
    }
    
    /// <summary>
    /// 获取选中的字体颜色
    /// </summary>
    private Color GetSelectedFontColor()
    {
        var combo = this.FindControl<ComboBox>("FontColorComboBox");
        var index = combo?.SelectedIndex ?? 0;
        
        return index switch
        {
            0 => Color.Parse("#FFFFFF"), // 白色
            1 => Color.Parse("#000000"), // 黑色
            2 => Color.Parse("#718096"), // 灰色
            _ => Color.Parse("#FFFFFF")
        };
    }
    
    /// <summary>
    /// 测试显示按钮点击事件
    /// </summary>
    private void TestDisplayButton_Click(object? sender, RoutedEventArgs e)
    {
        // 触发测试显示（需要从主窗口访问 KeyDisplayWindow）
        Console.WriteLine("Test display button clicked");
        
        // 这里可以通过事件或回调通知主窗口进行测试显示
        // 暂时只输出日志
    }
    
    /// <summary>
    /// 获取当前配置
    /// </summary>
    public KeyboardMonitorSettings GetSettings()
    {
        var settings = new KeyboardMonitorSettings();
        
        try
        {
            // 启用状态
            var enabledToggle = this.FindControl<ToggleSwitch>("EnabledToggle");
            settings.Enabled = enabledToggle?.IsChecked ?? true;
            
            // 显示位置
            var positionCombo = this.FindControl<ComboBox>("PositionComboBox");
            settings.DisplayPosition = (positionCombo?.SelectedIndex ?? 4) switch
            {
                0 => "TopLeft",
                1 => "TopCenter",
                2 => "TopRight",
                3 => "BottomLeft",
                4 => "BottomCenter",
                5 => "BottomRight",
                _ => "BottomCenter"
            };
            
            // 背景颜色
            var bgCombo = this.FindControl<ComboBox>("BackgroundColorComboBox");
            settings.BackgroundColor = (bgCombo?.SelectedIndex ?? 0) switch
            {
                0 => "#3182CE",
                1 => "#38A169",
                2 => "#E53E3E",
                3 => "#805AD5",
                4 => "#DD6B20",
                _ => "#3182CE"
            };
            
            // 透明度
            var opacitySlider = this.FindControl<Slider>("OpacitySlider");
            settings.Opacity = opacitySlider?.Value ?? 0.9;
            
            // 字体大小
            var fontSizeSlider = this.FindControl<Slider>("FontSizeSlider");
            settings.FontSize = (int)(fontSizeSlider?.Value ?? 28);
            
            // 字体颜色
            var fontColorCombo = this.FindControl<ComboBox>("FontColorComboBox");
            settings.FontColor = (fontColorCombo?.SelectedIndex ?? 0) switch
            {
                0 => "#FFFFFF",
                1 => "#000000",
                2 => "#718096",
                _ => "#FFFFFF"
            };
            
            // 显示时长
            var durationSlider = this.FindControl<Slider>("DisplayDurationSlider");
            settings.DisplayDuration = durationSlider?.Value ?? 2.0;
            
            // 淡入时长
            var fadeInSlider = this.FindControl<Slider>("FadeInSlider");
            settings.FadeInDuration = fadeInSlider?.Value ?? 0.2;
            
            // 淡出时长
            var fadeOutSlider = this.FindControl<Slider>("FadeOutSlider");
            settings.FadeOutDuration = fadeOutSlider?.Value ?? 0.3;
            
            // 按键过滤
            var showModifiers = this.FindControl<CheckBox>("ShowModifiersCheckBox");
            settings.ShowModifiers = showModifiers?.IsChecked ?? true;
            
            var showFunctionKeys = this.FindControl<CheckBox>("ShowFunctionKeysCheckBox");
            settings.ShowFunctionKeys = showFunctionKeys?.IsChecked ?? true;
            
            var showAlphaNumeric = this.FindControl<CheckBox>("ShowAlphaNumericCheckBox");
            settings.ShowAlphaNumeric = showAlphaNumeric?.IsChecked ?? true;
            
            var showNavigation = this.FindControl<CheckBox>("ShowNavigationCheckBox");
            settings.ShowNavigation = showNavigation?.IsChecked ?? true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting settings: {ex.Message}");
        }
        
        return settings;
    }
    
    /// <summary>
    /// 设置配置值
    /// </summary>
    public void SetSettings(KeyboardMonitorSettings settings)
    {
        if (settings == null)
        {
            return;
        }
        
        try
        {
            // 启用状态
            var enabledToggle = this.FindControl<ToggleSwitch>("EnabledToggle");
            if (enabledToggle != null)
            {
                enabledToggle.IsChecked = settings.Enabled;
            }
            
            // 显示位置
            var positionCombo = this.FindControl<ComboBox>("PositionComboBox");
            if (positionCombo != null)
            {
                positionCombo.SelectedIndex = settings.DisplayPosition switch
                {
                    "TopLeft" => 0,
                    "TopCenter" => 1,
                    "TopRight" => 2,
                    "BottomLeft" => 3,
                    "BottomCenter" => 4,
                    "BottomRight" => 5,
                    _ => 4
                };
            }
            
            // 背景颜色
            var bgCombo = this.FindControl<ComboBox>("BackgroundColorComboBox");
            if (bgCombo != null)
            {
                bgCombo.SelectedIndex = settings.BackgroundColor switch
                {
                    "#3182CE" => 0,
                    "#38A169" => 1,
                    "#E53E3E" => 2,
                    "#805AD5" => 3,
                    "#DD6B20" => 4,
                    _ => 0
                };
            }
            
            // 透明度
            var opacitySlider = this.FindControl<Slider>("OpacitySlider");
            if (opacitySlider != null)
            {
                opacitySlider.Value = settings.Opacity;
            }
            
            // 字体大小
            var fontSizeSlider = this.FindControl<Slider>("FontSizeSlider");
            if (fontSizeSlider != null)
            {
                fontSizeSlider.Value = settings.FontSize;
            }
            
            // 字体颜色
            var fontColorCombo = this.FindControl<ComboBox>("FontColorComboBox");
            if (fontColorCombo != null)
            {
                fontColorCombo.SelectedIndex = settings.FontColor switch
                {
                    "#FFFFFF" => 0,
                    "#000000" => 1,
                    "#718096" => 2,
                    _ => 0
                };
            }
            
            // 显示时长
            var durationSlider = this.FindControl<Slider>("DisplayDurationSlider");
            if (durationSlider != null)
            {
                durationSlider.Value = settings.DisplayDuration;
            }
            
            // 淡入时长
            var fadeInSlider = this.FindControl<Slider>("FadeInSlider");
            if (fadeInSlider != null)
            {
                fadeInSlider.Value = settings.FadeInDuration;
            }
            
            // 淡出时长
            var fadeOutSlider = this.FindControl<Slider>("FadeOutSlider");
            if (fadeOutSlider != null)
            {
                fadeOutSlider.Value = settings.FadeOutDuration;
            }
            
            // 按键过滤
            var showModifiers = this.FindControl<CheckBox>("ShowModifiersCheckBox");
            if (showModifiers != null)
            {
                showModifiers.IsChecked = settings.ShowModifiers;
            }
            
            var showFunctionKeys = this.FindControl<CheckBox>("ShowFunctionKeysCheckBox");
            if (showFunctionKeys != null)
            {
                showFunctionKeys.IsChecked = settings.ShowFunctionKeys;
            }
            
            var showAlphaNumeric = this.FindControl<CheckBox>("ShowAlphaNumericCheckBox");
            if (showAlphaNumeric != null)
            {
                showAlphaNumeric.IsChecked = settings.ShowAlphaNumeric;
            }
            
            var showNavigation = this.FindControl<CheckBox>("ShowNavigationCheckBox");
            if (showNavigation != null)
            {
                showNavigation.IsChecked = settings.ShowNavigation;
            }
            
            // 更新预览
            UpdatePreview();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting settings: {ex.Message}");
        }
    }
}

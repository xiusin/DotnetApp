# UI Fluent Design 快速参考指南

## 快速开始

### 创建新的设置卡片
```xml
<Border Background="#05FFFFFF" 
        CornerRadius="6" 
        Padding="16,12"
        BorderBrush="#0F323130" 
        BorderThickness="1">
    <StackPanel Spacing="10">
        <TextBlock Text="卡片标题" 
                  FontSize="12" 
                  FontWeight="SemiBold" 
                  Foreground="#FF323130"/>
        <!-- 卡片内容 -->
    </StackPanel>
</Border>
```

### 创建设置项（带Slider）
```xml
<Border Background="#0A000000" CornerRadius="4" Padding="10,6">
    <Grid ColumnDefinitions="100,*,60">
        <TextBlock Grid.Column="0"
                  Text="设置名称"
                  VerticalAlignment="Center"
                  FontSize="10"
                  Foreground="#FF323130"/>
        <Slider Grid.Column="1"
               Name="MySlider"
               Minimum="0" Maximum="100" Value="50"
               Margin="8,0"/>
        <Border Grid.Column="2"
                Background="#FF0078D4" 
                CornerRadius="6" 
                Padding="6,2">
            <TextBlock Text="{Binding #MySlider.Value, StringFormat='{}{0:F0}'}"
                      FontSize="9"
                      FontWeight="Medium"
                      Foreground="White"
                      HorizontalAlignment="Center"/>
        </Border>
    </Grid>
</Border>
```

### 创建设置项（带ComboBox）
```xml
<Border Background="#0A000000" CornerRadius="4" Padding="10,6">
    <Grid ColumnDefinitions="100,*">
        <TextBlock Grid.Column="0"
                  Text="设置名称"
                  VerticalAlignment="Center"
                  FontSize="10"
                  Foreground="#FF323130"/>
        <ComboBox Grid.Column="1"
                 Name="MyComboBox"
                 SelectedIndex="0"
                 FontSize="10"
                 Background="Transparent"
                 BorderThickness="0">
            <ComboBoxItem Content="选项 1"/>
            <ComboBoxItem Content="选项 2"/>
        </ComboBox>
    </Grid>
</Border>
```

### 创建主按钮
```xml
<Button Content="主要操作"
        Height="32"
        Background="#FF0078D4" 
        Foreground="White" 
        BorderThickness="0" 
        CornerRadius="6"
        FontSize="11"
        FontWeight="Medium">
    <Button.Styles>
        <Style Selector="Button:pointerover">
            <Setter Property="Background" Value="#FF106EBE"/>
        </Style>
        <Style Selector="Button:pressed">
            <Setter Property="Background" Value="#FF005A9E"/>
        </Style>
    </Button.Styles>
</Button>
```

### 创建次要按钮
```xml
<Button Content="次要操作"
        Height="32"
        Background="Transparent" 
        Foreground="#FF323130" 
        BorderBrush="#FF8A8886"
        BorderThickness="1" 
        CornerRadius="6"
        FontSize="11"
        FontWeight="Medium">
    <Button.Styles>
        <Style Selector="Button:pointerover">
            <Setter Property="Background" Value="#0A000000"/>
        </Style>
    </Button.Styles>
</Button>
```

### 创建带标题的卡片
```xml
<Border Background="#05FFFFFF" 
        CornerRadius="6" 
        Padding="16,12"
        BorderBrush="#0F323130" 
        BorderThickness="1">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*"/>
            <ColumnDefinition Width="Auto"/>
        </Grid.ColumnDefinitions>
        <StackPanel Grid.Column="0" Spacing="2">
            <TextBlock Text="主标题" 
                      FontSize="14" 
                      FontWeight="SemiBold"
                      Foreground="#FF323130"/>
            <TextBlock Text="副标题或描述" 
                      FontSize="10" 
                      Foreground="#FF605E5C"/>
        </StackPanel>
        <ToggleSwitch Grid.Column="1"
                     Name="EnableToggle"
                     OnContent="启用"
                     OffContent="禁用"
                     IsChecked="True"
                     VerticalAlignment="Center"/>
    </Grid>
</Border>
```

### 创建提示卡片
```xml
<!-- 信息提示 -->
<Border Background="#0A0078D4" 
       BorderBrush="#200078D4" 
       BorderThickness="1"
       CornerRadius="6" 
       Padding="14,10">
    <StackPanel Spacing="6">
        <TextBlock Text="💡 提示" 
                  FontSize="11" 
                  FontWeight="SemiBold"
                  Foreground="#FF0078D4"/>
        <TextBlock Text="这是一条提示信息"
                  FontSize="10"
                  Foreground="#FF323130"
                  TextWrapping="Wrap"/>
    </StackPanel>
</Border>

<!-- 警告提示 -->
<Border Background="#0AE81123" 
       BorderBrush="#20E81123" 
       BorderThickness="1" 
       CornerRadius="6" 
       Padding="14,10">
    <StackPanel Spacing="6">
        <TextBlock Text="⚠️ 警告" 
                  FontSize="11" 
                  FontWeight="SemiBold"
                  Foreground="#FFE81123"/>
        <TextBlock Text="这是一条警告信息"
                  FontSize="10"
                  Foreground="#FF323130"
                  TextWrapping="Wrap"/>
    </StackPanel>
</Border>

<!-- 成功提示 -->
<Border Background="#0A107C10" 
       BorderBrush="#20107C10" 
       BorderThickness="1" 
       CornerRadius="6" 
       Padding="14,10">
    <StackPanel Spacing="6">
        <TextBlock Text="✓ 成功" 
                  FontSize="11" 
                  FontWeight="SemiBold"
                  Foreground="#FF107C10"/>
        <TextBlock Text="这是一条成功信息"
                  FontSize="10"
                  Foreground="#FF323130"
                  TextWrapping="Wrap"/>
    </StackPanel>
</Border>
```

### 创建状态徽章
```xml
<!-- 蓝色徽章 -->
<Border Background="#FF0078D4"
        CornerRadius="8"
        Padding="8,4">
    <TextBlock Text="运行中" 
               FontSize="10" 
               FontWeight="Medium"
               Foreground="White"/>
</Border>

<!-- 绿色徽章 -->
<Border Background="#FF107C10"
        CornerRadius="8"
        Padding="8,4">
    <TextBlock Text="已启用" 
               FontSize="10" 
               FontWeight="Medium"
               Foreground="White"/>
</Border>

<!-- 灰色徽章 -->
<Border Background="#0A000000"
        CornerRadius="8"
        Padding="6,3">
    <TextBlock Text="3 项" 
               FontSize="9" 
               Foreground="#FF605E5C"/>
</Border>
```

### 创建状态指示器
```xml
<!-- 绿色（成功/运行中） -->
<Border Background="#FF107C10"
        Width="6" Height="6"
        CornerRadius="3"
        VerticalAlignment="Center"/>

<!-- 蓝色（信息/就绪） -->
<Border Background="#FF0078D4"
        Width="6" Height="6"
        CornerRadius="3"
        VerticalAlignment="Center"/>

<!-- 红色（错误/停止） -->
<Border Background="#FFE81123"
        Width="6" Height="6"
        CornerRadius="3"
        VerticalAlignment="Center"/>
```

## 颜色速查表

### 主题色
```
蓝色: #FF0078D4
绿色: #FF107C10
红色: #FFE81123
橙色: #FFFF8C00
紫色: #FF8764B8
```

### 中性色
```
文本主色: #FF323130
文本次色: #FF605E5C
文本禁用: #FFA19F9D
边框色: #FF8A8886
```

### 透明度
```
5%:  #0D (13)
10%: #1A (26)
15%: #26 (38)
20%: #33 (51)
30%: #4D (77)
```

## 字体大小速查表

```
9px:  标签、徽章
10px: 正文、描述
11px: 按钮、输入框
12px: 小标题
13px: 中标题
14px: 大标题
```

## 间距速查表

```
6px:  CheckBox间距
8px:  设置项间距
10px: 卡片内部间距
12px: 卡片标题间距
16px: 卡片外部间距
```

## 圆角速查表

```
4px:  设置项内部
6px:  卡片、按钮
8px:  徽章
12px: 大卡片
```

## 常见问题

### Q: 如何创建不抢焦点的窗口？
```xml
<Window Focusable="False"
        ShowActivated="False"
        IsHitTestVisible="False">
```

### Q: 如何创建Acrylic背景？
```xml
<ExperimentalAcrylicBorder IsHitTestVisible="False">
    <ExperimentalAcrylicBorder.Material>
        <ExperimentalAcrylicMaterial
            BackgroundSource="Digger"
            TintColor="#F9F9F9"
            TintOpacity="0.8"
            MaterialOpacity="0.95" />
    </ExperimentalAcrylicBorder.Material>
</ExperimentalAcrylicBorder>
```

### Q: 如何创建自定义标题栏？
```xml
<Window ExtendClientAreaToDecorationsHint="True"
        ExtendClientAreaChromeHints="NoChrome"
        ExtendClientAreaTitleBarHeightHint="32">
    <Border Height="32" VerticalAlignment="Top">
        <!-- 标题栏内容 -->
    </Border>
</Window>
```

### Q: 如何绑定Slider的值到TextBlock？
```xml
<Slider Name="MySlider" Value="50"/>
<TextBlock Text="{Binding #MySlider.Value, StringFormat='{}{0:F0}'}"/>
```

## 最佳实践

### 1. 保持一致性
- 使用统一的颜色
- 使用统一的字体大小
- 使用统一的间距
- 使用统一的圆角

### 2. 优化性能
- 避免过多嵌套
- 使用简单的Border
- 合理使用IsHitTestVisible
- 避免不必要的动画

### 3. 提升可访问性
- 确保足够的对比度
- 使用合适的字体大小
- 提供清晰的标签
- 支持键盘导航

### 4. 响应式设计
- 使用Grid布局
- 设置MinWidth/MinHeight
- 使用ScrollViewer
- 考虑不同DPI

---

**版本**: 1.0.0  
**最后更新**: 2025年10月7日

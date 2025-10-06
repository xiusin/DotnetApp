# Design Document

## Overview

ConfigButtonDisplay UI 优化重构的技术设计方案。重点关注代码重组、便签贴纸效果、配置界面优化和按键监控可配置化。

## Architecture

### Directory Structure

```
ConfigButtonDisplay/
├── Features/                  # 功能模块（已存在）
│   ├── KeyboardMonitoring/   # 键盘监控
│   ├── NoteTags/             # 便签标签
│   ├── AIChat/               # AI 聊天
│   ├── EdgeComponents/       # 边缘组件
│   ├── TextSelection/        # 文本选择
│   └── Debug/                # 调试
├── Core/                      # 核心服务
│   ├── Services/
│   │   ├── ConfigurationService.cs      # 配置管理
│   │   ├── WindowPositionService.cs     # 窗口定位
│   │   └── ThemeService.cs              # 主题管理
│   ├── Configuration/
│   │   ├── AppSettings.cs               # 应用配置模型
│   │   ├── KeyboardMonitorSettings.cs   # 键盘监控配置
│   │   └── WindowSettings.cs            # 窗口配置
│   └── Interfaces/
│       ├── IConfigurationService.cs
│       └── IWindowPositionService.cs
├── Infrastructure/            # 基础设施
│   ├── Hooks/
│   │   └── KeyboardHook.cs              # 键盘钩子（已存在）
│   └── Helpers/
│       ├── ScreenHelper.cs              # 屏幕辅助
│       └── AnimationHelper.cs           # 动画辅助
├── Views/                     # 视图文件
│   ├── MainWindow.axaml                 # 主窗口（已存在）
│   ├── ConfigWindow.axaml               # 配置窗口（新增）
│   └── KeyDisplayWindow.axaml           # 按键显示（已存在）
└── ViewModels/                # 视图模型（新增）
    ├── MainViewModel.cs
    └── ConfigViewModel.cs
```

## Components and Interfaces

### 1. Configuration Service

#### Interface

```csharp
public interface IConfigurationService
{
    Task<AppSettings> LoadAsync();
    Task SaveAsync(AppSettings settings);
    Task<T> GetModuleAsync<T>(string moduleName) where T : class;
    Task SaveModuleAsync<T>(string moduleName, T settings) where T : class;
}
```

#### Configuration Models

```csharp
public class AppSettings
{
    public int Version { get; set; } = 1;
    public WindowSettings Window { get; set; } = new();
    public KeyboardMonitorSettings KeyboardMonitor { get; set; } = new();
    public Dictionary<string, object> Modules { get; set; } = new();
}

public class WindowSettings
{
    public string Position { get; set; } = "RightEdge";  // RightEdge, LeftEdge, Custom
    public int? CustomX { get; set; }
    public int? CustomY { get; set; }
    public bool RememberPosition { get; set; } = true;
    public double Opacity { get; set; } = 0.95;
}

public class KeyboardMonitorSettings
{
    public bool Enabled { get; set; } = true;
    public string DisplayPosition { get; set; } = "BottomCenter";
    public string BackgroundColor { get; set; } = "#3182CE";
    public double Opacity { get; set; } = 0.9;
    public int FontSize { get; set; } = 28;
    public string FontColor { get; set; } = "#FFFFFF";
    public double DisplayDuration { get; set; } = 2.0;
    public bool ShowModifiers { get; set; } = true;
    public bool ShowFunctionKeys { get; set; } = true;
    public bool ShowAlphaNumeric { get; set; } = true;
}
```

#### Storage Location

- Windows: `%APPDATA%/ConfigButtonDisplay/appsettings.json`
- 使用 `System.Text.Json` 进行序列化

### 2. Window Position Service

#### Interface

```csharp
public interface IWindowPositionService
{
    PixelPoint CalculateRightEdgePosition(int windowWidth, int windowHeight);
    PixelPoint CalculatePosition(string position, int width, int height);
    void SavePosition(Window window);
    PixelPoint? LoadSavedPosition();
}
```

#### Implementation Details

**Right Edge Positioning:**
```csharp
var screen = Screens.Primary;
var x = screen.WorkingArea.Right - windowWidth - 16;  // 16px margin
var y = (screen.WorkingArea.Height - windowHeight) / 2;  // Vertically centered
return new PixelPoint(x, y);
```

**Display Position Mapping:**
- TopLeft: (50, 50)
- TopCenter: (centerX, 50)
- TopRight: (right - width - 50, 50)
- BottomLeft: (50, bottom - height - 50)
- BottomCenter: (centerX, bottom - height - 60)
- BottomRight: (right - width - 50, bottom - height - 50)

### 3. Sticky Note Window Design

#### Visual Specifications

**AXAML Structure:**
```xml
<Window SystemDecorations="None"
        Background="Transparent"
        TransparencyLevelHint="AcrylicBlur">
    <Border Background="#F2FFFFFF"
            CornerRadius="16"
            BoxShadow="0 8 32 0 #40000000"
            BorderThickness="1"
            BorderBrush="#33FFFFFF">
        <!-- Content -->
    </Border>
</Window>
```

**Startup Behavior:**
```csharp
// In MainWindow constructor or Loaded event
protected override void OnOpened(EventArgs e)
{
    base.OnOpened(e);
    
    var positionService = // Get from DI
    var settings = // Load from config
    
    if (settings.Window.Position == "RightEdge")
    {
        var pos = positionService.CalculateRightEdgePosition(
            (int)this.Width, (int)this.Height);
        this.Position = pos;
    }
    
    // Apply sticky note effect
    this.Topmost = true;
    AnimateSlideIn();
}
```

**Slide-in Animation:**
```csharp
private async void AnimateSlideIn()
{
    var startX = this.Position.X + 300;  // Start off-screen
    var endX = this.Position.X;
    
    // Animate from startX to endX over 300ms
    // Use Avalonia Animations or manual timer
}
```

### 4. Configuration Window

#### Tab Structure

```xml
<Window Title="设置" Width="800" Height="600"
        SystemDecorations="None"
        Background="Transparent">
    <Border Background="#F0FFFFFF" CornerRadius="16">
        <Grid RowDefinitions="Auto,*,Auto">
            <!-- Header -->
            <Border Grid.Row="0" Padding="24,16">
                <TextBlock Text="应用设置" FontSize="24"/>
            </Border>
            
            <!-- Tab Content -->
            <TabControl Grid.Row="1" Margin="16">
                <TabItem>
                    <TabItem.Header>
                        <StackPanel Orientation="Horizontal" Spacing="8">
                            <fluent:SymbolIcon Symbol="Settings"/>
                            <TextBlock Text="通用"/>
                        </StackPanel>
                    </TabItem.Header>
                    <GeneralSettingsPanel/>
                </TabItem>
                
                <TabItem>
                    <TabItem.Header>
                        <StackPanel Orientation="Horizontal" Spacing="8">
                            <fluent:SymbolIcon Symbol="Keyboard"/>
                            <TextBlock Text="键盘监控"/>
                        </StackPanel>
                    </TabItem.Header>
                    <KeyboardMonitorPanel/>
                </TabItem>
                
                <!-- More tabs... -->
            </TabControl>
            
            <!-- Footer Buttons -->
            <StackPanel Grid.Row="2" Orientation="Horizontal" 
                       HorizontalAlignment="Right" Spacing="12" Margin="16">
                <Button Content="保存" Click="SaveButton_Click"/>
                <Button Content="取消" Click="CancelButton_Click"/>
            </StackPanel>
        </Grid>
    </Border>
</Window>
```

#### Settings Panels

每个设置面板的结构：

```xml
<UserControl>
    <ScrollViewer>
        <StackPanel Spacing="24" Margin="24">
            <!-- Section Header -->
            <StackPanel Spacing="8">
                <TextBlock Text="显示位置" FontSize="18" FontWeight="SemiBold"/>
                <TextBlock Text="选择按键显示的屏幕位置" Foreground="#FF718096"/>
            </StackPanel>
            
            <!-- Settings Grid -->
            <Grid ColumnDefinitions="200,*" RowDefinitions="Auto,Auto,Auto">
                <TextBlock Grid.Row="0" Grid.Column="0" Text="位置" VerticalAlignment="Center"/>
                <ComboBox Grid.Row="0" Grid.Column="1" SelectedItem="{Binding Position}">
                    <ComboBoxItem Content="左上"/>
                    <ComboBoxItem Content="顶部居中"/>
                    <ComboBoxItem Content="右上"/>
                    <ComboBoxItem Content="左下"/>
                    <ComboBoxItem Content="底部居中"/>
                    <ComboBoxItem Content="右下"/>
                </ComboBox>
                
                <!-- More settings... -->
            </Grid>
            
            <!-- Preview Section -->
            <Border Background="#FAFBFC" CornerRadius="12" Padding="24">
                <StackPanel Spacing="12">
                    <TextBlock Text="预览" FontSize="16" FontWeight="SemiBold"/>
                    <Border Background="{Binding PreviewBackground}"
                            CornerRadius="8" Padding="16">
                        <TextBlock Text="Ctrl + Shift + A"
                                  FontSize="{Binding FontSize}"
                                  Foreground="{Binding FontColor}"/>
                    </Border>
                </StackPanel>
            </Border>
        </StackPanel>
    </ScrollViewer>
</UserControl>
```

### 5. Keyboard Monitor Configuration

#### KeyboardMonitorPanel Implementation

**Configurable Properties:**

1. **Display Position** - ComboBox with 6 options
2. **Background Color** - ColorPicker control
3. **Opacity** - Slider (0.1 - 1.0)
4. **Font Size** - Slider (12 - 48)
5. **Font Color** - ColorPicker control
6. **Display Duration** - Slider (1 - 10 seconds)
7. **Key Filters** - CheckBoxes for each key type

**ViewModel:**

```csharp
public class KeyboardMonitorViewModel : ViewModelBase
{
    private KeyboardMonitorSettings _settings;
    
    public string DisplayPosition
    {
        get => _settings.DisplayPosition;
        set { _settings.DisplayPosition = value; OnPropertyChanged(); UpdatePreview(); }
    }
    
    public string BackgroundColor
    {
        get => _settings.BackgroundColor;
        set { _settings.BackgroundColor = value; OnPropertyChanged(); UpdatePreview(); }
    }
    
    // More properties...
    
    private void UpdatePreview()
    {
        // Update preview display in real-time
    }
    
    public async Task SaveAsync()
    {
        await _configService.SaveModuleAsync("KeyboardMonitor", _settings);
        ApplyToKeyDisplayWindow();
    }
}
```

**Apply Configuration:**

```csharp
private void ApplyToKeyDisplayWindow()
{
    if (_keyDisplayWindow != null)
    {
        _keyDisplayWindow.UpdateSettings(_settings);
    }
}
```

## Data Models

### Configuration File Format

```json
{
  "version": 1,
  "window": {
    "position": "RightEdge",
    "customX": null,
    "customY": null,
    "rememberPosition": true,
    "opacity": 0.95
  },
  "keyboardMonitor": {
    "enabled": true,
    "displayPosition": "BottomCenter",
    "backgroundColor": "#3182CE",
    "opacity": 0.9,
    "fontSize": 28,
    "fontColor": "#FFFFFF",
    "displayDuration": 2.0,
    "showModifiers": true,
    "showFunctionKeys": true,
    "showAlphaNumeric": true
  },
  "modules": {
    "noteTags": {
      "enabled": true,
      "tags": []
    },
    "aiChat": {
      "enabled": true
    }
  }
}
```

## Error Handling

### Configuration Loading

```csharp
public async Task<AppSettings> LoadAsync()
{
    try
    {
        var json = await File.ReadAllTextAsync(ConfigPath);
        return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
    }
    catch (FileNotFoundException)
    {
        // Create default config
        var defaults = new AppSettings();
        await SaveAsync(defaults);
        return defaults;
    }
    catch (JsonException ex)
    {
        // Backup corrupted file
        File.Copy(ConfigPath, $"{ConfigPath}.bak", true);
        return new AppSettings();
    }
}
```

### Window Positioning

```csharp
public PixelPoint CalculateRightEdgePosition(int width, int height)
{
    var screen = Screens.Primary;
    if (screen == null)
        return new PixelPoint(100, 100);  // Fallback
    
    var x = Math.Max(0, screen.WorkingArea.Right - width - 16);
    var y = Math.Max(0, (screen.WorkingArea.Height - height) / 2);
    
    return new PixelPoint(x, y);
}
```

## Performance Considerations

### Configuration Caching

- 配置加载后缓存在内存中
- 配置更改时更新缓存
- 使用 `FileSystemWatcher` 监听外部更改

### Animation Performance

- 使用 Avalonia 内置动画系统
- 启用 GPU 加速
- 避免在动画中进行复杂计算

### Memory Management

- 及时释放事件订阅
- 使用 `IDisposable` 模式
- 避免循环引用

## Migration Strategy

### Phase 1: Core Services (不影响现有功能)

1. 创建 `Core/Services/ConfigurationService.cs`
2. 创建 `Core/Services/WindowPositionService.cs`
3. 创建配置模型类
4. 实现配置加载和保存

### Phase 2: Sticky Note Effect

1. 修改 `MainWindow.axaml` 添加便签样式
2. 在 `MainWindow.axaml.cs` 中集成 `WindowPositionService`
3. 实现右侧边缘定位逻辑
4. 添加滑入动画

### Phase 3: Configuration Window

1. 创建 `Views/ConfigWindow.axaml`
2. 创建 `ViewModels/ConfigViewModel.cs`
3. 实现 TabControl 结构
4. 创建通用设置面板

### Phase 4: Keyboard Monitor Panel

1. 创建 `KeyboardMonitorPanel.axaml`
2. 创建 `KeyboardMonitorViewModel.cs`
3. 实现配置项绑定
4. 实现实时预览
5. 集成到 `KeyDisplayWindow`

### Phase 5: Other Module Panels

1. 为每个功能模块创建设置面板
2. 实现模块特定配置
3. 集成到配置窗口

### Phase 6: Code Reorganization

1. 移动现有组件到 Features 目录
2. 更新命名空间
3. 更新引用
4. 验证编译

## Design Decisions

### Why JSON Configuration?

- 人类可读，便于手动编辑
- .NET 原生支持，无需额外依赖
- 跨平台兼容

### Why Service-based Architecture?

- 松耦合，易于维护
- 便于依赖注入
- 支持单元测试

### Why MVVM for Config Window?

- Avalonia 原生支持数据绑定
- 分离 UI 和业务逻辑
- 便于实现实时预览

### Why Gradual Migration?

- 降低风险，每个阶段可独立验证
- 不影响现有功能
- 便于回滚

---

*设计文档 v1.0 - 聚焦实现，无测试内容*

# ConfigButtonDisplay - 键盘按键显示器

## 项目概述

ConfigButtonDisplay 是一个基于 Avalonia UI 框架开发的跨平台桌面应用程序，用于实时显示用户按下的键盘按键组合。该应用程序采用 C# 和 .NET 6.0 开发，支持 Windows、macOS 和 Linux 系统。

### 主要功能
- **实时按键显示**：在屏幕左下角显示当前按下的按键组合（如 Ctrl + Shift + A）
- **全局键盘监听**：支持系统级键盘事件监听，不依赖应用窗口焦点
- **可定制显示**：支持自定义背景颜色、字体大小和显示时长
- **跨平台支持**：使用 Avalonia UI 实现 Windows、macOS、Linux 兼容性
- **后台运行**：支持最小化到后台，键盘监听持续运行

### 技术架构
- **UI框架**：Avalonia UI 11.0.0
- **目标框架**：.NET 6.0
- **编程语言**：C# 
- **平台支持**：Windows、macOS、Linux
- **主要依赖**：
  - Avalonia.Desktop
  - Avalonia.Themes.Fluent
  - Avalonia.Fonts.Inter

## 项目结构

```
/Users/tuoke/Desktop/DotnetApp/
├── App.axaml                    # 应用程序主样式文件
├── App.axaml.cs                 # 应用程序入口逻辑
├── MainWindow.axaml              # 主窗口UI定义
├── MainWindow.axaml.cs           # 主窗口业务逻辑
├── KeyDisplayWindow.axaml        # 按键显示窗口UI
├── KeyDisplayWindow.axaml.cs     # 按键显示窗口逻辑
├── KeyboardHook.cs               # 键盘钩子实现（跨平台）
├── PopupWindow.axaml             # 弹出窗口UI（备用）
├── PopupWindow.axaml.cs          # 弹出窗口逻辑
├── Program.cs                    # 程序入口点
├── ConfigButtonDisplay.csproj    # 项目文件
├── app.manifest                  # Windows应用程序清单
└── IFLOW.md                      # 项目文档
```

## 核心组件

### 1. KeyboardHook.cs - 键盘监听核心
- **功能**：实现跨平台键盘事件监听
- **技术特点**：
  - Windows：使用 `GetAsyncKeyState` API
  - macOS：使用 `CGEventSourceKeyState` API
  - Linux：预留 X11 接口（当前为简化实现）
  - 轮询机制：50ms间隔检测按键状态变化
  - 事件驱动：提供 `KeyDown` 和 `KeyUp` 事件

### 2. MainWindow - 主控制界面
- **功能**：提供用户界面和配置管理
- **主要特性**：
  - 监听状态开关控制
  - 显示设置（颜色、字体、时长）
  - 实时预览功能
  - 系统托盘集成
  - 后台运行支持

### 3. KeyDisplayWindow - 按键显示窗口
- **功能**：在屏幕左下角显示按键组合
- **设计特点**：
  - 置顶窗口（Topmost）
  - 无边框设计（SystemDecorations="None"）
  - 半透明黑色背景
  - 固定尺寸（280x60像素）
  - 自动位置调整（屏幕左下角）

## 构建和运行

### 环境要求
- .NET 6.0 SDK 或更高版本
- Visual Studio 2022 / VS Code / Rider
- 支持的平台：Windows 10+、macOS 10.15+、Linux (Ubuntu 18.04+)

### 构建命令
```bash
# 还原依赖
dotnet restore

# 构建项目
dotnet build

# 运行应用
dotnet run

# 发布应用（Windows）
dotnet publish -c Release -r win-x64 --self-contained

# 发布应用（macOS）
dotnet publish -c Release -r osx-x64 --self-contained

# 发布应用（Linux）
dotnet publish -c Release -r linux-x64 --self-contained
```

### 调试模式
```bash
# 以调试模式运行
dotnet run --configuration Debug
```

## 开发约定

### 代码风格
- **命名规范**：
  - 类名、方法名：PascalCase
  - 私有字段：`_camelCase`（带下划线前缀）
  - 局部变量：camelCase
- **代码组织**：
  - UI逻辑与业务逻辑分离（.axaml 和 .axaml.cs）
  - 使用部分类（partial）组织大型组件
  - 事件处理采用明确的方法命名（如 `Button_Click`）

### 错误处理
- 键盘钩子初始化失败时显示错误状态
- 跨平台API调用使用try-catch保护
- 轮询机制忽略异常，确保稳定性

### 性能考虑
- 50ms轮询间隔平衡响应性和CPU占用
- UI更新使用 `Dispatcher.UIThread.Post` 确保线程安全
- 按键状态使用哈希集合高效管理

## 功能详细说明

### 按键格式化
应用程序智能过滤和格式化按键组合：
- **修饰键检测**：Ctrl、Alt、Shift、Win
- **特殊按键映射**：
  - 方向键：↑↓←→
  - 功能键：F1-F12
  - 编辑键：⌫（退格）、Del、Ins 等
  - 符号键：正确处理 OEM 键
- **过滤机制**：仅显示包含普通按键的组合，避免空修饰键显示

### 显示控制
- **位置策略**：固定屏幕左下角（50px, 底部120px）
- **显示逻辑**：
  - 按键按下时立即显示
  - 按键释放时更新或隐藏
  - 空内容时窗口透明化
- **样式定制**：
  - 5种预设背景颜色（蓝、绿、红、紫、橙）
  - 字体大小范围：12-48px
  - 显示时长：1-5秒

### 跨平台实现
- **Windows**：使用 Win32 API `GetAsyncKeyState`
- **macOS**：使用 Core Graphics `CGEventSourceKeyState`
- **Linux**：预留 X11 接口（当前为占位实现）

## 扩展建议

### 短期改进
1. **Linux支持**：实现 X11 键盘状态检测
2. **多显示器支持**：改进窗口位置计算逻辑
3. **配置文件**：添加用户设置持久化
4. **热键支持**：添加全局热键控制监听状态

### 长期规划
1. **插件系统**：支持自定义按键格式化规则
2. **主题系统**：支持更丰富的视觉定制
3. **统计功能**：记录按键使用频率
4. **网络同步**：多设备间配置同步

## 注意事项

### 安全考虑
- 应用程序需要系统级键盘访问权限
- 在macOS上可能需要辅助功能权限
- Windows Defender可能标记为潜在风险（键盘监听特性）

### 性能优化
- 轮询机制在高频按键场景下的性能表现
- 长时间运行时的内存占用监控
- 多平台兼容性测试建议

### 用户指导
- 首次运行可能需要管理员权限
- macOS用户需要在系统偏好设置中授予辅助功能权限
- 建议添加到开机启动项以获得持续体验

---

*本文档基于项目代码分析生成，旨在为开发者提供全面的项目理解和技术指导。*




# 角色定位
You are a C#/dotnet/avalonia expert specializing in modern .NET development and enterprise-grade applications.
桌面软件开发专家，且擅长UI/UX设计，符合现代人的美感。

## Focus Areas

- Modern C# features (records, pattern matching, nullable reference types)
- .NET ecosystem and frameworks (ASP.NET Core, Entity Framework, Blazor)
- SOLID principles and design patterns in C#
- Performance optimization and memory management
- Async/await and concurrent programming with TPL
- Comprehensive testing (xUnit, NUnit, Moq, FluentAssertions)
- Enterprise patterns and microservices architecture

## Approach

1. Leverage modern C# features for clean, expressive code
2. Follow SOLID principles and favor composition over inheritance
3. Use nullable reference types and comprehensive error handling
4. Optimize for performance with span, memory, and value types
5. Implement proper async patterns without blocking
6. Maintain high test coverage with meaningful unit tests

## Output

- Clean C# code with modern language features
- Comprehensive unit tests with proper mocking
- Performance benchmarks using BenchmarkDotNet
- Async/await implementations with proper exception handling
- NuGet package configuration and dependency management
- Code analysis and style configuration (EditorConfig, analyzers)
- Enterprise architecture patterns when applicable

Follow .NET coding standards and include comprehensive XML documentation.

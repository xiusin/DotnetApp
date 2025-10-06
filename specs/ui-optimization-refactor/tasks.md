# Implementation Plan

本实施计划将 UI 优化重构分解为一系列可执行的编码任务。每个任务完成后需要编译验证并提交代码。

## Task Execution Rules

- ✅ 每个任务完成后必须运行 `dotnet build` 确保编译通过
- ✅ 编译通过后使用任务名称进行 git commit
- ✅ 按顺序执行任务，确保每一步都稳定后再进行下一步

---

## Phase 1: Core Services Foundation

- [x] 1. 创建配置服务基础架构



  - 创建 `Core/Interfaces/IConfigurationService.cs` 接口
  - 创建 `Core/Configuration/AppSettings.cs` 配置模型
  - 创建 `Core/Configuration/WindowSettings.cs` 窗口配置模型
  - 创建 `Core/Configuration/KeyboardMonitorSettings.cs` 键盘监控配置模型
  - _Requirements: 1.4, 1.5, 5.1_

  - _Commit: "创建配置服务基础架构"_

- [ ] 2. 实现配置服务
  - 创建 `Core/Services/ConfigurationService.cs` 实现 `IConfigurationService`
  - 实现 `LoadAsync()` 方法，支持从 JSON 文件加载配置
  - 实现 `SaveAsync()` 方法，支持保存配置到 JSON 文件
  - 实现 `GetModuleAsync<T>()` 和 `SaveModuleAsync<T>()` 方法
  - 添加错误处理：文件不存在时创建默认配置，JSON 格式错误时备份并使用默认值
  - _Requirements: 5.1, 5.2, 5.3, 5.4_
  - _Commit: "实现配置服务"_

- [ ] 3. 创建窗口定位服务
  - 创建 `Core/Interfaces/IWindowPositionService.cs` 接口
  - 创建 `Infrastructure/Helpers/ScreenHelper.cs` 屏幕辅助类
  - 创建 `Core/Services/WindowPositionService.cs` 实现窗口定位逻辑
  - 实现 `CalculateRightEdgePosition()` 方法计算右侧边缘位置
  - 实现 `CalculatePosition()` 方法支持多种位置（TopLeft, TopCenter, BottomCenter 等）
  - 实现 `SavePosition()` 和 `LoadSavedPosition()` 方法
  - _Requirements: 2.1, 2.3, 2.7_
  - _Commit: "创建窗口定位服务"_

## Phase 2: Sticky Note Window Effect

- [ ] 4. 修改主窗口样式为便签贴纸效果
  - 修改 `MainWindow.axaml` 的 Border 样式
  - 设置 `Background="#F2FFFFFF"` 实现半透明效果
  - 设置 `CornerRadius="16"` 实现圆角
  - 设置 `BoxShadow="0 8 32 0 #40000000"` 实现阴影效果
  - 设置 `BorderThickness="1"` 和 `BorderBrush="#33FFFFFF"` 实现边框
  - _Requirements: 2.2_
  - _Commit: "修改主窗口样式为便签贴纸效果"_

- [ ] 5. 集成窗口定位服务到主窗口
  - 在 `MainWindow.axaml.cs` 中注入 `IWindowPositionService` 和 `IConfigurationService`
  - 在 `OnOpened()` 或 `Loaded` 事件中加载窗口配置
  - 根据配置的 `Position` 属性计算窗口位置
  - 如果 `Position` 为 "RightEdge"，调用 `CalculateRightEdgePosition()` 定位到右侧边缘
  - 设置 `Topmost = true` 保持置顶
  - _Requirements: 2.1, 2.4_
  - _Commit: "集成窗口定位服务到主窗口"_

- [ ] 6. 实现主窗口滑入动画
  - 创建 `Infrastructure/Helpers/AnimationHelper.cs` 动画辅助类
  - 实现 `SlideInFromRight()` 方法，从右侧滑入动画（300ms ease-out）
  - 在主窗口启动时调用滑入动画
  - 确保动画流畅，不阻塞 UI 线程
  - _Requirements: 2.8_
  - _Commit: "实现主窗口滑入动画"_

- [ ] 7. 实现窗口位置保存和恢复
  - 在主窗口的 `PositionChanged` 事件中调用 `SavePosition()`
  - 使用防抖动机制避免频繁保存（500ms 延迟）
  - 在窗口启动时，如果 `RememberPosition` 为 true，加载保存的位置
  - 验证保存的位置在屏幕范围内，超出则使用默认位置
  - _Requirements: 2.6, 2.7_
  - _Commit: "实现窗口位置保存和恢复"_

## Phase 3: Configuration Window Foundation

- [ ] 8. 创建配置窗口基础结构
  - 创建 `Views/ConfigWindow.axaml` 和 `Views/ConfigWindow.axaml.cs`
  - 设置窗口属性：`SystemDecorations="None"`, `Background="Transparent"`
  - 创建 Fluent Design 风格的 Border 容器（圆角、阴影）
  - 创建 Grid 布局：Header (Auto), Content (*), Footer (Auto)
  - 添加标题栏（包含标题文本和关闭按钮）
  - 添加底部按钮栏（保存、取消按钮）
  - _Requirements: 3.1, 3.8_
  - _Commit: "创建配置窗口基础结构"_

- [ ] 9. 创建 TabControl 结构
  - 在配置窗口的 Content 区域添加 `TabControl`
  - 创建 6 个 TabItem：通用设置、键盘监控、AI 聊天、标签管理、边缘组件、调试选项
  - 为每个 TabItem 添加 Header（使用 FluentIcon + 文本）
  - 设置 TabControl 样式符合 Fluent Design（圆角、过渡动画）
  - _Requirements: 3.2, 3.3, 3.4, 3.8_
  - _Commit: "创建 TabControl 结构"_

- [ ] 10. 创建配置窗口 ViewModel
  - 创建 `ViewModels/ConfigViewModel.cs` 继承 `ViewModelBase`
  - 添加 `AppSettings` 属性绑定配置数据
  - 实现 `LoadAsync()` 方法从 `IConfigurationService` 加载配置
  - 实现 `SaveAsync()` 方法保存配置
  - 实现 `ResetCommand` 重置到默认值
  - 实现 `INotifyPropertyChanged` 支持数据绑定
  - _Requirements: 3.5, 3.6_
  - _Commit: "创建配置窗口 ViewModel"_

- [ ] 11. 实现配置窗口打开逻辑
  - 在 `MainWindow.axaml.cs` 的 `ConfigButton_Click` 事件中打开配置窗口
  - 创建 `ConfigWindow` 实例并设置 `DataContext` 为 `ConfigViewModel`
  - 调用 `ShowDialog()` 以模态方式显示配置窗口
  - 配置窗口关闭后，如果保存了配置，重新加载主窗口设置
  - _Requirements: 3.6_
  - _Commit: "实现配置窗口打开逻辑"_

## Phase 4: General Settings Panel

- [ ] 12. 创建通用设置面板
  - 创建 `Views/Panels/GeneralSettingsPanel.axaml` UserControl
  - 添加窗口位置设置：ComboBox 选择 RightEdge/LeftEdge/Custom
  - 添加窗口透明度设置：Slider (0.5 - 1.0)
  - 添加"记住窗口位置"复选框
  - 添加"始终置顶"复选框
  - 使用 Grid 布局：Label 列 (200px) + Control 列 (*)
  - _Requirements: 2.1, 2.4, 2.7_
  - _Commit: "创建通用设置面板"_

- [ ] 13. 绑定通用设置到 ViewModel
  - 在 `ConfigViewModel` 中添加 `WindowSettings` 属性
  - 在 `GeneralSettingsPanel.axaml` 中绑定控件到 `WindowSettings` 属性
  - 实现属性更改通知，确保 UI 实时更新
  - 添加输入验证（透明度范围、位置有效性）
  - _Requirements: 3.5, 3.7_
  - _Commit: "绑定通用设置到 ViewModel"_

## Phase 5: Keyboard Monitor Settings Panel

- [ ] 14. 创建键盘监控设置面板基础
  - 创建 `Views/Panels/KeyboardMonitorPanel.axaml` UserControl
  - 创建 `ViewModels/KeyboardMonitorViewModel.cs`
  - 添加面板标题和描述
  - 创建 ScrollViewer 容器支持滚动
  - 创建 StackPanel 布局，Spacing="24"
  - _Requirements: 4.1_
  - _Commit: "创建键盘监控设置面板基础"_

- [ ] 15. 添加显示位置配置
  - 添加"显示位置"ComboBox，包含 6 个选项：左上、顶部居中、右上、左下、底部居中、右下
  - 绑定到 `KeyboardMonitorViewModel.DisplayPosition` 属性
  - 添加自定义位置输入框（X, Y 坐标），当选择"自定义"时启用
  - _Requirements: 4.1_
  - _Commit: "添加显示位置配置"_

- [ ] 16. 添加显示样式配置
  - 添加背景颜色选择器（使用 ColorPicker 或预设颜色 ComboBox）
  - 添加透明度 Slider (0.1 - 1.0)，显示当前值
  - 添加字体大小 Slider (12 - 48)，显示当前值
  - 添加字体颜色选择器
  - 使用 Grid 布局组织控件
  - _Requirements: 4.2_
  - _Commit: "添加显示样式配置"_

- [ ] 17. 添加显示行为配置
  - 添加显示时长 Slider (1 - 10 秒)，显示当前值
  - 添加淡入动画时长 Slider (0.1 - 1.0 秒)
  - 添加淡出动画时长 Slider (0.1 - 1.0 秒)
  - 添加"自动隐藏"复选框
  - _Requirements: 4.3_
  - _Commit: "添加显示行为配置"_

- [ ] 18. 添加按键过滤配置
  - 添加"显示修饰键"复选框（Ctrl, Alt, Shift, Win）
  - 添加"显示功能键"复选框（F1-F12）
  - 添加"显示字母数字键"复选框（A-Z, 0-9）
  - 添加"显示导航键"复选框（方向键、Home、End 等）
  - 使用 StackPanel 垂直排列复选框
  - _Requirements: 4.4_
  - _Commit: "添加按键过滤配置"_

- [ ] 19. 实现实时预览功能
  - 在键盘监控面板底部添加预览区域
  - 创建 Border 容器模拟 KeyDisplayWindow 的样式
  - 绑定预览区域的背景色、透明度、字体大小、字体颜色到配置属性
  - 当配置更改时，实时更新预览显示
  - 添加"测试显示"按钮，触发实际的 KeyDisplayWindow 显示
  - _Requirements: 4.6_
  - _Commit: "实现实时预览功能"_

- [ ] 20. 实现配置应用到 KeyDisplayWindow
  - 在 `KeyDisplayWindow.axaml.cs` 中添加 `UpdateSettings()` 方法
  - 接受 `KeyboardMonitorSettings` 参数并应用到窗口
  - 更新窗口位置（调用 `IWindowPositionService.CalculatePosition()`）
  - 更新背景色、透明度、字体大小、字体颜色
  - 更新显示时长和动画时长
  - 在 `ConfigViewModel.SaveAsync()` 中调用 `UpdateSettings()`
  - _Requirements: 4.7_
  - _Commit: "实现配置应用到 KeyDisplayWindow"_

- [ ] 21. 实现按键过滤逻辑
  - 在 `MainWindow.axaml.cs` 的 `FormatKeyCombination()` 方法中添加过滤逻辑
  - 根据 `KeyboardMonitorSettings.ShowModifiers` 决定是否显示修饰键
  - 根据 `ShowFunctionKeys` 过滤功能键
  - 根据 `ShowAlphaNumeric` 过滤字母数字键
  - 根据 `ShowNavigation` 过滤导航键
  - 确保过滤后仍有按键显示时才显示窗口
  - _Requirements: 4.4_
  - _Commit: "实现按键过滤逻辑"_

## Phase 6: Other Module Settings Panels

- [ ] 22. 创建 AI 聊天设置面板
  - 创建 `Views/Panels/AIChatPanel.axaml` UserControl
  - 添加"启用 AI 聊天"复选框
  - 添加热键设置（默认 Shift+Shift）
  - 添加窗口位置和大小设置
  - 绑定到 `ConfigViewModel` 的 AI 聊天配置
  - _Requirements: 3.3_
  - _Commit: "创建 AI 聊天设置面板"_

- [ ] 23. 创建标签管理设置面板
  - 创建 `Views/Panels/NoteTagPanel.axaml` UserControl
  - 添加"启用便签标签"复选框
  - 添加标签列表（可添加、编辑、删除标签）
  - 为每个标签添加文本和颜色设置
  - 添加标签位置设置
  - 绑定到 `ConfigViewModel` 的标签配置
  - _Requirements: 3.3_
  - _Commit: "创建标签管理设置面板"_

- [ ] 24. 创建边缘组件设置面板
  - 创建 `Views/Panels/EdgeComponentPanel.axaml` UserControl
  - 添加"启用边缘组件"复选框
  - 添加"自动显示"复选框和间隔时间设置
  - 添加边缘触发区域大小设置
  - 添加显示动画设置
  - 绑定到 `ConfigViewModel` 的边缘组件配置
  - _Requirements: 3.3_
  - _Commit: "创建边缘组件设置面板"_

- [ ] 25. 创建调试选项设置面板
  - 创建 `Views/Panels/DebugPanel.axaml` UserControl
  - 添加"启用调试模式"复选框
  - 添加"显示调试覆盖层"复选框
  - 添加日志级别选择
  - 添加"清除日志"按钮
  - 绑定到 `ConfigViewModel` 的调试配置
  - _Requirements: 3.3_
  - _Commit: "创建调试选项设置面板"_

## Phase 7: Configuration Persistence

- [ ] 26. 实现配置保存逻辑
  - 在 `ConfigWindow.axaml.cs` 的保存按钮点击事件中调用 `ConfigViewModel.SaveAsync()`
  - 显示保存进度指示器
  - 保存成功后显示提示消息
  - 保存失败时显示错误消息并保留配置窗口打开
  - 关闭配置窗口并通知主窗口重新加载配置
  - _Requirements: 3.6, 5.1_
  - _Commit: "实现配置保存逻辑"_

- [ ] 27. 实现配置重置功能
  - 在 `ConfigViewModel` 中实现 `ResetCommand`
  - 显示确认对话框："确定要重置所有设置到默认值吗？"
  - 用户确认后，创建新的 `AppSettings` 实例（默认值）
  - 更新 ViewModel 的所有属性
  - 触发 UI 更新
  - _Requirements: 4.9_
  - _Commit: "实现配置重置功能"_

- [ ] 28. 实现配置验证
  - 在 `ConfigViewModel.SaveAsync()` 中添加验证逻辑
  - 验证颜色值格式（Hex 格式 #RRGGBB）
  - 验证数值范围（透明度 0-1，字体大小 12-48，显示时长 1-10）
  - 验证位置坐标（确保在屏幕范围内）
  - 验证失败时显示错误提示，阻止保存
  - _Requirements: 3.7, 4.8_
  - _Commit: "实现配置验证"_

- [ ] 29. 实现配置文件监听
  - 在 `ConfigurationService` 中添加 `FileSystemWatcher`
  - 监听配置文件的外部更改
  - 文件更改时触发 `ConfigurationChanged` 事件
  - 在主窗口中订阅事件，重新加载配置
  - 添加防抖动机制避免频繁重新加载
  - _Requirements: 5.7_
  - _Commit: "实现配置文件监听"_

## Phase 8: Code Reorganization

- [ ] 30. 移动键盘监控组件到 Features 目录
  - 将 `KeyboardHook.cs` 移动到 `Infrastructure/Hooks/`
  - 将 `KeyDisplayWindow.axaml` 和 `.axaml.cs` 移动到 `Features/KeyboardMonitoring/Controls/`
  - 更新命名空间为 `ConfigButtonDisplay.Features.KeyboardMonitoring.Controls`
  - 更新所有引用 `KeyDisplayWindow` 的代码
  - 验证编译通过
  - _Requirements: 1.1, 1.2, 1.3, 1.6_
  - _Commit: "移动键盘监控组件到 Features 目录"_

- [ ] 31. 移动便签标签组件到 Features 目录
  - 将 `NoteTagComponent` 相关文件移动到 `Features/NoteTags/Controls/`
  - 更新命名空间为 `ConfigButtonDisplay.Features.NoteTags.Controls`
  - 更新 `MainWindow.axaml.cs` 中的引用
  - 验证编译通过
  - _Requirements: 1.1, 1.2, 1.3, 1.6_
  - _Commit: "移动便签标签组件到 Features 目录"_

- [ ] 32. 移动 AI 聊天组件到 Features 目录
  - 将 `AIChatWindow` 相关文件移动到 `Features/AIChat/Controls/`
  - 更新命名空间为 `ConfigButtonDisplay.Features.AIChat.Controls`
  - 更新 `MainWindow.axaml.cs` 中的引用
  - 验证编译通过
  - _Requirements: 1.1, 1.2, 1.3, 1.6_
  - _Commit: "移动 AI 聊天组件到 Features 目录"_

- [ ] 33. 移动边缘组件到 Features 目录
  - 将 `SimpleEdgeComponent` 相关文件移动到 `Features/EdgeComponents/Controls/`
  - 更新命名空间为 `ConfigButtonDisplay.Features.EdgeComponents.Controls`
  - 更新 `MainWindow.axaml.cs` 中的引用
  - 验证编译通过
  - _Requirements: 1.1, 1.2, 1.3, 1.6_
  - _Commit: "移动边缘组件到 Features 目录"_

- [ ] 34. 移动文本选择组件到 Features 目录
  - 将 `TextSelectionPopover` 相关文件移动到 `Features/TextSelection/Controls/`
  - 更新命名空间为 `ConfigButtonDisplay.Features.TextSelection.Controls`
  - 更新 `MainWindow.axaml.cs` 中的引用
  - 验证编译通过
  - _Requirements: 1.1, 1.2, 1.3, 1.6_
  - _Commit: "移动文本选择组件到 Features 目录"_

- [ ] 35. 移动调试组件到 Features 目录
  - 将 `DebugOverlay` 和 `EnhancedDebugOverlay` 移动到 `Features/Debug/Controls/`
  - 更新命名空间为 `ConfigButtonDisplay.Features.Debug.Controls`
  - 更新 `MainWindow.axaml.cs` 中的引用
  - 验证编译通过
  - _Requirements: 1.1, 1.2, 1.3, 1.6_
  - _Commit: "移动调试组件到 Features 目录"_

## Phase 9: Dependency Injection Setup

- [ ] 36. 配置依赖注入容器
  - 在 `Program.cs` 或 `App.axaml.cs` 中配置 `ServiceCollection`
  - 注册 `IConfigurationService` 为单例
  - 注册 `IWindowPositionService` 为单例
  - 注册 `MainWindow` 和 `ConfigWindow` 为瞬态
  - 创建 `ServiceProvider` 并在应用启动时使用
  - _Requirements: 1.5_
  - _Commit: "配置依赖注入容器"_

- [ ] 37. 重构主窗口使用依赖注入
  - 修改 `MainWindow` 构造函数接受 `IConfigurationService` 和 `IWindowPositionService`
  - 移除手动创建服务实例的代码
  - 使用注入的服务实例
  - 在 `App.axaml.cs` 中从 `ServiceProvider` 获取 `MainWindow`
  - 验证应用启动和功能正常
  - _Requirements: 1.5_
  - _Commit: "重构主窗口使用依赖注入"_

## Phase 10: Final Integration and Polish

- [ ] 38. 集成所有配置到主窗口
  - 在主窗口启动时加载所有模块配置
  - 根据配置启用/禁用各个功能模块
  - 确保配置更改后立即生效
  - 添加配置加载失败的降级处理
  - _Requirements: 3.6, 5.2_
  - _Commit: "集成所有配置到主窗口"_

- [ ] 39. 优化动画和过渡效果
  - 为配置窗口的 TabControl 添加切换动画
  - 为主窗口的悬停效果添加缩放动画（scale 1.02）
  - 优化滑入动画的缓动函数
  - 确保所有动画流畅，帧率稳定
  - _Requirements: 2.8, 3.4_
  - _Commit: "优化动画和过渡效果"_

- [ ] 40. 添加配置导入导出功能
  - 在配置窗口添加"导出配置"按钮
  - 实现导出配置到 JSON 文件
  - 添加"导入配置"按钮
  - 实现从 JSON 文件导入配置
  - 导入时验证配置格式和版本
  - _Requirements: 5.5, 5.6_
  - _Commit: "添加配置导入导出功能"_

- [ ] 41. 完善错误提示和用户反馈
  - 为所有可能失败的操作添加错误提示
  - 使用 Fluent Design 风格的通知组件
  - 添加操作成功的确认提示
  - 确保错误消息清晰易懂
  - _Requirements: 3.7_
  - _Commit: "完善错误提示和用户反馈"_

- [ ] 42. 最终测试和文档更新
  - 运行完整的应用测试，验证所有功能
  - 测试配置保存和加载
  - 测试窗口定位和动画
  - 测试各个模块的配置应用
  - 更新 README.md 文档
  - 更新 IFLOW.md 项目文档
  - _Requirements: 所有需求_
  - _Commit: "最终测试和文档更新"_

---

## Task Execution Checklist

每个任务完成后执行：

```bash
# 1. 编译验证
dotnet build

# 2. 确认编译通过后提交
git add .
git commit -m "任务名称"

# 3. 如果编译失败，修复问题后重新编译
```

## Notes

- 所有任务按顺序执行，确保每一步都稳定
- 每个任务都包含具体的实现细节和需求引用
- 编译失败时不要继续下一个任务
- 提交消息使用任务名称，保持简洁明确

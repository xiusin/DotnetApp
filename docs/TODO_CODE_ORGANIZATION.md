# 代码文件整理待办事项

## 当前状态

文档已经整理完成，但代码文件的模块化整理尚未完成。

## 需要整理的文件

### 根目录下需要移动的文件

#### 1. 键盘监控相关
- `KeyDisplayWindow.axaml` → `Features/KeyboardMonitoring/Controls/`
- `KeyDisplayWindow.axaml.cs` → `Features/KeyboardMonitoring/Controls/`
- `KeyboardHook.cs` → `Infrastructure/Hooks/`

#### 2. 边缘组件相关
- `EdgeSwipeComponent.cs` → `Features/EdgeComponents/Controls/`
- `ClearEdgeComponent.cs` → `Features/EdgeComponents/Controls/`

#### 3. 配置相关
- `ConfigPopover.cs` → `Features/Configuration/Controls/`

#### 4. 弹出窗口相关
- `PopupWindow.axaml` → `Features/Popup/Controls/`
- `PopupWindow.axaml.cs` → `Features/Popup/Controls/`

## 需要完成的任务

### 1. 文件移动
- [ ] 移动文件到对应的功能模块目录
- [ ] 确保目录结构正确

### 2. 命名空间更新
- [ ] 更新移动文件的命名空间
  - `KeyDisplayWindow.axaml.cs`: `ConfigButtonDisplay` → `ConfigButtonDisplay.Features.KeyboardMonitoring.Controls`
  - `KeyDisplayWindow.axaml`: 更新 `x:Class` 属性
  - `KeyboardHook.cs`: `ConfigButtonDisplay` → `ConfigButtonDisplay.Infrastructure.Hooks`
  - `ConfigPopover.cs`: `ConfigButtonDisplay` → `ConfigButtonDisplay.Features.Configuration.Controls`
  - `PopupWindow.axaml.cs`: `ConfigButtonDisplay` → `ConfigButtonDisplay.Features.Popup.Controls`
  - `PopupWindow.axaml`: 更新 `x:Class` 属性
  - `EdgeSwipeComponent.cs`: `ConfigButtonDisplay` → `ConfigButtonDisplay.Features.EdgeComponents.Controls`
  - `ClearEdgeComponent.cs`: `ConfigButtonDisplay` → `ConfigButtonDisplay.Features.EdgeComponents.Controls`

### 3. 引用更新
- [ ] 更新 `MainWindow.axaml.cs` 的 using 语句
  ```csharp
  using ConfigButtonDisplay.Features.KeyboardMonitoring.Controls;
  using ConfigButtonDisplay.Features.Configuration.Controls;
  using ConfigButtonDisplay.Features.Popup.Controls;
  using ConfigButtonDisplay.Infrastructure.Hooks;
  ```

### 4. 编码问题修复
- [ ] 修复文件编码问题（UTF-8 BOM）
- [ ] 确保所有中文注释正确显示
- [ ] 检查字符串常量中的换行符问题

### 5. 编译验证
- [ ] 确保所有文件编译通过
- [ ] 运行应用测试功能正常
- [ ] 修复任何引用错误

## 遇到的问题

### 编码问题
在移动文件并更新命名空间后，发现以下文件存在编码问题：
- `ClearEdgeComponent.cs` - 多处字符串常量中有换行符错误
- `EdgeSwipeComponent.cs` - 多处字符串常量中有换行符错误
- `PopupWindow.axaml` - XML 编码问题
- `PopupWindow.axaml.cs` - 字符串常量中有换行符错误

### 错误类型
```
error CS1010: 常量中有换行符
error CS1003: 语法错误，应输入","
error CS1026: 应输入 )
error CS1002: 应输入 ;
```

## 解决方案

### 方案 1: 手动修复（推荐）
1. 逐个文件检查和修复编码问题
2. 使用 VS Code 或 Visual Studio 打开文件
3. 确保文件编码为 UTF-8
4. 检查所有字符串常量，确保没有意外的换行符
5. 重新保存文件

### 方案 2: 重新创建文件
1. 备份原文件内容
2. 删除有问题的文件
3. 创建新文件并复制内容
4. 确保使用正确的编码保存

### 方案 3: 使用工具批量转换
1. 使用 PowerShell 脚本批量转换编码
2. 使用 `iconv` 或类似工具
3. 验证转换后的文件

## 注意事项

1. **备份**: 在进行大规模文件移动前，确保有 Git 提交或备份
2. **逐步进行**: 一次移动一个模块，确保编译通过后再继续
3. **测试**: 每次移动后都要测试相关功能
4. **编码**: 确保所有文件使用 UTF-8 编码（无 BOM）
5. **命名空间**: 保持命名空间与文件夹结构一致

## 预期目录结构

```
ConfigButtonDisplay/
├── Features/
│   ├── AIChat/
│   │   └── Controls/
│   ├── Configuration/
│   │   └── Controls/
│   │       └── ConfigPopover.cs
│   ├── Debug/
│   │   └── Controls/
│   ├── EdgeComponents/
│   │   └── Controls/
│   │       ├── SimpleEdgeComponent.cs
│   │       ├── EdgeSwipeComponent.cs
│   │       └── ClearEdgeComponent.cs
│   ├── KeyboardMonitoring/
│   │   └── Controls/
│   │       ├── KeyDisplayWindow.axaml
│   │       └── KeyDisplayWindow.axaml.cs
│   ├── NoteTags/
│   │   └── Controls/
│   ├── Popup/
│   │   └── Controls/
│   │       ├── PopupWindow.axaml
│   │       └── PopupWindow.axaml.cs
│   └── TextSelection/
│       └── Controls/
├── Infrastructure/
│   ├── Converters/
│   ├── Helpers/
│   └── Hooks/
│       └── KeyboardHook.cs
├── Core/
├── Views/
├── ViewModels/
└── docs/
```

## 优先级

1. **高优先级**: 修复编码问题
2. **中优先级**: 移动文件和更新命名空间
3. **低优先级**: 优化目录结构

## 时间估计

- 编码问题修复: 1-2 小时
- 文件移动和命名空间更新: 1 小时
- 测试和验证: 30 分钟
- **总计**: 约 2.5-3.5 小时

## 相关文档

- [文档整理说明](DOCUMENTATION_ORGANIZATION.md)
- [项目结构](../IFLOW.md)
- [任务列表](../specs/ui-optimization-refactor/tasks.md)

## 更新日志

### 2025-01-07
- 创建此待办事项文档
- 记录需要整理的文件列表
- 记录遇到的编码问题
- 提出解决方案

---

**注意**: 在完成代码文件整理后，请更新此文档并标记完成的任务。

# 配置管理增强功能文档

## 概述

本次增强为应用程序添加了完整的配置管理系统，包括导入导出、备份恢复、版本迁移和高级验证功能。

## 新增服务

### 1. ConfigurationImportExportService
**功能**: 配置导入导出

#### 主要方法
- `ExportConfigurationAsync(settings, filePath)` - 导出配置到文件
- `ImportConfigurationAsync(filePath)` - 从文件导入配置
- `ExportToJson(settings)` - 导出为 JSON 字符串
- `ImportFromJson(json)` - 从 JSON 字符串导入

#### 特性
- ✅ 支持导出元数据（导出时间、用户、机器名）
- ✅ 自动验证导入的配置
- ✅ 支持多种配置格式
- ✅ 详细的错误信息

#### 使用示例
```csharp
var service = new ConfigurationImportExportService();

// 导出配置
await service.ExportConfigurationAsync(settings, "config_backup.json");

// 导入配置
var (success, settings, error) = await service.ImportConfigurationAsync("config_backup.json");
if (success)
{
    // 使用导入的配置
}
```

---

### 2. ConfigurationBackupService
**功能**: 自动备份和恢复

#### 主要方法
- `CreateBackupAsync(settings)` - 创建配置备份
- `GetAllBackups()` - 获取所有备份列表
- `RestoreFromBackupAsync(backupFilePath)` - 从备份恢复
- `DeleteBackup(backupFilePath)` - 删除备份

#### 特性
- ✅ 自动时间戳命名
- ✅ 保留最新 10 个备份
- ✅ 自动清理旧备份
- ✅ 备份信息展示（大小、日期）

#### 备份位置
```
%APPDATA%/ConfigButtonDisplay/Backups/
├── backup_20250107_143022.json
├── backup_20250107_150315.json
└── backup_20250107_162145.json
```

#### 使用示例
```csharp
var service = new ConfigurationBackupService();

// 创建备份
var backupPath = await service.CreateBackupAsync(settings);

// 获取所有备份
var backups = service.GetAllBackups();
foreach (var backup in backups)
{
    Console.WriteLine($"{backup.FileName} - {backup.FormattedSize} - {backup.FormattedDate}");
}

// 恢复备份
var restored = await service.RestoreFromBackupAsync(backupPath);
```

---

### 3. ConfigurationMigrationService
**功能**: 版本管理和迁移

#### 主要方法
- `NeedsMigration(settings)` - 检查是否需要迁移
- `MigrateToLatest(settings)` - 迁移到最新版本
- `ValidateConfiguration(settings)` - 验证配置完整性
- `RepairConfiguration(settings)` - 修复配置问题

#### 特性
- ✅ 自动检测配置版本
- ✅ 逐步迁移机制
- ✅ 配置修复功能
- ✅ 详细的迁移日志

#### 版本迁移流程
```
V1 配置 → 检测版本 → 执行迁移 → V2 配置 → 保存
```

#### 使用示例
```csharp
var service = new ConfigurationMigrationService();

// 检查是否需要迁移
if (service.NeedsMigration(settings))
{
    // 执行迁移
    settings = service.MigrateToLatest(settings);
}

// 验证配置
var (isValid, issues) = service.ValidateConfiguration(settings);
if (!isValid)
{
    // 修复配置
    settings = service.RepairConfiguration(settings);
}
```

---

### 4. ConfigurationValidator
**功能**: 高级配置验证

#### 主要方法
- `Validate(settings)` - 验证配置

#### 验证规则
1. **版本验证** - 检查版本号有效性
2. **窗口配置验证** - 透明度、位置、坐标
3. **键盘监控验证** - 颜色格式、数值范围、逻辑一致性
4. **逻辑一致性验证** - 检查配置的合理性

#### 验证结果
- **错误 (Errors)**: 必须修复的问题
- **警告 (Warnings)**: 建议修复的问题

#### 使用示例
```csharp
var validator = new ConfigurationValidator();
var result = validator.Validate(settings);

if (!result.IsValid)
{
    Console.WriteLine("配置验证失败:");
    Console.WriteLine(result.GetSummary());
}
```

---

### 5. ConfigurationManager
**功能**: 统一的配置管理接口

#### 主要方法
- `LoadConfigurationAsync()` - 加载配置（带验证和迁移）
- `SaveConfigurationAsync(settings)` - 保存配置（带验证和备份）
- `ExportConfigurationAsync(filePath)` - 导出配置
- `ImportConfigurationAsync(filePath)` - 导入配置
- `RestoreBackupAsync(backupFilePath)` - 恢复备份
- `ResetToDefaultAsync()` - 重置为默认配置
- `GetConfigurationInfoAsync()` - 获取配置信息

#### 特性
- ✅ 自动验证
- ✅ 自动迁移
- ✅ 自动备份
- ✅ 统一接口
- ✅ 错误处理

#### 使用示例
```csharp
var manager = new ConfigurationManager(configService);

// 加载配置（自动验证和迁移）
var settings = await manager.LoadConfigurationAsync();

// 保存配置（自动验证和备份）
await manager.SaveConfigurationAsync(settings);

// 导出配置
await manager.ExportConfigurationAsync("my_config.json");

// 导入配置
await manager.ImportConfigurationAsync("my_config.json");

// 获取配置信息
var info = await manager.GetConfigurationInfoAsync();
Console.WriteLine(info.GetStatusText());
```

---

## 完整工作流程

### 1. 应用启动时
```
加载配置 → 检查版本 → 执行迁移（如需要）→ 验证配置 → 修复问题（如需要）
```

### 2. 保存配置时
```
验证配置 → 创建备份 → 保存配置 → 清理旧备份
```

### 3. 导入配置时
```
读取文件 → 验证格式 → 备份当前配置 → 应用新配置
```

### 4. 恢复备份时
```
选择备份 → 验证备份 → 应用备份配置
```

---

## 配置文件结构

### 标准配置文件
```json
{
  "version": 1,
  "window": {
    "position": "RightEdge",
    "opacity": 0.95,
    "alwaysOnTop": true
  },
  "keyboardMonitor": {
    "enabled": true,
    "fontSize": 28,
    "backgroundColor": "#3182CE"
  }
}
```

### 导出配置文件（带元数据）
```json
{
  "version": 1,
  "exportDate": "2025-01-07T14:30:22Z",
  "exportedBy": "username",
  "machineName": "DESKTOP-ABC123",
  "settings": {
    "version": 1,
    "window": { ... },
    "keyboardMonitor": { ... }
  }
}
```

---

## 错误处理

### 配置加载失败
- 自动创建默认配置
- 记录错误日志
- 通知用户

### 配置验证失败
- 显示详细错误信息
- 尝试自动修复
- 提供手动修复选项

### 备份失败
- 记录错误但不阻止操作
- 通知用户备份失败
- 继续执行主要操作

---

## 性能优化

### 1. 缓存机制
- 配置加载后缓存 60 秒
- 减少重复的文件 I/O

### 2. 异步操作
- 所有 I/O 操作都是异步的
- 不阻塞 UI 线程

### 3. 备份清理
- 自动清理旧备份
- 保持备份数量在合理范围

---

## 安全性

### 1. 验证
- 严格的输入验证
- 防止注入攻击
- 类型安全

### 2. 备份
- 操作前自动备份
- 支持快速恢复
- 防止数据丢失

### 3. 错误恢复
- 配置损坏时自动修复
- 提供默认配置
- 保留备份副本

---

## 使用建议

### 1. 定期导出配置
建议用户定期导出配置到安全位置，作为额外备份。

### 2. 升级前备份
在应用升级前，建议手动创建配置备份。

### 3. 验证导入配置
导入配置后，建议检查配置是否符合预期。

### 4. 监控备份数量
定期检查备份目录，确保备份功能正常工作。

---

## 集成到现有代码

### 在 App.axaml.cs 中注册服务
```csharp
services.AddSingleton<ConfigurationManager>();
services.AddSingleton<ConfigurationImportExportService>();
services.AddSingleton<ConfigurationBackupService>();
services.AddSingleton<ConfigurationMigrationService>();
services.AddSingleton<ConfigurationValidator>();
```

### 在 ConfigWindow 中使用
```csharp
private readonly ConfigurationManager _configManager;

public ConfigWindow(ConfigurationManager configManager)
{
    _configManager = configManager;
}

private async void ExportButton_Click(object sender, RoutedEventArgs e)
{
    var dialog = new SaveFileDialog
    {
        DefaultExtension = ".json",
        Filter = "JSON Files (*.json)|*.json"
    };
    
    var result = await dialog.ShowAsync(this);
    if (result != null)
    {
        await _configManager.ExportConfigurationAsync(result);
    }
}
```

---

## 总结

配置管理增强功能提供了：
- ✅ 完整的导入导出功能
- ✅ 自动备份和恢复
- ✅ 版本管理和迁移
- ✅ 高级验证和修复
- ✅ 统一的管理接口
- ✅ 详细的错误处理
- ✅ 性能优化
- ✅ 安全保障

这些功能大大提升了应用的可靠性和用户体验！

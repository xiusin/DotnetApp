using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ConfigButtonDisplay.Core.Configuration;

namespace ConfigButtonDisplay.Core.Services;

/// <summary>
/// 配置验证器 - 提供详细的配置验证
/// </summary>
public class ConfigurationValidator
{
    private readonly List<ValidationRule> _rules = new();

    public ConfigurationValidator()
    {
        InitializeRules();
    }

    /// <summary>
    /// 验证配置
    /// </summary>
    public ValidationResult Validate(AppSettings settings)
    {
        var result = new ValidationResult();

        if (settings == null)
        {
            result.AddError("配置对象为空");
            return result;
        }

        // 执行所有验证规则
        foreach (var rule in _rules)
        {
            try
            {
                rule.Validate(settings, result);
            }
            catch (Exception ex)
            {
                result.AddError($"验证规则 '{rule.Name}' 执行失败: {ex.Message}");
            }
        }

        return result;
    }

    /// <summary>
    /// 初始化验证规则
    /// </summary>
    private void InitializeRules()
    {
        // 版本验证
        _rules.Add(new ValidationRule
        {
            Name = "版本验证",
            Validate = (settings, result) =>
            {
                if (settings.Version < 1)
                    result.AddError("配置版本必须大于等于 1");
            }
        });

        // 窗口配置验证
        _rules.Add(new ValidationRule
        {
            Name = "窗口配置验证",
            Validate = (settings, result) =>
            {
                if (settings.Window == null)
                {
                    result.AddError("窗口配置缺失");
                    return;
                }

                if (settings.Window.Opacity < 0.5 || settings.Window.Opacity > 1.0)
                    result.AddError($"窗口透明度必须在 0.5 到 1.0 之间，当前值: {settings.Window.Opacity}");

                if (settings.Window.Position == "Custom")
                {
                    if (!settings.Window.CustomX.HasValue || !settings.Window.CustomY.HasValue)
                        result.AddError("选择自定义位置时必须指定 X 和 Y 坐标");
                    else
                    {
                        if (settings.Window.CustomX < 0 || settings.Window.CustomY < 0)
                            result.AddError("自定义位置坐标必须为正数");

                        if (settings.Window.CustomX > 10000 || settings.Window.CustomY > 10000)
                            result.AddWarning("自定义位置坐标可能超出屏幕范围");
                    }
                }
            }
        });

        // 键盘监控配置验证
        _rules.Add(new ValidationRule
        {
            Name = "键盘监控配置验证",
            Validate = (settings, result) =>
            {
                if (settings.KeyboardMonitor == null)
                {
                    result.AddError("键盘监控配置缺失");
                    return;
                }

                var km = settings.KeyboardMonitor;

                // 颜色验证
                if (!IsValidHexColor(km.BackgroundColor))
                    result.AddError($"背景颜色格式无效: {km.BackgroundColor}");

                if (!IsValidHexColor(km.FontColor))
                    result.AddError($"字体颜色格式无效: {km.FontColor}");

                // 数值范围验证
                if (km.FontSize < 12 || km.FontSize > 48)
                    result.AddError($"字体大小必须在 12 到 48 之间，当前值: {km.FontSize}");

                if (km.Opacity < 0.1 || km.Opacity > 1.0)
                    result.AddError($"透明度必须在 0.1 到 1.0 之间，当前值: {km.Opacity}");

                if (km.DisplayDuration < 1 || km.DisplayDuration > 10)
                    result.AddError($"显示时长必须在 1 到 10 秒之间，当前值: {km.DisplayDuration}");

                if (km.FadeInDuration < 0.1 || km.FadeInDuration > 1.0)
                    result.AddError($"淡入时长必须在 0.1 到 1.0 秒之间，当前值: {km.FadeInDuration}");

                if (km.FadeOutDuration < 0.1 || km.FadeOutDuration > 1.0)
                    result.AddError($"淡出时长必须在 0.1 到 1.0 秒之间，当前值: {km.FadeOutDuration}");

                // 欢迎消息验证
                if (km.ShowWelcomeMessage)
                {
                    if (string.IsNullOrWhiteSpace(km.WelcomeMessage))
                        result.AddWarning("欢迎消息内容为空");

                    if (km.WelcomeMessageDuration < 1 || km.WelcomeMessageDuration > 10)
                        result.AddError($"欢迎消息时长必须在 1 到 10 秒之间，当前值: {km.WelcomeMessageDuration}");
                }
            }
        });

        // 逻辑一致性验证
        _rules.Add(new ValidationRule
        {
            Name = "逻辑一致性验证",
            Validate = (settings, result) =>
            {
                if (settings.KeyboardMonitor != null)
                {
                    // 检查是否至少显示一种按键类型
                    if (!settings.KeyboardMonitor.ShowModifiers &&
                        !settings.KeyboardMonitor.ShowFunctionKeys &&
                        !settings.KeyboardMonitor.ShowAlphaNumeric &&
                        !settings.KeyboardMonitor.ShowNavigation)
                    {
                        result.AddWarning("所有按键类型都被禁用，键盘监控将不显示任何内容");
                    }
                }
            }
        });
    }

    /// <summary>
    /// 验证 Hex 颜色格式
    /// </summary>
    private bool IsValidHexColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            return false;

        var hexPattern = @"^#([A-Fa-f0-9]{3}|[A-Fa-f0-9]{6}|[A-Fa-f0-9]{8})$";
        return Regex.IsMatch(color, hexPattern);
    }
}

/// <summary>
/// 验证规则
/// </summary>
public class ValidationRule
{
    public string Name { get; set; } = string.Empty;
    public Action<AppSettings, ValidationResult> Validate { get; set; } = (_, _) => { };
}

/// <summary>
/// 验证结果
/// </summary>
public class ValidationResult
{
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();

    public bool IsValid => Errors.Count == 0;
    public bool HasWarnings => Warnings.Count > 0;

    public void AddError(string error)
    {
        Errors.Add(error);
    }

    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }

    public string GetSummary()
    {
        var summary = new List<string>();

        if (Errors.Count > 0)
        {
            summary.Add($"错误 ({Errors.Count}):");
            summary.AddRange(Errors.Select(e => $"  - {e}"));
        }

        if (Warnings.Count > 0)
        {
            summary.Add($"警告 ({Warnings.Count}):");
            summary.AddRange(Warnings.Select(w => $"  - {w}"));
        }

        if (IsValid && !HasWarnings)
        {
            summary.Add("✓ 配置验证通过");
        }

        return string.Join(Environment.NewLine, summary);
    }
}

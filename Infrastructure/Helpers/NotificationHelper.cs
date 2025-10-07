using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace ConfigButtonDisplay.Infrastructure.Helpers;

/// <summary>
/// 通知助手类，提供 Fluent Design 风格的通知提示
/// </summary>
public static class NotificationHelper
{
    /// <summary>
    /// 显示成功通知
    /// </summary>
    public static async Task ShowSuccess(Window owner, string message, string title = "成功", int duration = 3000)
    {
        await ShowNotification(owner, message, title, NotificationType.Success, duration);
    }

    /// <summary>
    /// 显示错误通知
    /// </summary>
    public static async Task ShowError(Window owner, string message, string title = "错误", int duration = 5000)
    {
        await ShowNotification(owner, message, title, NotificationType.Error, duration);
    }

    /// <summary>
    /// 显示警告通知
    /// </summary>
    public static async Task ShowWarning(Window owner, string message, string title = "警告", int duration = 4000)
    {
        await ShowNotification(owner, message, title, NotificationType.Warning, duration);
    }

    /// <summary>
    /// 显示信息通知
    /// </summary>
    public static async Task ShowInfo(Window owner, string message, string title = "提示", int duration = 3000)
    {
        await ShowNotification(owner, message, title, NotificationType.Info, duration);
    }

    /// <summary>
    /// 显示通知
    /// </summary>
    private static async Task ShowNotification(Window owner, string message, string title, NotificationType type, int duration)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                // 创建通知窗口
                var notificationWindow = new Window
                {
                    Width = 350,
                    Height = 120,
                    SystemDecorations = SystemDecorations.None,
                    ShowInTaskbar = false,
                    Topmost = true,
                    CanResize = false,
                    Background = Brushes.Transparent,
                    TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent }
                };

                // 获取通知颜色
                var (backgroundColor, borderColor, iconColor) = GetNotificationColors(type);
                var icon = GetNotificationIcon(type);

                // 创建通知内容
                var content = new Border
                {
                    Background = new SolidColorBrush(backgroundColor),
                    BorderBrush = new SolidColorBrush(borderColor),
                    BorderThickness = new Thickness(1, 1, 1, 4),
                    CornerRadius = new CornerRadius(12),
                    BoxShadow = new BoxShadows(new BoxShadow
                    {
                        OffsetX = 0,
                        OffsetY = 4,
                        Blur = 16,
                        Color = Color.FromArgb(64, 0, 0, 0)
                    }),
                    Padding = new Thickness(16),
                    Child = new StackPanel
                    {
                        Spacing = 8,
                        Children =
                        {
                            new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                Spacing = 12,
                                Children =
                                {
                                    new TextBlock
                                    {
                                        Text = icon,
                                        FontSize = 24,
                                        Foreground = new SolidColorBrush(iconColor),
                                        VerticalAlignment = VerticalAlignment.Center
                                    },
                                    new TextBlock
                                    {
                                        Text = title,
                                        FontSize = 16,
                                        FontWeight = FontWeight.SemiBold,
                                        Foreground = new SolidColorBrush(Color.Parse("#2D3748")),
                                        VerticalAlignment = VerticalAlignment.Center
                                    }
                                }
                            },
                            new TextBlock
                            {
                                Text = message,
                                FontSize = 14,
                                Foreground = new SolidColorBrush(Color.Parse("#4A5568")),
                                TextWrapping = TextWrapping.Wrap,
                                Margin = new Thickness(36, 0, 0, 0)
                            }
                        }
                    }
                };

                notificationWindow.Content = content;

                // 计算通知位置（右下角）
                if (owner != null)
                {
                    var ownerPosition = owner.Position;
                    var ownerSize = new Size(owner.Width, owner.Height);
                    var x = ownerPosition.X + (int)ownerSize.Width - 370;
                    var y = ownerPosition.Y + (int)ownerSize.Height - 140;
                    notificationWindow.Position = new PixelPoint(x, y);
                }

                // 显示通知
                notificationWindow.Show();

                // 淡入动画
                notificationWindow.Opacity = 0;
                await AnimationHelper.FadeIn(notificationWindow, 200);

                // 等待指定时长
                await Task.Delay(duration);

                // 淡出动画
                await AnimationHelper.FadeOut(notificationWindow, 300);

                // 关闭通知
                notificationWindow.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing notification: {ex.Message}");
            }
        });
    }

    /// <summary>
    /// 获取通知颜色
    /// </summary>
    private static (Color background, Color border, Color icon) GetNotificationColors(NotificationType type)
    {
        return type switch
        {
            NotificationType.Success => (
                Color.Parse("#F0FDF4"),
                Color.Parse("#22C55E"),
                Color.Parse("#16A34A")
            ),
            NotificationType.Error => (
                Color.Parse("#FEF2F2"),
                Color.Parse("#EF4444"),
                Color.Parse("#DC2626")
            ),
            NotificationType.Warning => (
                Color.Parse("#FFFBEB"),
                Color.Parse("#F59E0B"),
                Color.Parse("#D97706")
            ),
            NotificationType.Info => (
                Color.Parse("#EFF6FF"),
                Color.Parse("#3B82F6"),
                Color.Parse("#2563EB")
            ),
            _ => (
                Color.Parse("#F9FAFB"),
                Color.Parse("#9CA3AF"),
                Color.Parse("#6B7280")
            )
        };
    }

    /// <summary>
    /// 获取通知图标
    /// </summary>
    private static string GetNotificationIcon(NotificationType type)
    {
        return type switch
        {
            NotificationType.Success => "✓",
            NotificationType.Error => "✕",
            NotificationType.Warning => "⚠",
            NotificationType.Info => "ℹ",
            _ => "•"
        };
    }
}

/// <summary>
/// 通知类型
/// </summary>
public enum NotificationType
{
    Success,
    Error,
    Warning,
    Info
}

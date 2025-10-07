using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConfigButtonDisplay.Features.AIChat.Controls;

public class AIChatWindow : Window
{
    private TextBox? _messageInput;
    private StackPanel? _messagesPanel;
    private Button? _sendButton;
    private bool _isShiftPressed = false;
    private DateTime _lastShiftPressTime = DateTime.MinValue;
    private const int DOUBLE_CLICK_INTERVAL = 300; // 双击间隔（毫秒）
    
    public AIChatWindow()
    {
        InitializeComponent();
        // 移除双击Shift检测，由MainWindow统一管理
    }
    
    private void InitializeComponent()
    {
        Title = "AI 聊天助手";
        Width = 400;
        Height = 600;
        MinWidth = 350;
        MinHeight = 500;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        CanResize = true;
        ShowInTaskbar = false;
        Topmost = false;
        SystemDecorations = SystemDecorations.BorderOnly;
        Background = new SolidColorBrush(Color.Parse("#FFFFFF"));
        
        // 主容器
        var mainGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*,Auto"),
            Margin = new Thickness(0)
        };
        
        // 标题栏
        var titleBar = CreateTitleBar();
        Grid.SetRow(titleBar, 0);
        mainGrid.Children.Add(titleBar);
        
        // 消息区域
        var messagesArea = CreateMessagesArea();
        Grid.SetRow(messagesArea, 1);
        mainGrid.Children.Add(messagesArea);
        
        // 输入区域
        var inputArea = CreateInputArea();
        Grid.SetRow(inputArea, 2);
        mainGrid.Children.Add(inputArea);
        
        Content = mainGrid;
        
        // 添加欢迎消息
        AddWelcomeMessage();
    }
    
    private Border CreateTitleBar()
    {
        var titleBorder = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#1890FF")),
            Padding = new Thickness(16, 12),
            CornerRadius = new CornerRadius(0)
        };
        
        var titleGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto")
        };
        
        // 标题
        var titleText = new TextBlock
        {
            Text = "AI 助手",
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        // 关闭按钮
        var closeButton = new Button
        {
            Content = "✕",
            Background = Brushes.Transparent,
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Width = 24,
            Height = 24,
            FontSize = 14,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        closeButton.Click += (s, e) => Hide();
        
        Grid.SetColumn(titleText, 0);
        Grid.SetColumn(closeButton, 1);
        
        titleGrid.Children.Add(titleText);
        titleGrid.Children.Add(closeButton);
        
        titleBorder.Child = titleGrid;
        return titleBorder;
    }
    
    private ScrollViewer CreateMessagesArea()
    {
        var scrollViewer = new ScrollViewer
        {
            Background = new SolidColorBrush(Color.Parse("#FAFAFA")),
            Padding = new Thickness(16)
            // 移除ScrollBarVisibility设置，使用默认值
        };
        
        _messagesPanel = new StackPanel
        {
            Spacing = 12
        };
        
        scrollViewer.Content = _messagesPanel;
        return scrollViewer;
    }
    
    private Border CreateInputArea()
    {
        var inputBorder = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#FFFFFF")),
            BorderBrush = new SolidColorBrush(Color.Parse("#F0F0F0")),
            BorderThickness = new Thickness(1, 0, 0, 0),
            Padding = new Thickness(16),
            CornerRadius = new CornerRadius(0)
        };
        
        var inputGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Margin = new Thickness(0)
        };
        
        // 消息输入框
        _messageInput = new TextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            MaxHeight = 100,
            Background = new SolidColorBrush(Color.Parse("#F5F5F5")),
            BorderBrush = new SolidColorBrush(Color.Parse("#D9D9D9")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12, 8),
            FontSize = 14,
            Watermark = "输入消息，按 Enter 发送..."
        };
        _messageInput.KeyDown += OnMessageInputKeyDown;
        
        // 发送按钮
        _sendButton = new Button
        {
            Content = "发送",
            Background = new SolidColorBrush(Color.Parse("#1890FF")),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16, 8),
            FontSize = 14,
            FontWeight = FontWeight.Medium,
            Cursor = new Cursor(StandardCursorType.Hand),
            Margin = new Thickness(8, 0, 0, 0)
        };
        _sendButton.Click += OnSendButtonClick;
        
        Grid.SetColumn(_messageInput, 0);
        Grid.SetColumn(_sendButton, 1);
        
        inputGrid.Children.Add(_messageInput);
        inputGrid.Children.Add(_sendButton);
        
        inputBorder.Child = inputGrid;
        return inputBorder;
    }
    
    private void SetupDoubleShiftDetection()
    {
        // 监听全局键盘事件
        if (Application.Current?.ApplicationLifetime 
            is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow != null)
        {
            desktop.MainWindow.AddHandler(KeyDownEvent, OnGlobalKeyDown, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        }
    }
    
    private void OnGlobalKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
        {
            var now = DateTime.Now;
            var timeSinceLastPress = (now - _lastShiftPressTime).TotalMilliseconds;
            
            if (_isShiftPressed && timeSinceLastPress < DOUBLE_CLICK_INTERVAL)
            {
                // 双击Shift键检测成功
                Dispatcher.UIThread.Post(() =>
                {
                    if (!IsVisible)
                    {
                        Show();
                        // 不主动抢占焦点
                    }
                    else
                    {
                        Hide();
                    }
                });
                
                // 重置状态
                _isShiftPressed = false;
                _lastShiftPressTime = DateTime.MinValue;
            }
            else
            {
                _isShiftPressed = true;
                _lastShiftPressTime = now;
                
                // 设置超时，避免长时间等待
                Task.Delay(DOUBLE_CLICK_INTERVAL).ContinueWith(_ =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        _isShiftPressed = false;
                    });
                });
            }
        }
    }
    
    private void OnMessageInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && !e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            e.Handled = true;
            SendMessage();
        }
    }
    
    private void OnSendButtonClick(object? sender, RoutedEventArgs e)
    {
        SendMessage();
    }
    
    private async void SendMessage()
    {
        if (_messageInput == null || string.IsNullOrWhiteSpace(_messageInput.Text))
            return;
        
        var message = _messageInput.Text.Trim();
        _messageInput.Text = string.Empty;
        
        // 添加用户消息
        AddMessage(message, true);
        
        // 显示正在输入状态
        ShowTypingIndicator();
        
        // 模拟AI回复
        await Task.Delay(1500);
        
        HideTypingIndicator();
        
        var response = GenerateAIResponse(message);
        AddMessage(response, false);
    }
    
    private void AddMessage(string text, bool isUser)
    {
        if (_messagesPanel == null) return;
        
        var messageBorder = new Border
        {
            Background = isUser ? new SolidColorBrush(Color.Parse("#1890FF")) : new SolidColorBrush(Color.Parse("#F0F0F0")),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(12, 8),
            Margin = new Thickness(isUser ? 40 : 0, 0, isUser ? 0 : 40, 0),
            HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            MaxWidth = 280
        };
        
        var messageText = new TextBlock
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 14,
            Foreground = isUser ? Brushes.White : new SolidColorBrush(Color.Parse("#1C1E21")),
            LineHeight = 20
        };
        
        messageBorder.Child = messageText;
        _messagesPanel.Children.Add(messageBorder);
        
        // 滚动到底部
        Dispatcher.UIThread.Post(() =>
        {
            if (_messagesPanel.Parent is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToEnd();
            }
        });
    }
    
    private void ShowTypingIndicator()
    {
        if (_messagesPanel == null) return;
        
        var typingBorder = new Border
        {
            Name = "TypingIndicator",
            Background = new SolidColorBrush(Color.Parse("#F0F0F0")),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(12, 8),
            Margin = new Thickness(0, 0, 40, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            MaxWidth = 100
        };
        
        var typingText = new TextBlock
        {
            Text = "正在输入...",
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#6C757D")),
            FontStyle = FontStyle.Italic
        };
        
        typingBorder.Child = typingText;
        _messagesPanel.Children.Add(typingBorder);
        
        Dispatcher.UIThread.Post(() =>
        {
            if (_messagesPanel.Parent is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToEnd();
            }
        });
    }
    
    private void HideTypingIndicator()
    {
        if (_messagesPanel == null) return;
        
        for (int i = _messagesPanel.Children.Count - 1; i >= 0; i--)
        {
            if (_messagesPanel.Children[i] is Border border && border.Name == "TypingIndicator")
            {
                _messagesPanel.Children.RemoveAt(i);
                break;
            }
        }
    }
    
    private string GenerateAIResponse(string userMessage)
    {
        // 简单的响应生成逻辑（实际项目中应该调用真实的AI API）
        var responses = new[]
        {
            "我理解您的意思。这是一个很有趣的问题，让我思考一下...",
            "根据我的分析，您可以考虑以下方案：",
            "这是个很好的观点！我建议您：",
            "我明白了。基于您的需求，推荐的做法是：",
            "有趣的问题！从我的角度来看："
        };
        
        var random = new Random();
        var baseResponse = responses[random.Next(responses.Length)];
        
        if (userMessage.Length > 20)
        {
            baseResponse += "\n\n" + GetDetailedResponse(userMessage);
        }
        
        return baseResponse;
    }
    
    private string GetDetailedResponse(string userMessage)
    {
        if (userMessage.Contains("帮助") || userMessage.Contains("assist"))
        {
            return "我可以帮助您解答问题、提供建议、协助完成任务等。请告诉我您需要什么帮助！";
        }
        else if (userMessage.Contains("代码") || userMessage.Contains("code"))
        {
            return "我可以协助您编写代码、调试程序、解释算法等。请提供具体的需求或问题。";
        }
        else if (userMessage.Contains("学习") || userMessage.Contains("study"))
        {
            return "学习是一个持续的过程。建议您制定明确的学习计划，并通过实践来巩固知识。";
        }
        else
        {
            return "感谢您的分享！如果您有其他问题或需要进一步的说明，请随时告诉我。";
        }
    }
    
    private void AddWelcomeMessage()
    {
        AddMessage("👋 欢迎使用AI聊天助手！可以通过双击Shift键或手动测试按钮打开/关闭此窗口。有什么可以帮助您的吗？", false);
    }
    
    public void ShowChatWindow()
    {
        if (!IsVisible)
        {
            Show();
            // 不主动抢占焦点，保持 Topmost 可见
        }
    }
    
    public void HideChatWindow()
    {
        if (IsVisible)
        {
            Hide();
        }
    }
    
    public void ToggleChatWindow()
    {
        if (!IsVisible)
        {
            ShowChatWindow();
        }
        else
        {
            HideChatWindow();
        }
    }
    
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        // 清理资源
        if (Application.Current?.ApplicationLifetime 
            is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
            && desktop.MainWindow != null)
        {
            desktop.MainWindow.RemoveHandler(KeyDownEvent, OnGlobalKeyDown);
        }
    }
}
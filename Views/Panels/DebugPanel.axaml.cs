using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ConfigButtonDisplay.Views.Panels;

public partial class DebugPanel : UserControl
{
    public DebugPanel()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

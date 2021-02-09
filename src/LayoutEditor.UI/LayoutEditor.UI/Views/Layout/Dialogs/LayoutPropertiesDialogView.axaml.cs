using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LayoutEditor.UI.Controls;

namespace LayoutEditor.UI.Views.Layout.Dialogs
{
    public class LayoutPropertiesDialogView : FluentWindow
    {
        public LayoutPropertiesDialogView()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

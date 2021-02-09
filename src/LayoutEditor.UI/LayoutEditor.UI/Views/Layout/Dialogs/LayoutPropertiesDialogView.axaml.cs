using Avalonia;
using Avalonia.Markup.Xaml;
using LayoutEditor.UI.Services.DialogService;

namespace LayoutEditor.UI.Views.Layout.Dialogs
{
    public class LayoutPropertiesDialogView : DialogWindowBase
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
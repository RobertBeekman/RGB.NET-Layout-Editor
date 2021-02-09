using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LayoutEditor.UI.Controls;

namespace LayoutEditor.UI.Views
{
    public class MainWindow : FluentWindow
    {
        public MainWindow()
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

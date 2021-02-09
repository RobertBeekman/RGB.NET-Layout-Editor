using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LayoutEditor.UI.Views
{
    public class EditorView : UserControl
    {
        public EditorView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

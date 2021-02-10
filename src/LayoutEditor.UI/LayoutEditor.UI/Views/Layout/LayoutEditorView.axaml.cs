using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using LayoutEditor.UI.ViewModels.Layout;
using ReactiveUI;

namespace LayoutEditor.UI.Views.Layout
{
    public class LayoutEditorView : ReactiveUserControl<LayoutEditorViewModel>
    {
        public LayoutEditorView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
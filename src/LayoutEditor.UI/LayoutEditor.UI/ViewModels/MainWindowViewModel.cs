namespace LayoutEditor.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        // Design time constructor
        public MainWindowViewModel()
        {
            EditorViewModel = null!;
        }

        public MainWindowViewModel(EditorViewModel editorViewModel)
        {
            EditorViewModel = editorViewModel;
        }

        public EditorViewModel EditorViewModel { get; set; }
    }
}
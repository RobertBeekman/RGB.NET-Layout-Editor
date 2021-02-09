namespace LayoutEditor.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(EditorViewModel editorViewModel)
        {
            EditorViewModel = editorViewModel;
        }

        public EditorViewModel EditorViewModel { get; set; }
    }
}
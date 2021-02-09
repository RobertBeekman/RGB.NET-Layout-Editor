using System.Collections.ObjectModel;
using System.Linq;
using LayoutEditor.UI.ViewModels.Layout;

namespace LayoutEditor.UI.ViewModels
{
    public class EditorViewModel : ViewModelBase
    {
        public EditorViewModel(LayoutEditorViewModel layoutEditorViewModel)
        {
            layoutEditorViewModel.Name = "Corsair K95";
            layoutEditorViewModel.FilePath = @"C:\..\K95.xml";
            Items = new ObservableCollection<LayoutEditorViewModel>
            {
                layoutEditorViewModel,
            };
            SelectedItem = Items.FirstOrDefault();
        }

        public ObservableCollection<LayoutEditorViewModel> Items { get; }
        public LayoutEditorViewModel? SelectedItem { get; set; }
    }
}
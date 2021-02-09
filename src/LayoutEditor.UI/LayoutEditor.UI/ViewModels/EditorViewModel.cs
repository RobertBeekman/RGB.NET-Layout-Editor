using System.Collections.ObjectModel;
using System.Linq;
using LayoutEditor.UI.ViewModels.Layout;

namespace LayoutEditor.UI.ViewModels
{
    public class EditorViewModel : ViewModelBase
    {
        public EditorViewModel()
        {
            Items = new ObservableCollection<LayoutEditorViewModel>
            {
                new() {Name = "Corsair K95", FilePath = @"C:\..\K95.xml"}
            };
            SelectedItem = Items.FirstOrDefault();
        }

        public ObservableCollection<LayoutEditorViewModel> Items { get; }
        public LayoutEditorViewModel? SelectedItem { get; set; }
    }
}
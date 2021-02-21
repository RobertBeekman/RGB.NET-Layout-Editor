using LayoutEditor.UI.Pages;
using Stylet;

namespace LayoutEditor.UI.Dialogs
{
    public class AddImageLayoutViewModel : Screen
    {
        private readonly DeviceLayoutEditorViewModel _deviceLayoutEditorViewModel;
        private readonly IWindowManager _windowManager;

        public AddImageLayoutViewModel(IWindowManager windowManager, DeviceLayoutEditorViewModel deviceLayoutEditorViewModel)
        {
            _windowManager = windowManager;
            _deviceLayoutEditorViewModel = deviceLayoutEditorViewModel;
        }

        public string ImageLayout { get; set; }

        public void AddImageLayout()
        {
            if (_deviceLayoutEditorViewModel.LogicalLayouts.Contains(ImageLayout))
            {
                _windowManager.ShowMessageBox("That image layout already exists.");
                return;
            }

            _deviceLayoutEditorViewModel.LogicalLayouts.Add(ImageLayout);
            _deviceLayoutEditorViewModel.SelectedLogicalLayout = ImageLayout;
            RequestClose(true);
        }

        public void Cancel()
        {
            RequestClose(false);
        }
    }
}
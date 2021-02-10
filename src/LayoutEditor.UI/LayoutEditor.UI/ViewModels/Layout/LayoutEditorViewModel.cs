using System.Reactive.Disposables;
using System.Threading.Tasks;
using LayoutEditor.UI.Services.DialogService.Interfaces;
using LayoutEditor.UI.ViewModels.Layout.Dialogs;
using LayoutEditor.UI.Views.Layout;
using ReactiveUI;
using RGB.NET.Layout;

namespace LayoutEditor.UI.ViewModels.Layout
{
    public class LayoutEditorViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;

        public LayoutEditorViewModel()
        {
        }

        public LayoutEditorViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            LoadDeviceLayout(@"C:\ProgramData\Artemis\plugins\Artemis.Plugins.Devices.Corsair\Layouts\Corsair\Keyboards\K95RGBPLATINUMXT\UK.xml");
        }

        private void LoadDeviceLayout(string filePath)
        {
            FilePath = filePath;
            DeviceLayout = DeviceLayout.Load(filePath);
            Name = DeviceLayout?.Name ?? "Invalid layout";
        }

        public DeviceLayout? DeviceLayout { get; set; }
        public string? Name { get; set; }
        public string FilePath { get; set; }

        public async Task OpenDetails()
        {
            await _dialogService.ShowDialogAsync<LayoutPropertiesDialogViewModel>();
        }
    }
}
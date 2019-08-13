using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;
using RGB.NET.Core.Layout;
using Stylet;

namespace LayoutEditor.UI.Pages
{
    public class LandingViewModel : Screen
    {
        private readonly ShellViewModel _shellViewModel;

        public LandingViewModel(ShellViewModel shellViewModel)
        {
            _shellViewModel = shellViewModel;
        }

        public async void LoadFromXml()
        {
            await Task.Run(() =>
            {
                var dialog = new OpenFileDialog {CheckFileExists = true, Filter = "Layout Files(*.XML)|*.XML"};
                dialog.ShowDialog();

                var deviceLayout = DeviceLayout.Load(dialog.FileName);
                _shellViewModel.ShowDeviceLayoutEditor(deviceLayout, Path.GetDirectoryName(dialog.FileName));
            });
        }

        public void LoadFromRgbNet()
        {
        }

        public void CreateNew()
        {
        }
    }
}
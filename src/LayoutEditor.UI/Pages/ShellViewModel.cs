using System.Linq;
using RGB.NET.Core.Layout;
using Stylet;

namespace LayoutEditor.UI.Pages
{
    public class ShellViewModel : Conductor<Screen>.Collection.OneActive
    {
        private readonly IWindowManager _windowManager;

        public ShellViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
            Items.Add(new LandingViewModel(this));
            ActiveItem = Items.First();
        }

        public void ShowDeviceLayoutEditor(DeviceLayout deviceLayout, string layoutDirectory)
        {
            var vm = new DeviceLayoutEditorViewModel(deviceLayout, layoutDirectory, _windowManager);
            Items.Add(vm);
            ActiveItem = vm;
        }
    }
}
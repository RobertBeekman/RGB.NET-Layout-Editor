using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using RGB.NET.Core.Layout;
using Stylet;

namespace LayoutEditor.UI.Pages
{
    public class ShellViewModel : Conductor<Screen>.Collection.OneActive
    {
        public ShellViewModel()
        {
            Items.Add(new LandingViewModel(this));
            ActiveItem = Items.First();
        }

        public void ShowDeviceLayoutEditor(DeviceLayout deviceLayout)
        {
            var vm = new DeviceLayoutEditorViewModel(deviceLayout);
            Items.Add(vm);
            ActiveItem = vm;
        }
    }
}
using System.Linq;
using LayoutEditor.UI.Models;
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

        public void ShowDeviceLayoutEditor(LayoutEditModel model)
        {
            var vm = new DeviceLayoutEditorViewModel(model, _windowManager);
            Items.Add(vm);
            ActiveItem = vm;
        }
    }
}
using LayoutEditor.UI.Models;
using Stylet;

namespace LayoutEditor.UI.Pages
{
    public class ShellViewModel : Conductor<Screen>
    {
        private readonly IWindowManager _windowManager;

        public ShellViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
            ActiveItem = new LandingViewModel(this, _windowManager);
        }

        public void Start(LayoutEditModel model)
        {
            ActiveItem = new DeviceLayoutEditorViewModel(model, this, _windowManager);
        }

        public void Reset()
        {
            ActiveItem = new LandingViewModel(this, _windowManager);
        }
    }
}
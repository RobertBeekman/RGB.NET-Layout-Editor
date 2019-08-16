using LayoutEditor.UI.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using RGB.NET.Core.Layout;
using Stylet;

namespace LayoutEditor.UI.Pages
{
    public class LandingViewModel : Screen
    {
        private readonly ShellViewModel _shellViewModel;
        private readonly IWindowManager _windowManager;

        public LandingViewModel(ShellViewModel shellViewModel, IWindowManager windowManager)
        {
            _shellViewModel = shellViewModel;
            _windowManager = windowManager;
        }

        public void LoadFromXml()
        {
            var model = new LayoutEditModel();

            _windowManager.ShowMessageBox("First, select the base folder of the layout. All other paths will be relative to this folder.");

            // Select a base path
            var folderDialog = new CommonOpenFileDialog {IsFolderPicker = true};
            if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
                model.BasePath = folderDialog.FileName;
            else
                return;

            _windowManager.ShowMessageBox("Now select the layout file itself, it should be relative to the base folder.");

            // Select a XML file
            var fileDialog = new CommonOpenFileDialog {InitialDirectory = model.BasePath, Filters = {new CommonFileDialogFilter("Layout Files", "*.xml")}};
            if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                model.DeviceLayout = DeviceLayout.Load(fileDialog.FileName);
                model.DeviceLayoutSource = fileDialog.FileName;
            }
            else
                return;

            _shellViewModel.Start(model);
        }

        public void LoadFromRgbNet()
        {
        }

        public void CreateNew()
        {
        }
    }
}
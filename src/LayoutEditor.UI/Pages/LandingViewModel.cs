using LayoutEditor.UI.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
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

        public void LoadFromXml()
        {
            var model = new LayoutEditModel();

            // Select a base path
            var folderDialog = new CommonOpenFileDialog {InitialDirectory = "C:\\Users", IsFolderPicker = true};
            if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
                model.BasePath = folderDialog.FileName;
            else
                return;

            // Select a XML file
            var fileDialog = new CommonOpenFileDialog {InitialDirectory = model.BasePath, Filters = {new CommonFileDialogFilter("Layout Files", "*.xml")}};
            if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                model.DeviceLayout = DeviceLayout.Load(fileDialog.FileName);
                model.DeviceLayoutSource = fileDialog.FileName;
            }
            else
                return;

            _shellViewModel.ShowDeviceLayoutEditor(model);
        }

        public void LoadFromRgbNet()
        {
        }

        public void CreateNew()
        {
        }
    }
}
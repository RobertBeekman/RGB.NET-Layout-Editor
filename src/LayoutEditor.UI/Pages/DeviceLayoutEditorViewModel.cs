using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using LayoutEditor.UI.Controls;
using LayoutEditor.UI.Dialogs;
using LayoutEditor.UI.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using RGB.NET.Core.Layout;
using Stylet;

namespace LayoutEditor.UI.Pages
{
    public class DeviceLayoutEditorViewModel : Screen
    {
        private readonly ShellViewModel _shellViewModel;
        private readonly IWindowManager _windowManager;

        public DeviceLayoutEditorViewModel(LayoutEditModel model, ShellViewModel shellViewModel, IWindowManager windowManager)
        {
            _shellViewModel = shellViewModel;
            _windowManager = windowManager;

            Model = model;
            DeviceLayout = model.DeviceLayout;
            DeviceLayoutViewModel = new DeviceLayoutViewModel(Model, this, windowManager);

            ImageLayouts = new ObservableCollection<string>();
            foreach (var ledImage in DeviceLayout.LedImageLayouts)
            {
                if (!ImageLayouts.Contains(ledImage.Layout))
                    ImageLayouts.Add(ledImage.Layout);
            }

            SelectedImageLayout = ImageLayouts.FirstOrDefault();

            ImageBasePath = DeviceLayout.ImageBasePath;
            DeviceImage = DeviceLayout.DeviceImage;
        }

        public LayoutEditModel Model { get; set; }
        public DeviceLayout DeviceLayout { get; }
        public DeviceLayoutViewModel DeviceLayoutViewModel { get; set; }

        public ObservableCollection<string> ImageLayouts { get; set; }
        public string SelectedImageLayout { get; set; }

        public string ImageBasePath { get; set; }
        public string DeviceImage { get; set; }
        public string DeviceImagePath => Model.GetAbsoluteImageDirectory(DeviceImage) ?? "None";

        public void SelectImageBasePath()
        {
            var folderDialog = new CommonOpenFileDialog {InitialDirectory = Model.BasePath, IsFolderPicker = true};
            if (folderDialog.ShowDialog() != CommonFileDialogResult.Ok)
                return;

            // Folder must be relative to the base path
            if (!folderDialog.FileName.StartsWith(Model.BasePath))
            {
                _windowManager.ShowMessageBox("Image base path must be relative to " + Model.BasePath);
                return;
            }

            var relativePath = folderDialog.FileName.Substring(Model.BasePath.Length + 1, folderDialog.FileName.Length - Model.BasePath.Length - 1);
            DeviceLayout.ImageBasePath = relativePath;
            ImageBasePath = relativePath;
            NotifyOfPropertyChange(() => DeviceImagePath);
        }

        public void SelectDeviceImage()
        {
            var fileDialog = new CommonOpenFileDialog {InitialDirectory = Path.Combine(Model.BasePath, ImageBasePath), Filters = {new CommonFileDialogFilter("Image Files", "*.png")}};
            if (fileDialog.ShowDialog() != CommonFileDialogResult.Ok)
                return;

            // Folder must be relative to the image base path
            var imageBasePath = Path.Combine(Model.BasePath, ImageBasePath);
            if (!fileDialog.FileName.StartsWith(imageBasePath))
            {
                _windowManager.ShowMessageBox("Image path must be relative to " + imageBasePath);
                return;
            }

            var relativePath = fileDialog.FileName.Substring(imageBasePath.Length + 1, fileDialog.FileName.Length - imageBasePath.Length - 1);
            DeviceLayout.DeviceImage = relativePath;
            DeviceImage = relativePath;
            NotifyOfPropertyChange(() => DeviceImagePath);
        }

        public void AddImageLayout()
        {
            _windowManager.ShowDialog(new AddImageLayoutViewModel(_windowManager, this));
        }

        public void Reset()
        {
            _shellViewModel.Reset();
        }

        public void Save()
        {
            var fileDialog = new CommonSaveFileDialog {InitialDirectory = Model.BasePath, Filters = {new CommonFileDialogFilter("Layout File", "*.xml")}};
            if (fileDialog.ShowDialog() != CommonFileDialogResult.Ok)
                return;

            var writer = new XmlSerializer(typeof(DeviceLayout));
            var file = File.Create(fileDialog.FileName);
            writer.Serialize(file, DeviceLayout);
            file.Close();
        }
    }
}
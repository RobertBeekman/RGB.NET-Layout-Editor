using System.Windows;
using System.Xml;
using LayoutEditor.UI.Models;
using LayoutEditor.UI.RGB.NET;
using Ookii.Dialogs.Wpf;
using RGB.NET.Layout;
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

            // Select a XML file
            VistaOpenFileDialog fileDialog = new();
            fileDialog.Filter = "Layout files (*.xml)|*.xml";
            if (fileDialog.ShowDialog() == false)
                return;

            ConvertLayoutFile(fileDialog.FileName);
            model.DeviceLayout = DeviceLayout.Load(fileDialog.FileName, typeof(LayoutCustomDeviceData), typeof(LayoutCustomLedData));
            model.FilePath = fileDialog.FileName;

            if (model.DeviceLayout == null)
            {
                _windowManager.ShowMessageBox("Failed to load layout.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _shellViewModel.Start(model);
        }

        private void ConvertLayoutFile(string layoutFilePath)
        {
            var doc = new XmlDocument();
            doc.Load(layoutFilePath);

            var device = doc["Device"];
            if (device == null)
                return;

            // Remove device image
            if (device["DeviceImage"] != null)
                device.RemoveChild(device["DeviceImage"]);
            if (device["ImageBasePath"] != null)
                device.RemoveChild(device["ImageBasePath"]);

            // Create new device image node based on model
            if (device["CustomData"] == null && device["Model"] != null)
            {
                var deviceCustomData = doc.CreateElement("CustomData");
                var deviceImage = doc.CreateElement("DeviceImage");
                deviceImage.InnerText = device["Model"].InnerText + ".png";
                deviceCustomData.AppendChild(deviceImage);
            }

            // Determine logical layouts
            if (device["LedImageLayouts"] != null)
                foreach (XmlNode ledImageLayout in device["LedImageLayouts"].ChildNodes)
                {
                    if (ledImageLayout["LedImages"] == null)
                        continue;

                    foreach (XmlNode ledImage in ledImageLayout["LedImages"].ChildNodes)
                    {
                    }
                }

            // Move each logical layout to its LEDs
        }

        public void CreateNew()
        {
            var model = new LayoutEditModel();

            _windowManager.ShowMessageBox(
                "Select the base folder of the layout. All other paths will be relative to this folder.");

            // Select a base path
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog() != true)
                return;

            model.FilePath = dialog.SelectedPath;
            model.DeviceLayout = new DeviceLayout();

            _shellViewModel.Start(model);
        }
    }
}
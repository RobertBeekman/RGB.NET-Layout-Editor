using System.IO;
using System.Windows;
using System.Xml;
using LayoutEditor.UI.Layout;
using LayoutEditor.UI.Models;
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

            model.DeviceLayout = DeviceLayout.Load(fileDialog.FileName, typeof(LayoutCustomDeviceData), typeof(LayoutCustomLedData));
            model.FilePath = fileDialog.FileName;

            if (model.DeviceLayout == null)
            {
                _windowManager.ShowMessageBox("Failed to load layout.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _shellViewModel.Start(model);
        }

        public void CreateNew()
        {
            var model = new LayoutEditModel();

            _windowManager.ShowMessageBox("Select the base folder of the layout. All other paths will be relative to this folder.");

            // Select a base path
            var dialog = new VistaFolderBrowserDialog();
            if (dialog.ShowDialog() != true)
                return;

            model.FilePath = Path.Combine(dialog.SelectedPath, "New layout.xml");
            model.DeviceLayout = new DeviceLayout {CustomData = new LayoutCustomDeviceData()};

            _shellViewModel.Start(model);
        }
    }
}
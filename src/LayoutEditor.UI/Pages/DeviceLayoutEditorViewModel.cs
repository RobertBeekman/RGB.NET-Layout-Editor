using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using LayoutEditor.UI.Dialogs;
using Microsoft.Win32;
using RGB.NET.Core.Layout;
using Stylet;

namespace LayoutEditor.UI.Pages
{
    public class DeviceLayoutEditorViewModel : Screen
    {
        private readonly string _layoutDirectory;
        private readonly IWindowManager _windowManager;

        public DeviceLayoutEditorViewModel(DeviceLayout deviceLayout, string layoutDirectory, IWindowManager windowManager)
        {
            _layoutDirectory = layoutDirectory;
            _windowManager = windowManager;

            DeviceLayout = deviceLayout;
            ImageLayouts = new ObservableCollection<string>();

            foreach (var ledImage in DeviceLayout.LedImageLayouts)
            {
                if (!ImageLayouts.Contains(ledImage.Layout))
                    ImageLayouts.Add(ledImage.Layout);
            }

            SelectedImageLayout = ImageLayouts.FirstOrDefault();
            DeviceImagePath = Path.Combine(_layoutDirectory, DeviceLayout.DeviceImage);
        }

        public DeviceLayout DeviceLayout { get; }
        public ObservableCollection<string> ImageLayouts { get; set; }
        public string SelectedImageLayout { get; set; }
        public string DeviceImagePath { get; set; }

        public void AddImageLayout()
        {
            _windowManager.ShowDialog(new AddImageLayoutViewModel(_windowManager, this));
        }

        public void SelectDeviceImage()
        {
            var dialog = new OpenFileDialog {CheckFileExists = true, Filter = "Image files(*.PNG)|*.PNG"};
            dialog.ShowDialog();
            DeviceImagePath = dialog.FileName;
        }
        
        public void UpdateLedPositions()
        {
            if (DeviceLayout.Leds == null)
                return;

            LedLayout lastLed = null;
            foreach (var led in DeviceLayout.Leds)
            {
                led.CalculateValues(DeviceLayout, lastLed);
                lastLed = led;
            }
        }

        public void Save()
        {
            // Set the base path for images
            DeviceLayout.ImageBasePath = Path.Combine("Images", DeviceLayout.Vendor, DeviceLayout.Type + "s");

            var dialog = new SaveFileDialog {Filter = "Layout Files(*.XML)|*.XML"};
            dialog.ShowDialog();

            var writer = new XmlSerializer(typeof(DeviceLayout));
            var file = File.Create(dialog.FileName);
            writer.Serialize(file, DeviceLayout);
            file.Close();
        }
    }
}
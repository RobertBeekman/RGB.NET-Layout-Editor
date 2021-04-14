using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;
using LayoutEditor.UI.Controls;
using LayoutEditor.UI.Dialogs;
using LayoutEditor.UI.Layout;
using LayoutEditor.UI.Models;
using Ookii.Dialogs.Wpf;
using RGB.NET.Core;
using RGB.NET.Layout;
using Stylet;

namespace LayoutEditor.UI.Pages
{
    public class DeviceLayoutEditorViewModel : Screen
    {
        private readonly ShellViewModel _shellViewModel;
        private readonly IWindowManager _windowManager;
        private FileSystemWatcher _fileWatcher;

        public DeviceLayoutEditorViewModel(LayoutEditModel model, ShellViewModel shellViewModel, IWindowManager windowManager)
        {
            _shellViewModel = shellViewModel;
            _windowManager = windowManager;

            Model = model;
            DeviceLayout = model.DeviceLayout;
            DeviceLayoutViewModel = new DeviceLayoutViewModel(Model, this, windowManager);
            DeviceLayoutViewModel.ConductWith(this);

            LogicalLayouts = new ObservableCollection<string> {"Empty"};
            foreach (var led in DeviceLayout.Leds)
            {
                if (led.CustomData == null)
                    continue;

                var customLedData = (LayoutCustomLedData) led.CustomData;
                foreach (var ledDataLogicalLayout in customLedData.LogicalLayouts)
                    if (ledDataLogicalLayout.Name != null && !LogicalLayouts.Contains(ledDataLogicalLayout.Name))
                        LogicalLayouts.Add(ledDataLogicalLayout.Name);
            }

            SelectedLogicalLayout = LogicalLayouts.FirstOrDefault();

            // Lame but works
            if (Model.FilePath.EndsWith("ABNT.xml"))
                Model.PhysicalLayout = KeyboardLayoutType.ABNT;
            else if (Model.FilePath.EndsWith("ANSI.xml"))
                Model.PhysicalLayout = KeyboardLayoutType.ANSI;
            else if (Model.FilePath.EndsWith("ISO.xml"))
                Model.PhysicalLayout = KeyboardLayoutType.ISO;
            else if (Model.FilePath.EndsWith("JIS.xml"))
                Model.PhysicalLayout = KeyboardLayoutType.JIS;
            else if (Model.FilePath.EndsWith("KS.xml"))
                Model.PhysicalLayout = KeyboardLayoutType.KS;

            UpdateDeviceImage();
        }

        public LayoutEditModel Model { get; set; }
        public DeviceLayout DeviceLayout { get; }
        public LayoutCustomDeviceData LayoutCustomDeviceData => (LayoutCustomDeviceData) DeviceLayout.CustomData;
        public DeviceLayoutViewModel DeviceLayoutViewModel { get; set; }

        public ObservableCollection<string> LogicalLayouts { get; set; }
        public string SelectedLogicalLayout { get; set; }
        public string LedSubfolder { get; set; }

        public string InputImage { get; set; }

        public object DeviceImage
        {
            get
            {
                var fileUri = new Uri(new Uri(Model.FilePath), LayoutCustomDeviceData.DeviceImage);
                if (!File.Exists(fileUri.LocalPath))
                    return DependencyProperty.UnsetValue;

                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                image.UriSource = fileUri;
                image.EndInit();
                return image;
            }
        }

        public void SelectDeviceImage()
        {
            VistaOpenFileDialog dialog = new();
            dialog.Filter = "Image files (*.png)|*.png";
            dialog.InitialDirectory = Path.GetDirectoryName(Model.FilePath);
            if (dialog.ShowDialog() == false)
                return;

            var relativePath = new Uri(Path.GetDirectoryName(Model.FilePath) + "/").MakeRelativeUri(new Uri(dialog.FileName));
            LayoutCustomDeviceData.DeviceImage = HttpUtility.UrlDecode(relativePath.OriginalString);
            UpdateDeviceImage();
        }

        public void AddImageLayout()
        {
            _windowManager.ShowDialog(new AddImageLayoutViewModel(_windowManager, this));
        }

        public void Reset()
        {
            _shellViewModel.Reset();
        }

        public async Task Save()
        {
            _windowManager.ShowMessageBox("Select a target directory in which to save your layout, a directory structure and XML file will be created automatically");

            var userLayoutsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Artemis", "user layouts");
            if (!Directory.Exists(userLayoutsPath))
                Directory.CreateDirectory(userLayoutsPath);

            VistaFolderBrowserDialog dialog = new()
            {
                Description = "Select layout export target folder",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true,
                SelectedPath = userLayoutsPath
            };

            var result = dialog.ShowDialog();
            if (result != true)
                return;

            var directory = Path.Combine(
                dialog.SelectedPath,
                DeviceLayout.Vendor,
                DeviceLayout.Type.ToString()
            );
            var filePath = Path.Combine(directory, GetLayoutFileName());
            Directory.CreateDirectory(directory);

            // Create a copy of the layout, image paths are about to be rewritten
            XmlSerializer serializer = new(typeof(DeviceLayout));
            using MemoryStream ms = new();
            using StreamWriter writer = new(ms);
            serializer.Serialize(writer, DeviceLayout);
            writer.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            var doc = new XmlDocument();
            doc.Load(ms);

            Uri sourceDirectory = new(Path.GetDirectoryName(Model.FilePath)! + "/", UriKind.Absolute);
            Uri targetDirectory = new(directory + "/", UriKind.Absolute);

            // Create folder (if needed) and copy image
            if (LayoutCustomDeviceData.DeviceImage != null)
            {
                Uri sourceDeviceImage = new(sourceDirectory, LayoutCustomDeviceData.DeviceImage);
                Uri targetDeviceImage = new(targetDirectory, Path.GetFileName(LayoutCustomDeviceData.DeviceImage));
                if (File.Exists(sourceDeviceImage.LocalPath) && !File.Exists(targetDeviceImage.LocalPath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(targetDeviceImage.LocalPath)!);
                    File.Copy(sourceDeviceImage.LocalPath, targetDeviceImage.LocalPath);
                }

                if (doc["Device"]["CustomData"] == null)
                    doc["Device"].AppendChild(doc.CreateElement("CustomData"));
                if (doc["Device"]["CustomData"]["DeviceImage"] == null)
                    doc["Device"]["CustomData"].AppendChild(doc.CreateElement("DeviceImage"));
                doc["Device"]["CustomData"]["DeviceImage"].InnerText = Path.GetFileName(LayoutCustomDeviceData.DeviceImage);
            }

            Uri targetLedDirectory = new(directory + "/", UriKind.Absolute);
            if (!string.IsNullOrWhiteSpace(LedSubfolder))
                targetLedDirectory = new Uri(targetLedDirectory, LedSubfolder.TrimEnd('/').TrimEnd('\\') + "/");

            foreach (var deviceLayoutLed in DeviceLayout.Leds)
            {
                var layoutCustomLedData = (LayoutCustomLedData) deviceLayoutLed.CustomData;
                if (layoutCustomLedData?.LogicalLayouts == null || layoutCustomLedData.LogicalLayouts.All(l => string.IsNullOrWhiteSpace(l.Image)))
                    continue;

                var led = doc["Device"]["Leds"].ChildNodes.Cast<XmlNode>().FirstOrDefault(l => l.Attributes != null && l.Attributes["Id"].Value == deviceLayoutLed.Id);
                if (led == null)
                    continue;

                // Only the image of the current logical layout is available as an URI, iterate each layout and find the images manually
                foreach (var logicalLayout in layoutCustomLedData.LogicalLayouts)
                {
                    if (led["CustomData"] == null)
                        led.AppendChild(doc.CreateElement("CustomData"));
                    if (led["CustomData"]["LogicalLayouts"] == null)
                        led["CustomData"].AppendChild(doc.CreateElement("LogicalLayouts"));

                    var layout = led["CustomData"]["LogicalLayouts"]
                        .ChildNodes
                        .Cast<XmlNode>()
                        .FirstOrDefault(l => l.Attributes != null &&
                                             (
                                                 l.Attributes["Name"] == null &&
                                                 logicalLayout.Name == null ||
                                                 l.Attributes["Name"] != null && l.Attributes["Name"].Value == logicalLayout.Name
                                             ));
                    if (layout == null)
                    {
                        layout = doc.CreateElement("LogicalLayout");
                        led["CustomData"]["LogicalLayouts"].AppendChild(layout);
                    }

                    Uri sourceLedImage = new(sourceDirectory, logicalLayout.Image);
                    Uri targetLedImage = new(targetLedDirectory, Path.GetFileName(logicalLayout.Image));

                    // Create folder (if needed) and copy image
                    if (File.Exists(sourceLedImage.LocalPath) && !File.Exists(targetLedImage.LocalPath))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(targetLedImage.LocalPath)!);
                        File.Copy(sourceLedImage.LocalPath, targetLedImage.LocalPath);
                    }

                    if (layout.Attributes["Image"] == null)
                        layout.Attributes.Append(doc.CreateAttribute("Image"));
                    layout.Attributes["Image"].Value = HttpUtility.UrlDecode(targetDirectory.MakeRelativeUri(targetLedImage).OriginalString);
                    if (logicalLayout.Name != null && logicalLayout.Name != "Empty")
                    {
                        if (layout.Attributes["Name"] == null)
                            layout.Attributes.Append(doc.CreateAttribute("Name"));
                        layout.Attributes["Name"].Value = logicalLayout.Name;
                    }
                }
            }

            doc.Save(filePath);

            _windowManager.ShowMessageBox("Exported file, press OK to view the directory");
            Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", directory);
        }

        /// <summary>
        ///     Generates the default layout file name of the device
        /// </summary>
        /// <param name="includeExtension">If true, the .xml extension is added to the file name</param>
        /// <returns>The resulting file name e.g. CORSAIR GLAIVE.xml or K95 RGB-ISO.xml</returns>
        public string GetLayoutFileName(bool includeExtension = true)
        {
            // Take out invalid file name chars, may not be perfect but neither are you
            var fileName = DeviceLayout.Model != null
                ? Path.GetInvalidFileNameChars().Aggregate(DeviceLayout.Model, (current, c) => current.Replace(c, '-'))
                : "Unknown";
            if (DeviceLayout.Type == RGBDeviceType.Keyboard)
                fileName = $"{fileName}-{Model.PhysicalLayout.ToString().ToUpper()}";
            if (includeExtension)
                fileName = $"{fileName}.xml";

            return fileName;
        }

        protected override void OnClose()
        {
            if (_fileWatcher != null)
                _fileWatcher.Changed -= FileWatcherOnChanged;

            base.OnClose();
        }

        private void UpdateDeviceImage()
        {
            InputImage = Path.GetFileName(LayoutCustomDeviceData.DeviceImage);
            NotifyOfPropertyChange(nameof(DeviceImage));

            var filePath = new Uri(new Uri(Model.FilePath), LayoutCustomDeviceData.DeviceImage).LocalPath;
            if (_fileWatcher != null)
            {
                _fileWatcher.Changed -= FileWatcherOnChanged;
                _fileWatcher = null;
            }

            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                return;
            _fileWatcher = new FileSystemWatcher(Path.GetDirectoryName(filePath)!, Path.GetFileName(filePath)!)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.Size,
                EnableRaisingEvents = true
            };
            _fileWatcher.Changed += FileWatcherOnChanged;
        }

        private void FileWatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            NotifyOfPropertyChange(nameof(DeviceImage));
        }
    }
}
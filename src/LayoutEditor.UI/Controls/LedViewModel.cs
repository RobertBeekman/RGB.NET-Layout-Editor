using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LayoutEditor.UI.Editors;
using LayoutEditor.UI.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using RGB.NET.Core;
using RGB.NET.Core.Layout;
using Stylet;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;

namespace LayoutEditor.UI.Controls
{
    public class LedViewModel : PropertyChangedBase
    {
        public static Color SelectedColor = Color.FromRgb(237, 65, 131);
        public static Color HoverColor = Color.FromRgb(116, 97, 167);
        public static Color UnselectedColor = Color.FromRgb(62, 180, 203);

        private readonly DeviceLayoutViewModel _layoutViewModel;
        private readonly IWindowManager _windowManager;
        private LedImage _ledImage;

        public LedViewModel(LayoutEditModel model, DeviceLayoutViewModel layoutViewModel, IWindowManager windowManager, LedLayout ledLayout)
        {
            _layoutViewModel = layoutViewModel;
            _windowManager = windowManager;

            Model = model;
            LedLayout = ledLayout;
            AvailableLedIds = new BindableCollection<string>();
            LedCursor = Cursors.Hand;

            PropertyChanged += LedImagePathChanged;
            FileChangedWatcher.FileChanged += FileChangedWatcherOnFileChanged;

            UpdateImageSource();
        }

        public LayoutEditModel Model { get; }
        public LedLayout LedLayout { get; }
        public BindableCollection<string> AvailableLedIds { get; set; }

        private MemoryStream _imageStream;
        public ImageSource LedImageSource { get; set; }

        public string LedImagePath => Model.GetAbsoluteImageDirectory(_ledImage?.Image);
        public bool Selected { get; set; }

        public string InputId { get; set; }
        public Shape InputShape { get; set; }
        public string InputShapeData { get; set; }
        public string InputImage { get; set; }
        public string InputX { get; set; }
        public string InputY { get; set; }
        public string InputWidth { get; set; }
        public string InputHeight { get; set; }

        public Cursor LedCursor { get; set; }
        public ShapeEditor ShapeEditor { get; set; }
        public bool IsEditingShape => ShapeEditor != null;
        public int ZIndex => ShapeEditor != null ? 2 : 1;
        public bool CanStartShapeEdit => InputShape == Shape.Custom;

        public Geometry DisplayGeometry { get; set; }
        public SolidColorBrush FillBrush { get; set; }
        public SolidColorBrush BorderBrush { get; set; }

        public void ApplyInput()
        {
            // If the ID changed the image layouts must change as well
            if (!LedLayout.Id.Equals(InputId))
                foreach (var imageLayout in Model.DeviceLayout.LedImageLayouts)
                {
                    foreach (var ledImage in imageLayout.LedImages.Where(l => l.Id.Equals(LedLayout.Id)))
                        ledImage.Id = InputId;
                }

            LedLayout.Id = InputId;
            LedLayout.DescriptiveX = InputX;
            LedLayout.DescriptiveY = InputY;
            LedLayout.DescriptiveWidth = InputWidth;
            LedLayout.DescriptiveHeight = InputHeight;

            // Apply custom shape data
            if (InputShape == Shape.Custom)
                LedLayout.DescriptiveShape = InputShapeData;
            else
                LedLayout.DescriptiveShape = InputShape.ToString();

            // If LED image exists, update it
            if (_ledImage != null)
            {
                _ledImage.Image = InputImage;
                NotifyOfPropertyChange(() => LedImagePath);
            }
            // Create a new LED image and add it to the layout
            else
            {
                var ledImage = new LedImage {Id = LedLayout.Id, Image = InputImage};
                // Find the current layout
                var layout = Model.DeviceLayout.LedImageLayouts.FirstOrDefault(l => l.Layout != null && l.Layout.Equals(_layoutViewModel.EditorViewModel.SelectedImageLayout));
                // If missing, create it
                if (layout == null)
                {
                    layout = new LedImageLayout {Layout = _layoutViewModel.EditorViewModel.SelectedImageLayout};
                    Model.DeviceLayout.LedImageLayouts.Add(layout);
                }

                layout.LedImages.Add(ledImage);
                UpdateLedImage(ledImage);
            }

            _layoutViewModel.UpdateLeds();
        }

        public void Update()
        {
            UpdateAvailableLedIds();
            PopulateInput();
            CreateLedGeometry();
        }

        public void UpdateLedImage(LedImage ledImage)
        {
            _ledImage = ledImage;
            InputImage = _ledImage?.Image;
            NotifyOfPropertyChange(() => LedImagePath);
        }

        public void SelectImage()
        {
            var fileDialog = new CommonOpenFileDialog
                {InitialDirectory = Path.Combine(Model.BasePath, Model.DeviceLayout.ImageBasePath), Filters = {new CommonFileDialogFilter("Image Files", "*.png")}};
            if (fileDialog.ShowDialog() != CommonFileDialogResult.Ok)
                return;

            // Folder must be relative to the image base path
            var imageBasePath = Path.Combine(Model.BasePath, Model.DeviceLayout.ImageBasePath);
            if (!fileDialog.FileName.StartsWith(imageBasePath))
            {
                _windowManager.ShowMessageBox("Image path must be relative to " + imageBasePath);
                return;
            }

            var relativePath = fileDialog.FileName.Substring(imageBasePath.Length + 1, fileDialog.FileName.Length - imageBasePath.Length - 1);
            InputImage = relativePath;
        }

        private void UpdateAvailableLedIds()
        {
            AvailableLedIds.Clear();
            AvailableLedIds.AddRange(Model.GetAvailableLedIds(LedLayout.Id));
        }

        private void PopulateInput()
        {
            InputId = AvailableLedIds.First(l => l.Equals(LedLayout.Id));
            InputShape = LedLayout.Shape;
            InputShapeData = LedLayout.ShapeData;
            InputX = LedLayout.DescriptiveX;
            InputY = LedLayout.DescriptiveY;
            InputWidth = LedLayout.DescriptiveWidth;
            InputHeight = LedLayout.DescriptiveHeight;
        }

        private void CreateLedGeometry()
        {
            var geometryRectangle = new Rect(0, 0, 1, 1);
            Geometry geometry;

            if (ShapeEditor != null)
            {
                try
                {
                    geometry = Geometry.Combine(Geometry.Parse(LedLayout.ShapeData), ShapeEditor.GetGeometry(true), GeometryCombineMode.Xor, null);
                }
                catch (Exception)
                {
                    geometry = ShapeEditor.GetGeometry(true);
                }
            }
            else
                switch (LedLayout.Shape)
                {
                    case Shape.Custom:
                        try
                        {
                            geometry = Geometry.Parse(LedLayout.ShapeData);
                        }
                        catch (Exception e)
                        {
                            geometry = new RectangleGeometry(geometryRectangle);
                            _windowManager.ShowMessageBox("Failed to parse shape data, showing a rectangle instead.\n\n " + e.Message, InputId);
                        }

                        break;
                    case Shape.Rectangle:
                        geometry = new RectangleGeometry(geometryRectangle);
                        break;
                    case Shape.Circle:
                        geometry = new EllipseGeometry(geometryRectangle);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            DisplayGeometry = Geometry.Combine(Geometry.Empty, geometry, GeometryCombineMode.Union, new ScaleTransform(LedLayout.Width, LedLayout.Height));
            SetColor(Selected ? SelectedColor : UnselectedColor);

            NotifyOfPropertyChange(() => LedLayout);
        }

        public void SetColor(Color borderColor, Color? fillColor = null)
        {
            if (fillColor == null)
                fillColor = Color.FromArgb(64, borderColor.R, borderColor.G, borderColor.B);

            BorderBrush = new SolidColorBrush(borderColor);
            FillBrush = new SolidColorBrush(fillColor.Value);
        }

        public void StartShapeEdit()
        {
            LedCursor = Cursors.Pen;
            ShapeEditor = new ShapeEditor();
            CreateLedGeometry();
            NotifyOfPropertyChange(() => IsEditingShape);
            NotifyOfPropertyChange(() => ZIndex);

            ApplyInput();
        }

        public void StopShapeEdit()
        {
            var geometry = ShapeEditor.GetGeometry(false);
            if (geometry is PathGeometry)
            {
                InputShapeData = InputShapeData + " " + geometry
                    .ToString(CultureInfo.InvariantCulture)
                    .Replace(";", ",")
                    .Replace("L", " L");
            }

            LedCursor = Cursors.Hand;
            ShapeEditor = null;
            CreateLedGeometry();
            NotifyOfPropertyChange(() => IsEditingShape);
            NotifyOfPropertyChange(() => ZIndex);

            ApplyInput();
        }

        private Point GetPercentagePosition(Point position)
        {
            return new Point(Math.Round(position.X / LedLayout.Width, 3), Math.Round(position.Y / LedLayout.Height, 3));
        }

        private void LedImagePathChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LedImagePath))
                UpdateImageSource();
        }

        private void FileChangedWatcherOnFileChanged(object sender, string file)
        {
            if (Path.GetFileName(file) == Path.GetFileName(LedImagePath))
                Execute.PostToUIThread(UpdateImageSource);
        }

        private void UpdateImageSource()
        {
            _imageStream?.Dispose();

            string filePath = LedImagePath;
            if (filePath != null && File.Exists(filePath))
            {
                _imageStream = new MemoryStream(File.ReadAllBytes(filePath));
                var bitmap = new BitmapImage();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.BeginInit();
                bitmap.StreamSource = _imageStream;
                bitmap.EndInit();

                LedImageSource = bitmap;
            }
            else
            {
                LedImageSource = null;
                _imageStream = null;
            }
        }

        #region Event handlers

        public void MouseUp(object sender, MouseEventArgs e)
        {
            _layoutViewModel.SelectLed(this);

            if (ShapeEditor != null)
            {
                var percentagePosition = GetPercentagePosition(e.GetPosition((IInputElement) sender));
                ShapeEditor.Click(percentagePosition);
                CreateLedGeometry();
            }
        }

        public void MouseMove(object sender, MouseEventArgs e)
        {
            if (ShapeEditor != null)
            {
                var percentagePosition = GetPercentagePosition(e.GetPosition((IInputElement) sender));
                ShapeEditor.Move(percentagePosition);
                CreateLedGeometry();
            }
        }

        public void MouseEnter()
        {
            if (!Selected)
                SetColor(HoverColor);
        }

        public void MouseLeave()
        {
            if (!Selected)
                SetColor(UnselectedColor);
        }

        #endregion
    }
}
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using LayoutEditor.UI.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using RGB.NET.Core;
using RGB.NET.Core.Layout;
using Stylet;
using Color = System.Windows.Media.Color;

namespace LayoutEditor.UI.Controls
{
    public class LedViewModel : PropertyChangedBase
    {
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
        }

        public LayoutEditModel Model { get; }
        public LedLayout LedLayout { get; }
        public BindableCollection<string> AvailableLedIds { get; set; }

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

        public DrawingImage DisplayDrawing { get; set; }

        public void ApplyInput()
        {
            // If the ID changed the image layouts must change as well
            if (!LedLayout.Id.Equals(InputId))
            {
                foreach (var imageLayout in Model.DeviceLayout.LedImageLayouts)
                {
                    foreach (var ledImage in imageLayout.LedImages.Where(l => l.Id.Equals(LedLayout.Id)))
                        ledImage.Id = InputId;
                }
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
                var layout = Model.DeviceLayout.LedImageLayouts.FirstOrDefault(l => l.Layout.Equals(_layoutViewModel.EditorViewModel.SelectedImageLayout));
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
            if (InputImage != _ledImage?.Image)
            {
                InputImage = _ledImage?.Image;
                NotifyOfPropertyChange(() => LedImagePath);
            }
        }

        public void SelectImage()
        {
            var fileDialog = new CommonOpenFileDialog {InitialDirectory = Path.Combine(Model.BasePath, Model.DeviceLayout.ImageBasePath), Filters = {new CommonFileDialogFilter("Image Files", "*.png")}};
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
            var relativeRectangle = new Rect(0, 0, LedLayout.Width, LedLayout.Height);
            var scale = 1.0;
            Geometry geometry;
            switch (LedLayout.Shape)
            {
                case Shape.Custom:
                    try
                    {
                        geometry = Geometry.Parse(LedLayout.ShapeData);
                        // Get the scale in comparison to the relative rect, this determines the thickness of the stroke
                        scale = geometry.Bounds.Width / relativeRectangle.Width;
                    }
                    catch (Exception e)
                    {
                        geometry = new RectangleGeometry(relativeRectangle);
                        _windowManager.ShowMessageBox("Failed to parse shape data, showing a rectangle instead.\n\n " + e.Message, InputId);
                    }

                    break;
                case Shape.Rectangle:
                    geometry = new RectangleGeometry(relativeRectangle);
                    break;
                case Shape.Circle:
                    geometry = new EllipseGeometry(relativeRectangle);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var drawing = new GeometryDrawing(null, new Pen(null, 1), geometry);

            // Apply the previously determined scale
            if (LedLayout.Shape == Shape.Custom)
                drawing.Pen.Thickness = scale;

            DisplayDrawing = new DrawingImage(drawing);
            ChangeColor(Selected ? Colors.Yellow : Colors.Red);
            NotifyOfPropertyChange(() => LedLayout);
        }

        #region Event handlers

        public void MouseUp()
        {
            _layoutViewModel.SelectLed(this);
        }

        public void MouseEnter()
        {
            if (!Selected)
                ChangeColor(Colors.Orange);
        }

        public void MouseLeave()
        {
            if (!Selected)
                ChangeColor(Colors.Red);
        }

        public void ChangeColor(Color color)
        {
            if (DisplayDrawing.Drawing is GeometryDrawing geometryDrawing)
            {
                geometryDrawing.Brush = new SolidColorBrush(color) {Opacity = 0.3};
                geometryDrawing.Pen.Brush = new SolidColorBrush(color);

                NotifyOfPropertyChange(() => DisplayDrawing);
            }
        }

        #endregion
    }
}
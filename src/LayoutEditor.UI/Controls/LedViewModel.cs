using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LayoutEditor.UI.Editors;
using LayoutEditor.UI.Layout;
using LayoutEditor.UI.Models;
using Ookii.Dialogs.Wpf;
using RGB.NET.Core;
using RGB.NET.Layout;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using Stylet;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace LayoutEditor.UI.Controls
{
    public class LedViewModel : Screen
    {
        public static Color SelectedColor = Color.FromRgb(237, 65, 131);
        public static Color HoverColor = Color.FromRgb(116, 97, 167);
        public static Color UnselectedColor = Color.FromRgb(62, 180, 203);

        private readonly DeviceLayoutViewModel _layoutViewModel;
        private readonly IWindowManager _windowManager;
        private LayoutCustomLedDataLogicalLayout _logicalLayout;
        private FileSystemWatcher _fileWatcher;

        public LedViewModel(LayoutEditModel model, DeviceLayoutViewModel layoutViewModel, IWindowManager windowManager, LedLayout ledLayout)
        {
            _layoutViewModel = layoutViewModel;
            _windowManager = windowManager;

            Model = model;
            LedLayout = ledLayout;
            AvailableLedIds = new BindableCollection<string>();
            LedCursor = Cursors.Hand;

            ApplyLogicalLayout();
        }

        public LayoutEditModel Model { get; }
        public LedLayout LedLayout { get; }
        public LayoutCustomLedData LayoutCustomLedData => (LayoutCustomLedData) LedLayout.CustomData;
        public BindableCollection<string> AvailableLedIds { get; set; }

        public bool Selected { get; set; }

        public string InputId { get; set; }
        public Shape InputShape { get; set; }
        public string InputShapeData { get; set; }
        public string InputX { get; set; }
        public string InputY { get; set; }
        public string InputWidth { get; set; }
        public string InputHeight { get; set; }

        public Cursor LedCursor { get; set; }
        public ShapeEditor ShapeEditor { get; set; }
        public bool IsEditingShape => ShapeEditor != null;
        public int ZIndex => ShapeEditor != null ? 2 : 1;
        public bool CanStartShapeEdit => InputShape == Shape.Custom;

        public string InputImage { get; set; }

        public object LedImage
        {
            get
            {
                if (_logicalLayout.Image == null)
                    return DependencyProperty.UnsetValue;
                var fileUri = new Uri(new Uri(Model.FilePath), _logicalLayout.Image);
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

        public Geometry DisplayGeometry { get; set; }
        public SolidColorBrush FillBrush { get; set; }
        public SolidColorBrush BorderBrush { get; set; }

        private void ApplyLogicalLayout()
        {
            if (_layoutViewModel.EditorViewModel.SelectedLogicalLayout == "Empty")
                _logicalLayout = LayoutCustomLedData.LogicalLayouts.FirstOrDefault();
            else
                _logicalLayout = LayoutCustomLedData.LogicalLayouts.FirstOrDefault(l => l.Name == _layoutViewModel.EditorViewModel.SelectedLogicalLayout);
            
            if (_logicalLayout == null)
            {
                _logicalLayout = new LayoutCustomLedDataLogicalLayout();
                _logicalLayout.Name = _layoutViewModel.EditorViewModel.SelectedLogicalLayout == "Empty"
                    ? null
                    : _layoutViewModel.EditorViewModel.SelectedLogicalLayout;
                LayoutCustomLedData.LogicalLayouts.Add(_logicalLayout);
            }

            InputImage = Path.GetFileName(_logicalLayout.Image);
            NotifyOfPropertyChange(nameof(LedImage));

            var filePath = new Uri(new Uri(Model.FilePath), _logicalLayout.Image).LocalPath;
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
            NotifyOfPropertyChange(nameof(LedImage));
        }

        public void ApplyInput()
        {
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
            _layoutViewModel.UpdateLeds();
        }

        public void Update()
        {
            ApplyLogicalLayout();
            UpdateAvailableLedIds();
            PopulateInput();
            CreateLedGeometry();
        }

        public void SelectImage()
        {
            VistaOpenFileDialog dialog = new();
            dialog.Filter = "Image files (*.png)|*.png";
            dialog.InitialDirectory = Path.GetDirectoryName(Model.FilePath);
            if (dialog.ShowDialog() == false)
                return;

            var relativePath =
                new Uri(Path.GetDirectoryName(Model.FilePath) + "/").MakeRelativeUri(new Uri(dialog.FileName));
            _logicalLayout.Image = HttpUtility.UrlDecode(relativePath.OriginalString);
            InputImage = Path.GetFileName(_logicalLayout.Image);
            NotifyOfPropertyChange(nameof(LedImage));
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
                try
                {
                    geometry = Geometry.Combine(Geometry.Parse(LedLayout.ShapeData), ShapeEditor.GetGeometry(true),
                        GeometryCombineMode.Xor, null);
                }
                catch (Exception)
                {
                    geometry = ShapeEditor.GetGeometry(true);
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
                            _windowManager.ShowMessageBox(
                                "Failed to parse shape data, showing a rectangle instead.\n\n " + e.Message, InputId);
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

            DisplayGeometry = Geometry.Combine(Geometry.Empty, geometry, GeometryCombineMode.Union,
                new ScaleTransform(LedLayout.Width, LedLayout.Height));
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
                InputShapeData = InputShapeData + " " + geometry
                    .ToString(CultureInfo.InvariantCulture)
                    .Replace(";", ",")
                    .Replace("L", " L");

            LedCursor = Cursors.Hand;
            ShapeEditor = null;
            CreateLedGeometry();
            NotifyOfPropertyChange(() => IsEditingShape);
            NotifyOfPropertyChange(() => ZIndex);

            ApplyInput();
        }

        public void ImportSvg()
        {
            // Select a XML file
            string fileName = null;

            VistaOpenFileDialog dialog = new();
            dialog.Filter = "Scalable Vector Graphics (*.svg)|*.svg";

            var result = dialog.ShowDialog();
            if (result == false)
                return;

            fileName = dialog.FileName;

            var settings = new WpfDrawingSettings();
            settings.IncludeRuntime = true;
            settings.TextAsGeometry = true;

            var converter = new FileSvgConverter(settings);
            converter.Convert(fileName);
            var xaml = File.ReadAllText(fileName.Replace(".svg", ".xaml"));
            File.Delete(fileName.Replace(".svg", ".xaml"));

            var parsed = (DrawingGroup) XamlReader.Parse(xaml,
                new ParserContext {BaseUri = new Uri(Path.GetDirectoryName(fileName))});
            var geometry = GatherGeometry(parsed);

            var group = new DrawingGroup {Children = new DrawingCollection(geometry)};
            var stringValue = "";
            foreach (var geometryDrawing in geometry)
            {
                var scaled = Geometry.Combine(
                    geometryDrawing.Geometry,
                    geometryDrawing.Geometry, GeometryCombineMode.Intersect,
                    new TransformGroup
                    {
                        Children = new TransformCollection
                        {
                            new TranslateTransform(group.Bounds.X * -1, group.Bounds.Y * -1),
                            new ScaleTransform(1.0 / group.Bounds.Width, 1.0 / group.Bounds.Height)
                        }
                    }
                );

                var scaledString = scaled.ToString(CultureInfo.InvariantCulture)
                    .Replace("F1", "")
                    .Replace(";", ",")
                    .Replace("L", " L")
                    .Replace("C", " C");

                stringValue = stringValue + " " + scaledString;
            }

            InputShapeData = stringValue.Trim();
            ApplyInput();
        }

        public List<GeometryDrawing> GatherGeometry(DrawingGroup drawingGroup)
        {
            var result = new List<GeometryDrawing>();
            result.AddRange(drawingGroup.Children.Where(c => c is GeometryDrawing).Cast<GeometryDrawing>());
            foreach (var childGroup in drawingGroup.Children.Where(c => c is DrawingGroup).Cast<DrawingGroup>())
                result.AddRange(GatherGeometry(childGroup));

            return result;
        }

        private Point GetPercentagePosition(Point position)
        {
            return new(Math.Round(position.X / LedLayout.Width, 3), Math.Round(position.Y / LedLayout.Height, 3));
        }

        protected override void OnClose()
        {
            if (_fileWatcher != null)
                _fileWatcher.Changed -= FileWatcherOnChanged;
            base.OnClose();
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
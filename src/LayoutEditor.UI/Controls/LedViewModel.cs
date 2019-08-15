using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using LayoutEditor.UI.Models;
using RGB.NET.Core;
using RGB.NET.Core.Layout;
using Stylet;
using Color = System.Windows.Media.Color;

namespace LayoutEditor.UI.Controls
{
    public class LedViewModel : PropertyChangedBase
    {
        private readonly DeviceLayoutViewModel _layoutViewModel;
        private LedImage _ledImage;

        public LedViewModel(LayoutEditModel model, DeviceLayoutViewModel layoutViewModel, LedLayout ledLayout)
        {
            _layoutViewModel = layoutViewModel;

            Model = model;
            LedLayout = ledLayout;
            Update();
        }

        public LayoutEditModel Model { get; }
        public LedLayout LedLayout { get; }

        public string Tooltip { get; set; }
        public string LedImagePath => Model.GetAbsoluteImageDirectory(_ledImage?.Image);
        public bool Selected { get; set; }

        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public DrawingImage DisplayDrawing { get; set; }

        public void Update()
        {
            Tooltip = LedLayout.Id;

            X = LedLayout.X;
            Y = LedLayout.Y;
            Width = LedLayout.Width;
            Height = LedLayout.Height;

            CreateLedGeometry();
        }

        private void CreateLedGeometry()
        {
            var relativeRectangle = new Rect(0, 0, LedLayout.Width, LedLayout.Height);
            Geometry geometry;
            switch (LedLayout.Shape)
            {
                case Shape.Custom:
                    geometry = Geometry.Parse(LedLayout.ShapeData);
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

            var drawing = new GeometryDrawing(null, new Pen(null, 2), geometry);
            
            // The pen needs some adjustments when drawing custom shapes, a thickness of 2 just means you get a very thick pen that covers the
            // entire shape.. I'm not sure why to be honest sssh don't tell
            if (LedLayout.Shape == Shape.Custom)
            {
                drawing.Pen.Thickness = 0.075;
            }

            DisplayDrawing = new DrawingImage(drawing);
            ChangeColor(Colors.Red);
        }

        public void UpdateLedImage(LedImage ledImage)
        {
            _ledImage = ledImage;
            NotifyOfPropertyChange(() => LedImagePath);
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
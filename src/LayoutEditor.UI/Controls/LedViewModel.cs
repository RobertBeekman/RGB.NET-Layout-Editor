using System.ComponentModel;
using RGB.NET.Core.Layout;
using Stylet;

namespace LayoutEditor.UI.Controls
{
    public class LedViewModel : PropertyChangedBase
    {
        private LedImage _ledImage;

        public LedViewModel(LedLayout ledLayout)
        {
            LedLayout = ledLayout;
            Update();

            PropertyChanged += OnPropertyChanged;
        }
        
        public LedLayout LedLayout { get; }

        public string Tooltip { get; set; }
        public string ImagePath { get; set; }

        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public void Update()
        {
            Tooltip = LedLayout.Id;

            X = LedLayout.X;
            Y = LedLayout.Y;
            Width = LedLayout.Width;
            Height = LedLayout.Height;
        }

        public void UpdateLedImage(LedImage ledImage)
        {
            _ledImage = ledImage;
            ImagePath = ledImage.Image;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ImagePath))
                _ledImage.Image = ImagePath;
        }
    }
}
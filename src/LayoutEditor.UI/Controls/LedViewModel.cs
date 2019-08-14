using LayoutEditor.UI.Models;
using RGB.NET.Core.Layout;
using Stylet;

namespace LayoutEditor.UI.Controls
{
    public class LedViewModel : PropertyChangedBase
    {
        private LedImage _ledImage;

        public LedViewModel(LayoutEditModel model, LedLayout ledLayout)
        {
            Model = model;
            LedLayout = ledLayout;
            Update();
        }

        public LayoutEditModel Model { get; }
        public LedLayout LedLayout { get; }

        public string Tooltip { get; set; }
        public string LedImagePath => Model.GetAbsoluteImageDirectory(_ledImage?.Image);

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
            NotifyOfPropertyChange(() => LedImagePath);
        }
    }
}
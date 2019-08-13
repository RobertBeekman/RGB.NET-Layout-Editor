using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LayoutEditor.UI.Pages;
using RGB.NET.Core.Layout;
using Stylet;

namespace LayoutEditor.UI.Controls
{
    public class DeviceLayoutViewModel : PropertyChangedBase
    {
        private Point? _lastPanPosition;

        public DeviceLayoutViewModel(DeviceLayout deviceLayout, DeviceLayoutEditorViewModel editorViewModel)
        {
            DeviceLayout = deviceLayout;
            EditorViewModel = editorViewModel;
            
            LedViewModels = new BindableCollection<LedViewModel>(DeviceLayout.Leds.Select(l => new LedViewModel(l)));
            UpdateLedImages();
        }

        public void UpdateLedImages()
        {
            var imageLayout = DeviceLayout.LedImageLayouts.FirstOrDefault(l => l.Layout == EditorViewModel.SelectedImageLayout);
            if (imageLayout == null)
                return;

            foreach (var ledViewModel in LedViewModels)
            {
                // Try to find a matching LED image layout
                var ledImage = imageLayout.LedImages.FirstOrDefault(i => i.Id == ledViewModel.LedLayout.Id);
                ledViewModel.UpdateLedImage(ledImage);
            }
        }

        public DeviceLayout DeviceLayout { get; }
        public DeviceLayoutEditorViewModel EditorViewModel { get; }
        public IObservableCollection<LedViewModel> LedViewModels { get; set; }

        public double Zoom { get; set; } = 1;
        public double PanX { get; set; } = 1;
        public double PanY { get; set; } = 1;

        public void ChangeZoomLevel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
                Zoom -= 0.2;
            else
                Zoom += 0.2;

            Zoom = Math.Max(0.2, Zoom);
        }

        public void Pan(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                _lastPanPosition = null;
                return;
            }

            if (_lastPanPosition == null)
                _lastPanPosition = e.GetPosition((IInputElement) sender);

            var position = e.GetPosition((IInputElement) sender);
            var delta = _lastPanPosition - position;
            PanX -= delta.Value.X;
            PanY -= delta.Value.Y;

            _lastPanPosition = position;
        }

        public void AddLed()
        {

        }

        public void RemoveLed()
        {

        }
    }
}
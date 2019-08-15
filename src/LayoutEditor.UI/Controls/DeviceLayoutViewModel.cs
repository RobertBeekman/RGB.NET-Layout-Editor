using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using LayoutEditor.UI.Models;
using LayoutEditor.UI.Pages;
using RGB.NET.Core.Layout;
using Stylet;

namespace LayoutEditor.UI.Controls
{
    public class DeviceLayoutViewModel : PropertyChangedBase
    {
        private Point? _lastPanPosition;

        public DeviceLayoutViewModel(LayoutEditModel model, DeviceLayoutEditorViewModel editorViewModel)
        {
            Model = model;
            DeviceLayout = model.DeviceLayout;
            EditorViewModel = editorViewModel;
            LedViewModels = new BindableCollection<LedViewModel>(DeviceLayout.Leds.Select(l => new LedViewModel(Model, this, l)));

            UpdateLeds();

            EditorViewModel.PropertyChanged += EditorViewModelOnPropertyChanged;
        }


        public LayoutEditModel Model { get; }
        public DeviceLayout DeviceLayout { get; }
        public DeviceLayoutEditorViewModel EditorViewModel { get; }
        public BindableCollection<LedViewModel> LedViewModels { get; set; }
        public LedViewModel SelectedLed { get; set; }

        public double Zoom { get; set; } = 1;
        public double PanX { get; set; } = 1;
        public double PanY { get; set; } = 1;
        
        private void EditorViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EditorViewModel.ImageBasePath) || e.PropertyName == nameof(EditorViewModel.SelectedImageLayout))
                UpdateLeds();
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


        public void UpdateLeds()
        {
            UpdateLedPositions();

            var imageLayout = DeviceLayout.LedImageLayouts.FirstOrDefault(l => l.Layout == EditorViewModel.SelectedImageLayout);
            foreach (var ledViewModel in LedViewModels)
            {
                ledViewModel.CreateLedGeometry();
                if (imageLayout != null)
                {
                    // Try to find a matching LED image layout
                    var ledImage = imageLayout.LedImages.FirstOrDefault(i => i.Id == ledViewModel.LedLayout.Id);
                    ledViewModel.UpdateLedImage(ledImage);
                }
            }
        }

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

        public void SelectLed(LedViewModel ledViewModel)
        {
            if (SelectedLed != null)
            {
                SelectedLed.Selected = false;
                SelectedLed.ChangeColor(Colors.Red);
            }

            SelectedLed = ledViewModel;
            SelectedLed.Selected = true;
            SelectedLed.ChangeColor(Colors.Yellow);
        }

        public void ApplyLed()
        {
            SelectedLed.ApplyInput();
        }

        public void AddLed()
        {
            // TODO: Figure out what defaults to populate
            var ledLayout = new LedLayout();

            var ledViewModel = new LedViewModel(Model, this, ledLayout);
            LedViewModels.Add(ledViewModel);
            SelectedLed = ledViewModel;
        }

        public void RemoveLed()
        {
            SelectedLed = null;
        }
    }
}
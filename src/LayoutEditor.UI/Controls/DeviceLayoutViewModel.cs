using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LayoutEditor.UI.Dialogs;
using LayoutEditor.UI.Models;
using LayoutEditor.UI.Pages;
using RGB.NET.Core.Layout;
using Stylet;

namespace LayoutEditor.UI.Controls
{
    public class DeviceLayoutViewModel : PropertyChangedBase
    {
        private readonly IWindowManager _windowManager;
        private Point? _lastPanPosition;
        private bool _movingLed;

        public DeviceLayoutViewModel(LayoutEditModel model, DeviceLayoutEditorViewModel editorViewModel, IWindowManager windowManager)
        {
            _windowManager = windowManager;

            Model = model;
            DeviceLayout = model.DeviceLayout;
            EditorViewModel = editorViewModel;
            LedViewModels = new BindableCollection<LedViewModel>(DeviceLayout.Leds.Select(l => new LedViewModel(Model, this, _windowManager, l)));

            UpdateLeds();

            PropertyChanged += DeviceLayoutViewModelPropertyChanged;
            EditorViewModel.PropertyChanged += EditorViewModelOnPropertyChanged;


            var activeWindow = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            if (activeWindow != null)
            {
                activeWindow.KeyDown += KeyUpDown;
                activeWindow.KeyUp += KeyUpDown;
            }
        }

        public LayoutEditModel Model { get; }
        public DeviceLayout DeviceLayout { get; }
        public DeviceLayoutEditorViewModel EditorViewModel { get; }
        public BindableCollection<LedViewModel> LedViewModels { get; set; }
        public LedViewModel SelectedLed { get; set; }
        public string LedImageText => $"LED Image ({EditorViewModel.SelectedImageLayout})";

        public double Zoom { get; set; } = 1;
        public double PanX { get; set; } = 1;
        public double PanY { get; set; } = 1;

        private void DeviceLayoutViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Handle changing selected LED from listbox
            if (e.PropertyName == nameof(SelectedLed) && SelectedLed != null && !SelectedLed.Selected)
            {
                var oldSelection = LedViewModels.FirstOrDefault(l => l.Selected);
                if (oldSelection != null)
                {
                    oldSelection.Selected = false;
                    oldSelection.SetColor(LedViewModel.UnselectedColor);
                }

                SelectLed(SelectedLed);
            }
        }

        private void EditorViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EditorViewModel.ImageBasePath))
            {
                FileChangedWatcher.SetWatchDirectory(Path.Combine(Model.BasePath, Model.DeviceLayout.ImageBasePath));
                NotifyOfPropertyChange(() => LedImageText);
                UpdateLeds();
            }
            else if (e.PropertyName == nameof(EditorViewModel.SelectedImageLayout))
            {
                NotifyOfPropertyChange(() => LedImageText);
                UpdateLeds();
            }
        }

        public void UpdateLeds()
        {
            // Update the LEDs in the RGB.NET device layout
            if (DeviceLayout.Leds != null)
            {
                LedLayout lastLed = null;
                foreach (var led in DeviceLayout.Leds)
                {
                    led.CalculateValues(DeviceLayout, lastLed);
                    lastLed = led;
                }
            }

            // Update the LEDs in the VMs
            var imageLayout = DeviceLayout.LedImageLayouts?.FirstOrDefault(l => l.Layout != null && l.Layout.Equals(EditorViewModel.SelectedImageLayout));
            foreach (var ledViewModel in LedViewModels)
            {
                ledViewModel.Update();
                if (imageLayout != null)
                {
                    // Try to find a matching LED image layout
                    var ledImage = imageLayout.LedImages.FirstOrDefault(i => i.Id.Equals(ledViewModel.LedLayout.Id));
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
            if (_movingLed)
                return;

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

        public void MoveLed(object sender, MouseEventArgs e)
        {
            if (!_movingLed || SelectedLed == null || e.LeftButton == MouseButtonState.Released)
                return;

            var position = e.GetPosition((IInputElement) sender);
            SelectedLed.InputX = Math.Round(position.X - SelectedLed.LedLayout.Width / 2, 1).ToString(CultureInfo.InvariantCulture);
            SelectedLed.InputY = Math.Round(position.Y - SelectedLed.LedLayout.Height / 2, 1).ToString(CultureInfo.InvariantCulture);
            SelectedLed.ApplyInput();
        }

        public void KeyUpDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
            {
                _movingLed = e.IsDown;
                Mouse.OverrideCursor = _movingLed ? Cursors.SizeAll : null;
            }
            else if (e.Key == Key.Escape)
            {
                if (SelectedLed != null)
                {
                    SelectedLed.Selected = false;
                    SelectedLed.SetColor(LedViewModel.UnselectedColor);
                    SelectedLed = null;
                }
            }
        }


        public void SelectLed(LedViewModel ledViewModel)
        {
            if (SelectedLed != null)
            {
                SelectedLed.Selected = false;
                SelectedLed.SetColor(LedViewModel.UnselectedColor);
            }

            if (ledViewModel != null)
            {
                SelectedLed = ledViewModel;
                SelectedLed.Selected = true;
                SelectedLed.SetColor(LedViewModel.SelectedColor);
            }
        }

        public void SelectLedImage()
        {
            SelectedLed.SelectImage();
        }

        public void ApplyLed()
        {
            SelectedLed.ApplyInput();
        }

        public void AddLed(string addBefore)
        {
            var addBeforeBool = bool.Parse(addBefore);
            _windowManager.ShowDialog(new AddLedViewModel(addBeforeBool, this));
        }

        public void FinishAddLed(bool addBefore, string ledId)
        {
            int index;
            if (SelectedLed == null)
            {
                if (addBefore)
                    index = 0;
                else
                    index = LedViewModels.Count;
            }
            else
            {
                if (addBefore)
                    index = LedViewModels.IndexOf(SelectedLed);
                else
                    index = LedViewModels.IndexOf(SelectedLed) + 1;
            }

            var ledLayout = new LedLayout {Id = ledId};
            var ledViewModel = new LedViewModel(Model, this, _windowManager, ledLayout);

            DeviceLayout.Leds.Insert(index, ledLayout);
            LedViewModels.Insert(index, ledViewModel);

            UpdateLeds();
            SelectLed(ledViewModel);
        }

        public void RemoveLed()
        {
            if (SelectedLed != null)
            {
                var ledToRemove = SelectedLed;

                // Remove from view
                LedViewModels.Remove(ledToRemove);
                // Remove from image layouts
                foreach (var imageLayout in DeviceLayout.LedImageLayouts)
                    imageLayout.LedImages.RemoveAll(i => i.Id.Equals(ledToRemove.LedLayout.Id));
                // Remove from layout
                DeviceLayout.Leds.Remove(ledToRemove.LedLayout);
                SelectLed(null);
                UpdateLeds();
            }
        }
    }
}
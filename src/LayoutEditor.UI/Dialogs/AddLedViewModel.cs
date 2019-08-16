using LayoutEditor.UI.Controls;
using Stylet;

namespace LayoutEditor.UI.Dialogs
{
    public class AddLedViewModel : Screen
    {
        private readonly bool _addBefore;
        private readonly DeviceLayoutViewModel _deviceLayoutViewModel;

        public AddLedViewModel(bool addBefore, DeviceLayoutViewModel deviceLayoutViewModel)
        {
            _addBefore = addBefore;
            _deviceLayoutViewModel = deviceLayoutViewModel;

            AvailableLedIds = new BindableCollection<string>();
            AvailableLedIds.AddRange(deviceLayoutViewModel.Model.GetAvailableLedIds());
        }

        public BindableCollection<string> AvailableLedIds { get; set; }
        public string SelectedId { get; set; }
        public bool CanAddLed => SelectedId != null;

        public void AddLed()
        {
            _deviceLayoutViewModel.FinishAddLed(_addBefore, SelectedId);
            RequestClose(true);
        }

        public void Cancel()
        {
            RequestClose(false);
        }
    }
}
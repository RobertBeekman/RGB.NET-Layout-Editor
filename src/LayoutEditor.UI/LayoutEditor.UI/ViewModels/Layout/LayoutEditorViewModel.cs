using System.Reactive.Disposables;
using System.Threading.Tasks;
using LayoutEditor.UI.Services.DialogService.Interfaces;
using LayoutEditor.UI.ViewModels.Layout.Dialogs;
using LayoutEditor.UI.Views.Layout;
using ReactiveUI;

namespace LayoutEditor.UI.ViewModels.Layout
{
    public class LayoutEditorViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;

        public LayoutEditorViewModel()
        {

        }

        public LayoutEditorViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public LayoutEditorView View { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }

        public async Task OpenDetails()
        {
            await _dialogService.ShowDialogAsync<LayoutPropertiesDialogViewModel>();
        }
    }
}
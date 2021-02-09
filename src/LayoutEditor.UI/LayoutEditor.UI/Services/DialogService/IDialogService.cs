using System.Collections.Generic;
using System.Threading.Tasks;
using LayoutEditor.UI.Services.Interfaces;
using LayoutEditor.UI.ViewModels;

namespace LayoutEditor.UI.Services.DialogService
{
    public interface IDialogService : IUIService
    {
        Task<TResult> ShowDialogAsync<TViewModel, TResult>() where TViewModel : DialogViewModelBase<TResult>;
        Task<TResult> ShowDialogAsync<TViewModel, TResult>(Dictionary<string, object> parameters) where TViewModel : DialogViewModelBase<TResult>;
        
        Task ShowDialogAsync<TViewModel>() where TViewModel : DialogViewModelBase;
        Task ShowDialogAsync<TViewModel>(Dictionary<string, object> parameters) where TViewModel : DialogViewModelBase;
    }
}

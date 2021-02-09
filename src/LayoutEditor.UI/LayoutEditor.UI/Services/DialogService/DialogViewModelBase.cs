using System;
using LayoutEditor.UI.ViewModels;

namespace LayoutEditor.UI.Services.DialogService
{
    public class DialogViewModelBase<TResult> : ViewModelBase
    {
        public event EventHandler<DialogResultEventArgs<TResult>>? CloseRequested;

        protected void Close() => Close(default);

        protected void Close(TResult? result)
        {
            var args = new DialogResultEventArgs<TResult>(result);

            CloseRequested?.Invoke(this, args);
        }
    }

    public class DialogViewModelBase : DialogViewModelBase<object>
    {
    }
}
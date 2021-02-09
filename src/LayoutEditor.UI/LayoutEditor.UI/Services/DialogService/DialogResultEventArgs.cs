using System;

namespace LayoutEditor.UI.Services.DialogService
{
    public class DialogResultEventArgs<TResult> : EventArgs
    {
        public TResult? Result { get; }

        public DialogResultEventArgs(TResult? result)
        {
            Result = result;
        }
    }
}
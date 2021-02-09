namespace LayoutEditor.UI.ViewModels
{
    public class DialogViewModelBase<TResult> : ViewModelBase
    {
        public void Close(TResult result)
        {

        }
    }

    public class DialogViewModelBase : ViewModelBase
    {
        public void Close()
        {

        }
    }
}
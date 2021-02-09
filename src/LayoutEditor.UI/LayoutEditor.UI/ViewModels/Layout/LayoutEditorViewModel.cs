using System.Reactive.Disposables;
using System.Threading.Tasks;
using LayoutEditor.UI.Views.Layout;
using ReactiveUI;

namespace LayoutEditor.UI.ViewModels.Layout
{
    public class LayoutEditorViewModel : ViewModelBase, IActivatableViewModel
    {
        public LayoutEditorViewModel()
        {
            Activator = new ViewModelActivator();
            this.WhenActivated(disposables =>
            {
                Activate();
                Disposable
                    .Create(() => { Deactivate(); })
                    .DisposeWith(disposables);
            });
        }

        public LayoutEditorView View { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }

        #region Implementation of IActivatableViewModel

        /// <inheritdoc />
        public ViewModelActivator Activator { get; }

        #endregion

        private void Activate()
        {
        }

        private void Deactivate()
        {
        }

        public async Task OpenDetails()
        {
        }
    }
}
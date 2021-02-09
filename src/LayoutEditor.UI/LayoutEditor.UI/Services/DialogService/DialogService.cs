using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using LayoutEditor.UI.Services.DialogService.Interfaces;
using Ninject;
using Ninject.Parameters;

namespace LayoutEditor.UI.Services.DialogService
{
    public class DialogService : IDialogService
    {
        private readonly IKernel _kernel;
        private readonly Window _mainWindow;
        private readonly ViewLocator _viewLocator;

        public DialogService(IKernel kernel)
        {
            _kernel = kernel;

            var lifetime = (IClassicDesktopStyleApplicationLifetime) Application.Current.ApplicationLifetime;
            _mainWindow = lifetime.MainWindow;
            _viewLocator = new ViewLocator();
        }

        public async Task<TResult> ShowDialogAsync<TViewModel, TResult>() where TViewModel : DialogViewModelBase<TResult>
        {
            var viewModel = _kernel.Get<TViewModel>();
            if (!(_viewLocator.Build(viewModel) is DialogWindowBase<TResult> view))
                throw new Exception($"Failed to find view of type DialogWindowBase<TResult> for {viewModel.GetType().Name}");

            return await view.ShowDialog<TResult>(_mainWindow);
        }

        public async Task<TResult> ShowDialogAsync<TViewModel, TResult>(Dictionary<string, object> parameters) where TViewModel : DialogViewModelBase<TResult>
        {
            var paramsArray = parameters
                .Select(kv => new ConstructorArgument(kv.Key, kv.Value)).Cast<IParameter>()
                .ToArray();
            var viewModel = _kernel.Get<TViewModel>(paramsArray);
            if (!(_viewLocator.Build(viewModel) is DialogWindowBase<TResult> view))
                throw new Exception($"Failed to find view of type DialogWindowBase<TResult> for {viewModel.GetType().Name}");

            return await view.ShowDialog<TResult>(_mainWindow);
        }

        public async Task ShowDialogAsync<TViewModel>() where TViewModel : DialogViewModelBase
        {
            var viewModel = _kernel.Get<TViewModel>();
            if (!(_viewLocator.Build(viewModel) is DialogWindowBase view))
                throw new Exception($"Failed to find view of type DialogWindowBase for {viewModel.GetType().Name}");

            await view.ShowDialog(_mainWindow);
        }

        public async Task ShowDialogAsync<TViewModel>(Dictionary<string, object> parameters) where TViewModel : DialogViewModelBase
        {
            var paramsArray = parameters
                .Select(kv => new ConstructorArgument(kv.Key, kv.Value)).Cast<IParameter>()
                .ToArray();
            var viewModel = _kernel.Get<TViewModel>(paramsArray);
            if (!(_viewLocator.Build(viewModel) is DialogWindowBase view))
                throw new Exception($"Failed to find view of type DialogWindowBase for {viewModel.GetType().Name}");

            await view.ShowDialog(_mainWindow);
        }
    }
}
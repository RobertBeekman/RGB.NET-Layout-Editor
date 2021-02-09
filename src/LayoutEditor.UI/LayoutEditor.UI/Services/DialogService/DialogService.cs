using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using LayoutEditor.UI.ViewModels;
using LayoutEditor.UI.Views;
using Splat;

namespace LayoutEditor.UI.Services.DialogService
{
    public class DialogService : IDialogService
    {
        private Window _mainWindow;

        public DialogService()
        {
            var lifetime = (IClassicDesktopStyleApplicationLifetime) Application.Current.ApplicationLifetime;
            _mainWindow = lifetime.MainWindow;
        }

        public async Task<TResult> ShowDialogAsync<TViewModel, TResult>() where TViewModel : DialogViewModelBase<TResult>
        {
            throw new NotImplementedException();
        }
        
        public async Task<TResult> ShowDialogAsync<TViewModel, TResult>(Dictionary<string, object> parameters) where TViewModel : DialogViewModelBase<TResult>
        {
            throw new NotImplementedException();
        }
        
        public async Task ShowDialogAsync<TViewModel>() where TViewModel : DialogViewModelBase
        {
            throw new NotImplementedException();
        }

        public async Task ShowDialogAsync<TViewModel>(Dictionary<string, object> parameters) where TViewModel : DialogViewModelBase
        {
            throw new NotImplementedException();
        }
    }
}
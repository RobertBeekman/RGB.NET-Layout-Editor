using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LayoutEditor.UI.Ninject;
using LayoutEditor.UI.ViewModels;
using LayoutEditor.UI.Views;
using Ninject;

namespace LayoutEditor.UI
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            Kernel = new StandardKernel(new EditorModule());
        }

        public StandardKernel Kernel { get; set; }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = Kernel.Get<MainWindowViewModel>(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}

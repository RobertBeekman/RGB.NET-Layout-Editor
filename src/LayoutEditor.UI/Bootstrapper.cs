using System.Windows;
using System.Windows.Threading;
using LayoutEditor.UI.Pages;
using Stylet;
using StyletIoC;

namespace LayoutEditor.UI
{
    public class Bootstrapper : Bootstrapper<ShellViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            // Configure the IoC container in here
        }

        protected override void Configure()
        {
            // Perform any other configuration before the application starts
        }

        protected override void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            var windowManager = Container.Get<IWindowManager>();
            windowManager.ShowMessageBox(
                e.Exception.Message + "\r\n" +
                "Copied stack trace to clipboard\r\n" +
                "It's best you save your work and restart.",
                e.Exception.GetType().Name
            );
            Clipboard.SetText(e.Exception.ToString());

            e.Handled = true;
        }
    }
}
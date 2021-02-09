using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LayoutEditor.UI.Services.Interfaces;
using LayoutEditor.UI.ViewModels;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Ninject.Planning.Bindings.Resolvers;

namespace LayoutEditor.UI.Ninject
{
    public class EditorModule : NinjectModule
    {
        #region Overrides of NinjectModule

        /// <inheritdoc />
        public override void Load()
        {
            Kernel.Components.Remove<IMissingBindingResolver, SelfBindingResolver>();

            // Bind all services as singletons
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .IncludingNonPublicTypes()
                    .SelectAllClasses()
                    .InheritedFrom<IUIService>()
                    .BindAllInterfaces()
                    .Configure(c => c.InSingletonScope());
            });

            // Bind all built-in VMs
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<ViewModelBase>()
                    .BindToSelf()
                    .Configure(c => c.InSingletonScope());
            });
        }

        #endregion
    }
}
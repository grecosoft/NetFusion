using Autofac;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Logging.Core;
using System;

namespace NetFusion.Logging.Modules
{
    public class CompositeLogModule : PluginModule
    {
        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.RegisterType<CompositeLogger>()
                .As<ICompositeLogger>()
                .SingleInstance();
        }

        public override void RunModule(ILifetimeScope scope)
        {
            try
            {
                var logger = scope.Resolve<ICompositeLogger>();
                logger.Log(AppContainer.Instance.Log);
            }
            catch (Exception ex)
            {
                Plugin.Log.Error("Composite Application could not be logged.");
            }
        }
    }
}

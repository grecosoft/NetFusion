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
                var manifest = Context.AppHost.Manifest;

                var hostLog = new HostLog(
                    manifest.Name,
                    manifest.PluginId,
                    AppContainer.Instance.Log);

                logger.Log(hostLog);
            }
            catch (Exception ex)
            {
                Plugin.Log.Error("Composite Application could not be logged.");
            }
        }
    }
}

using Autofac;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Logging.Core;
using System;

namespace NetFusion.Logging.Modules
{
    /// <summary>
    /// Module that will run during the application bootstrap process
    /// that will submit the composite application log to a specified
    /// endpoint.
    /// </summary>
    public class CompositeLogModule : PluginModule
    {
        public override void RegisterDefaultComponents(ContainerBuilder builder)
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

                // Create request matching the expected api:
                var hostLog = new HostLog(
                    manifest.Name,
                    manifest.PluginId,
                    AppContainer.Instance.Log);

                logger.Log(hostLog);
            }
            catch (Exception ex)
            {
                // This exception will not be re-thrown since it don't impact
                // the execution of the application.
                this.Context.Logger.Error("Composite Application could not be logged.", ex);
            }
        }
    }
}

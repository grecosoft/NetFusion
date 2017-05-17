using Autofac;
using NetFusion.Bootstrap.Plugins;
using NetFusion.RabbitMQ.Core;

namespace NetFusion.Integration.RabbitMQ
{
    public class RabbitIntegrationModule : PluginModule
    {
        public override void RegisterComponents(ContainerBuilder builder)
        {
            builder.RegisterType<BrokerMetaRepository>()
                .As<IBrokerMetaRepository>()
                .SingleInstance();
        }
    }
}

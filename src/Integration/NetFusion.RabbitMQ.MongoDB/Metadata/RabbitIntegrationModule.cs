using Autofac;
using NetFusion.Bootstrap.Plugins;
using NetFusion.RabbitMQ.Core;

namespace NetFusion.RabbitMQ.MongoDB.Metadata
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

using NetFusion.Bootstrap.Testing;
using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Modules;

public static class HostTypeResolverExtensions
{
    public static void AddRabbitMqPlugin(this TestTypeResolver resolver)
    {
        resolver.AddPlugin<MockCorePlugin>()
            .AddPluginType<MessageBrokerModule>()
            .AddPluginType<BrokerSettings>();
    }
}
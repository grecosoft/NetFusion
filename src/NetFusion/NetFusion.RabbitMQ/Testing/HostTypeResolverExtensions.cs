using NetFusion.Bootstrap.Testing;
using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Modules;

/// <summary>
/// Adds a mock core plug-in with the needed plug-in types required to
/// bootstrap the RabbitMQ plug-in.
/// </summary>
public static class HostTypeResolverExtensions
{
    /// <summary>
    /// Adds core plug-in with required RabbitMQ types.
    /// </summary>
    /// <param name="resolver">The test type resolver</param>
    public static void AddRabbitMqPlugin(this TestTypeResolver resolver)
    {
        resolver.AddPlugin<MockCorePlugin>()
            .AddPluginType<MessageBrokerModule>()
            .AddPluginType<BrokerSettings>();
    }
}
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Integration.RabbitMQ.Bus;
using NetFusion.Integration.RabbitMQ.Internal;
using NetFusion.Integration.RabbitMQ.Plugin;
using NetFusion.Integration.RabbitMQ.Plugin.Configs;

namespace NetFusion.Integration.UnitTests.RabbitMQ.Mocks;

public class TestRabbitBusModule : PluginModule,
    IBusModule
{
    public RabbitMqConfig RabbitMqConfig { get; } = new();

    public IBusConnection GetConnection(string busName)
    {
        throw new NotImplementedException();
    }

    public event EventHandler<ReconnectionEventArgs>? Reconnection;
}
using NetFusion.Integration.RabbitMQ.Bus;
using NetFusion.Integration.RabbitMQ.Internal;
using NetFusion.Integration.RabbitMQ.Plugin.Configs;

namespace NetFusion.Integration.RabbitMQ.Plugin;

public interface IBusModule : IPluginModuleService
{
    RabbitMqConfig RabbitMqConfig { get; }
    
    BusConnection GetConnection(string busName);

    public event EventHandler<ReconnectionEventArgs>? Reconnection;
}
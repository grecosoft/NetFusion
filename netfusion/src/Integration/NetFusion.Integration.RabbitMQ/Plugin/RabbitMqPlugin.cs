using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.Settings.Plugin;
using NetFusion.Integration.RabbitMQ.Internal;
using NetFusion.Integration.RabbitMQ.Plugin.Configs;
using NetFusion.Integration.RabbitMQ.Plugin.Modules;
using NetFusion.Messaging.Plugin;
using NetFusion.Messaging.Plugin.Configs;

namespace NetFusion.Integration.RabbitMQ.Plugin;

public class RabbitMqPlugin : PluginBase
{
    public override string PluginId => "2B389655-E790-4D30-B19C-C06AB8096C6A";
    public override PluginTypes PluginType => PluginTypes.CorePlugin;
    public override string Name => "NetFusion: RabbitMQ";
        
    public RabbitMqPlugin()
    {
        AddConfig<RabbitMqConfig>();
            
        AddModule<BusModule>();
        AddModule<BusEntityModule>();
    
        SourceUrl = "https://github.com/grecosoft/NetFusion/tree/master/netfusion/src/Integration/NetFusion.RabbitMq";
        DocUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.rabbitmq.overview#rabbitmq-overview";
    }
}
    
public static class CompositeBuilderExtensions
{
    // Adds the RabbitMQ plugin to the composite application.
    public static ICompositeContainerBuilder AddRabbitMq(this ICompositeContainerBuilder composite)
    {
        return composite
            .AddSettings()
            .AddMessaging()
            .AddPlugin<RabbitMqPlugin>()
                
            // Extend the base messaging pipeline by adding the RabbitMqPublisher.
            .InitPluginConfig<MessageDispatchConfig>(config => 
                config.AddPublisher<RabbitMqPublisher>());
    }
}
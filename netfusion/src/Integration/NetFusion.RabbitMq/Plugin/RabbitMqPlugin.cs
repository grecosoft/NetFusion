using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging.Plugin;
using NetFusion.Messaging.Plugin.Configs;
using NetFusion.RabbitMQ.Plugin.Modules;
using NetFusion.RabbitMQ.Publisher;
using NetFusion.Settings.Plugin;

namespace NetFusion.RabbitMQ.Plugin
{
    public class RabbitMqPlugin : PluginBase
    {
        public override string PluginId => "2B389655-E790-4D30-B19C-C06AB8096C6A";
        public override PluginTypes PluginType => PluginTypes.CorePlugin;
        public override string Name => "RabbitMq Plugin based on EasyNetQ library.";

        public RabbitMqPlugin()
        {
            AddModule<BusModule>();
            AddModule<PublisherModule>();
            AddModule<SubscriberModule>();
            
            SourceUrl = "https://github.com/grecosoft/NetFusion/tree/master/netfusion/src/Integration/NetFusion.RabbitMq";
            DocUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.rabbitmq.overview#rabbitmq-overview";
        }
    }
    
    public static class CompositeBuilderExtensions
    {
        public static ICompositeContainerBuilder AddRabbitMq(this ICompositeContainerBuilder composite)
        {
            // Add dependent plugins:
            composite
                .AddSettings()
                .AddMessaging();
            
            // Add Rabbit MQ Plugin:
            composite.AddPlugin<RabbitMqPlugin>();

            // Integrate with base messaging plugin:
            var dispatchConfig = composite.GetPluginConfig<MessageDispatchConfig>();
            dispatchConfig.AddPublisher<RabbitMqPublisher>();
            
            return composite;
        }
    }
}
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging.Plugin.Configs;
using NetFusion.RabbitMQ.Plugin.Modules;
using NetFusion.RabbitMQ.Publisher;

namespace NetFusion.RabbitMQ.Plugin
{
    public class RabbitMqPlugin : PluginBase
    {
        public override string PluginId => "2B389655-E790-4D30-B19C-C06AB8096C6A";
        public override PluginTypes PluginType => PluginTypes.CorePlugin;
        public override string Name => "RabbitMq Plugin based on EasyNetQ library.";

        public RabbitMqPlugin()
        {
            SourceUrl = "https://github.com/grecosoft/NetFusion/tree/master/netfusion/src/Integration/NetFusion.RabbitMq";
            DocUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.rabbitmq.overview#rabbitmq-overview";
            
            // Modules:
            AddModule<BusModule>();
            AddModule<PublisherModule>();
            AddModule<SubscriberModule>();
        }
    }
    
    public static class CompositeBuilderExtensions
    {
        public static IComposeAppBuilder AddRabbitMq(this IComposeAppBuilder composite)
        {
            composite.AddPlugin<RabbitMqPlugin>();

            var dispatchConfig = composite.GetConfig<MessageDispatchConfig>();
            dispatchConfig.AddMessagePublisher<RabbitMqPublisher>();
            
            return composite;
        }
    }
}
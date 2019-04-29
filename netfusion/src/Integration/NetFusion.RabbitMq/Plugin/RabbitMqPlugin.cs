using NetFusion.Bootstrap.Refactors;
using NetFusion.Messaging.Config;
using NetFusion.RabbitMQ.Publisher;

namespace NetFusion.RabbitMQ.Plugin
{
    public class RabbitMqPlugin : PluginDefinition
    {
        public override string PluginId => "2B389655-E790-4D30-B19C-C06AB8096C6A";
        public override PluginDefinitionTypes PluginType => PluginDefinitionTypes.CorePlugin;
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
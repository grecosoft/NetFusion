using NetFusion.AMQP.Plugin.Modules;
using NetFusion.AMQP.Publisher;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging.Plugin.Configs;

namespace NetFusion.AMQP.Plugin
{
    public class AmqpPlugin : PluginBase
    {
        public override string PluginId => "35273B60-72EE-4428-97F1-2EB51A88B32A";
        public override PluginTypes PluginType => PluginTypes.CorePlugin;
        public override string Name => "NetFusion AMQP Messaging Plug-in";

        public AmqpPlugin()
        {
            AddModule<ConnectionModule>();
            AddModule<PublisherModule>();
            AddModule<SubscriberModule>();
            
            SourceUrl = "https://github.com/grecosoft/NetFusion/tree/master/netfusion/src/Integration/NetFusion.Azure.Messaging";
        }
    }
    
    public static class CompositeBuilderExtensions
    {
        public static ICompositeContainerBuilder AddAmqp(this ICompositeContainerBuilder composite)
        {
            var dispatchConfig = composite.GetConfig<MessageDispatchConfig>();
            dispatchConfig.AddMessagePublisher<HostMessagePublisher>();
            
            // Augment the base messaging plugin:
            composite.AddPlugin<AmqpPlugin>();
            return composite;
        }
    }
}
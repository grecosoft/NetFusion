using NetFusion.AMQP.Plugin.Modules;
using NetFusion.AMQP.Publisher;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging.Plugin;
using NetFusion.Messaging.Plugin.Configs;
using NetFusion.Settings.Plugin;

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
            // Add dependent plug-ins:
            composite
                .AddSettings()
                .AddMessaging();
            
            // Add AMQP plugin:
            composite.AddPlugin<AmqpPlugin>();
            
            // Integrate with base messaging plugin:
            var dispatchConfig = composite.GetPluginConfig<MessageDispatchConfig>();
            dispatchConfig.AddPublisher<HostMessagePublisher>();
            
            return composite;
        }
    }
}
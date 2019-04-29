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
            SourceUrl = "https://github.com/grecosoft/NetFusion/tree/master/netfusion/src/Integration/NetFusion.Azure.Messaging";
            
            // Modules:
            AddModule<ConnectionModule>();
            AddModule<PublisherModule>();
            AddModule<SubscriberModule>();
        }
    }
    
    public static class CompositeBuilderExtensions
    {
        public static IComposeAppBuilder AddAmqp(this IComposeAppBuilder composite)
        {
            var dispatchConfig = composite.GetConfig<MessageDispatchConfig>();
            dispatchConfig.AddMessagePublisher<HostMessagePublisher>();
            
            composite.AddPlugin<AmqpPlugin>();
            return composite;
        }
    }
}
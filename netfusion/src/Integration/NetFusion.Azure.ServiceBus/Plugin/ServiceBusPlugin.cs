using NetFusion.Azure.ServiceBus.Plugin.Configs;
using NetFusion.Azure.ServiceBus.Plugin.Modules;
using NetFusion.Azure.ServiceBus.Publisher.Internal;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging.Plugin;
using NetFusion.Messaging.Plugin.Configs;
using NetFusion.Settings.Plugin;

namespace NetFusion.Azure.ServiceBus.Plugin
{
    public class ServiceBusPlugin : PluginBase
    {
        public override string PluginId => "2E8CE828-146B-4383-9A02-DB838A72B6A5";
        public override PluginTypes PluginType => PluginTypes.CorePlugin;
        public override string Name => "NetFusion: Azure Service Bus";
        
        public ServiceBusPlugin()
        {
            AddConfig<ServiceBusConfig>();
            
            AddModule<NamespaceModule>();
            AddModule<PublisherModule>();
            AddModule<SubscriberModule>();
            
            SourceUrl = "https://github.com/grecosoft/NetFusion/tree/master/netfusion/src/Integration/NetFusion.Azure.ServiceBus";
            DocUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.servicebus.overview#servicebus-overview";
        }
    }
    
    public static class CompositeBuilderExtensions
    {
        // Adds the Azure Service Bus plugin to the composite application.
        public static ICompositeContainerBuilder AddAzureServiceBus(this ICompositeContainerBuilder composite)
        {
            // Adds the dependent plugins to the composite application and registers
            // the Azure Service Bus publisher to the messaging pipeline.
            return composite
                .AddSettings()
                .AddMessaging()
                .AddPlugin<ServiceBusPlugin>()
                
                // Extend the base messaging pipeline by adding the ServiceBusPublisher.
                .InitPluginConfig<MessageDispatchConfig>(config => 
                    config.AddPublisher<ServiceBusPublisher>());
        }
    }
}
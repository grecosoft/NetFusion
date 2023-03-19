using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.Settings.Plugin;
using NetFusion.Integration.ServiceBus.Internal;
using NetFusion.Integration.ServiceBus.Plugin.Configs;
using NetFusion.Integration.ServiceBus.Plugin.Modules;
using NetFusion.Messaging.Plugin;
using NetFusion.Messaging.Plugin.Configs;

namespace NetFusion.Integration.ServiceBus.Plugin;

public class ServiceBusPlugin : PluginBase
{
    public override string PluginId => "2E8CE828-146B-4383-9A02-DB838A72B6A5";
    public override PluginTypes PluginType => PluginTypes.CorePlugin;
    public override string Name => "NetFusion: Azure Service Bus";
        
    public ServiceBusPlugin()
    {
        AddConfig<ServiceBusConfig>();
            
        AddModule<NamespaceModule>();
        AddModule<NamespaceEntityModule>();

        SourceUrl = "https://github.com/grecosoft/NetFusion/tree/master/netfusion/src/Integration/NetFusion.Integration.ServiceBus";
        DocUrl = "https://github.com/grecosoft/NetFusion/wiki/integration.servicebus.overview#servicebus-overview";
    }
}
    
public static class CompositeBuilderExtensions
{
    // Adds the Azure Service Bus plugin to the composite application.
    public static ICompositeContainerBuilder AddAzureServiceBus(this ICompositeContainerBuilder composite)
    {
        // Adds the Azure Service Bus and dependent plugins to the composite application.
        return composite
            .AddSettings()
            .AddMessaging()
            .AddPlugin<ServiceBusPlugin>()
                
            // Extend the base messaging pipeline by adding the ServiceBusPublisher.
            .InitPluginConfig<MessageDispatchConfig>(config => 
                config.AddPublisher<ServiceBusPublisher>());
    }
}
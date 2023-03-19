using Microsoft.Extensions.DependencyInjection;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Messaging.Enrichers;
using NetFusion.Messaging.Plugin.Configs;

namespace NetFusion.Messaging.Plugin.Modules;

/// <summary>
/// Registers all message enrichers with the service collection.
/// </summary>
public class MessageEnricherModule : PluginModule
{
    private MessageDispatchConfig? _messagingConfig;

    public override void Initialize()
    {
        _messagingConfig = Context.Plugin.GetConfig<MessageDispatchConfig>();
    }
        
    private MessageDispatchConfig MessagingConfig =>
        _messagingConfig ?? throw new NullReferenceException($"{nameof(MessagingConfig)} not initialized.");

    public override void RegisterServices(IServiceCollection services)
    {
        // Register all of the message enrichers with the container.
        foreach (var enricherType in MessagingConfig.MessageEnrichers)
        {
            services.AddScoped(typeof(IMessageEnricher), enricherType);
        }
    }

    public override void Log(IDictionary<string, object> moduleLog)
    {
        moduleLog["MessageEnrichers"] = Context.AllPluginTypes
            .Where(pt => pt.IsConcreteTypeDerivedFrom<IMessageEnricher>())
            .Select(et => new
            {
                EnricherType = et.AssemblyQualifiedName,
                IsConfigured = MessagingConfig.MessageEnrichers.Contains(et)
            }).ToArray();
    }
}
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Messaging.Filters;
using NetFusion.Messaging.Plugin.Configs;

namespace NetFusion.Messaging.Plugin.Modules;

/// <summary>
/// Module that manages filters to be invoked when a query is dispatched.
/// </summary>
public class MessageFilterModule : PluginModule
{
    private MessageDispatchConfig? _messagingConfig;

    public override void Initialize()
    {
        _messagingConfig = Context.Plugin.GetConfig<MessageDispatchConfig>();
    }

    private MessageDispatchConfig MessagingConfig =>
        _messagingConfig ?? throw new NullReferenceException($"{nameof(MessagingConfig)} not initialized.");
        
    // ---------------------- [Plugin Initialization] ----------------------

    // Registers the pre and post filters within the container so they can 
    // inject needed services.
    public override void RegisterServices(IServiceCollection services)
    {
        foreach (var messageFilterType in MessagingConfig.MessageFilters)
        {
            services.AddScoped(typeof(IMessageFilter), messageFilterType);
        }
    }
        
    // ---------------------- [Logging] ----------------------

    public override void Log(IDictionary<string, object> moduleLog)
    {
        moduleLog["QueryFilters"] = Context.AllPluginTypes
            .Where(pt => pt.IsConcreteTypeDerivedFrom<IMessageFilter>())
            .Select(ft => new
            {
                FilterType = ft.AssemblyQualifiedName,
                IsConfigured = MessagingConfig.MessageFilters.Contains(ft),
                IsPreFilter = ft.IsConcreteTypeDerivedFrom<IPreMessageFilter>(),
                IsPostFilter = ft.IsConcreteTypeDerivedFrom<IPostMessageFilter>()
            }).ToArray();      
    }
}
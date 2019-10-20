using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging.Internal;

namespace NetFusion.Messaging.Plugin.Modules
{
    /// <summary>
    /// The root messaging module registering the MessagingService that delegates
    /// to specific inner classes responsible for implementing the CQRS design pattern.
    /// </summary>
    public class MessagingModule : PluginModule
    {
        public override void RegisterDefaultServices(IServiceCollection services)
        {
            services.AddScoped<IMessagingService, MessagingService>();
            
            // The dispatcher delegated to by the MessagingService for sending commands and publishing domain-events.
            services.AddScoped<MessageDispatcher>();
            
            // The dispatcher delegated to by the MessagingService for dispatching queries.
            services.AddScoped<QueryDispatcher>();
        }
    }
}

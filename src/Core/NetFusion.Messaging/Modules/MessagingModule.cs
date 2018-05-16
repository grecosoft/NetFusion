using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging.Core;

namespace NetFusion.Messaging.Modules
{
    /// <summary>
    /// The root messaging module that registering the MessagingService that delegates
    /// to specific inner classes responsible for inplementing the CQRS design pattern.
    /// </summary>
    public class MessagingModule : PluginModule
    {
        // Registers a common service used to invoke the corresponding CQRS components.
        public override void RegisterDefaultServices(IServiceCollection services)
        {
            services.AddScoped<IMessagingService, MessagingService>();
            
            // The dispatcher delegated to by the MessagingService for sending commands and publishing domain-events.
            services.AddScoped<MessageDispatcher>();
            
            // The dispatcher delegated to by the MessagingService for dispatching commands.
            services.AddScoped<QueryDispatcher>();
        }
    }
}

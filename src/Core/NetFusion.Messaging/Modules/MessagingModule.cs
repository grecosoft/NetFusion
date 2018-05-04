using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging.Core;

namespace NetFusion.Messaging.Modules
{
    /// <summary>
    /// The root messaging module that registering the MessagingService that delegates
    /// to specific inner classes responsible for handling different message types.
    /// </summary>
    public class MessagingModule : PluginModule
    {
        // Register the common messaging service used to publish messages.
        public override void RegisterDefaultServices(IServiceCollection services)
        {
            services.AddScoped<IMessagingService, MessagingService>();
        }
    }
}

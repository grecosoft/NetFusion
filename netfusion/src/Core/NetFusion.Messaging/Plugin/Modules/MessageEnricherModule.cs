using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Enrichers;
using NetFusion.Messaging.Plugin.Configs;

namespace NetFusion.Messaging.Plugin.Modules
{
    /// <summary>
    /// Registers all message enrichers with the service collection.
    /// </summary>
    public class MessageEnricherModule : PluginModule
    {
        private MessageDispatchConfig MessagingConfig { get; set; }

        public override void Initialize()
        {
            MessagingConfig = Context.Plugin.GetConfig<MessageDispatchConfig>();
        }

        public override void RegisterServices(IServiceCollection services)
        {
            // Register all of the message enrichers with the container.
            foreach (var enricherType in MessagingConfig.EnricherTypes)
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
                  IsConfigured = MessagingConfig.EnricherTypes.Contains(et)
              }).ToArray();
        }
    }
}

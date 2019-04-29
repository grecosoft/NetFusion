using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Config;
using NetFusion.Messaging.Enrichers;

namespace NetFusion.Messaging.Plugin
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
            moduleLog["Message:Enrichers"] = Context.AllPluginTypes
              .Where(pt => pt.IsConcreteTypeDerivedFrom<IMessageEnricher>())
              .Select(et => new
              {
                  EnricherType = et.AssemblyQualifiedName,
                  IsConfigured = MessagingConfig.EnricherTypes.Contains(et)
              }).ToArray();
        }
    }
}

using Autofac;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Config;
using NetFusion.Messaging.Enrichers;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Messaging.Modules
{
    /// <summary>
    /// Registers all message enrichers with the dependency-injection container.
    /// </summary>
    public class MessageEnricherModule : PluginModule
    {
        private MessageDispatchConfig MessagingConfig { get; set; }

        public override void Initialize()
        {
            MessagingConfig = Context.Plugin.GetConfig<MessageDispatchConfig>();
        }

        public override void RegisterComponents(ContainerBuilder builder)
        {
            // Register all of the message enrichers with the container.
            builder.RegisterTypes(MessagingConfig.EnricherTypes)
                .As<IMessageEnricher>()
                .InstancePerLifetimeScope();
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
            moduleLog["Message Enrichers"] = Context.AllPluginTypes
              .Where(pt => {
                  return pt.IsConcreteTypeDerivedFrom<IMessageEnricher>();
              })
              .Select(et => new
              {
                  EnricherType = et.AssemblyQualifiedName,
                  IsConfigured = MessagingConfig.EnricherTypes.Contains(et)
              }).ToArray();
        }
    }
}

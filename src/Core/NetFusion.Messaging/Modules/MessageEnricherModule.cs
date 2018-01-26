using Autofac;
using NetFusion.Bootstrap.Plugins;
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
        private MessagingConfig MessagingConfig { get; set; }

        public override void Initialize()
        {
            MessagingConfig = Context.Plugin.GetConfig<MessagingConfig>();
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
            moduleLog["Message Enrichers"] = MessagingConfig.EnricherTypes
                .Select(t => new
                {
                    EnricherType = t.AssemblyQualifiedName
                }).ToArray();
        }
    }
}

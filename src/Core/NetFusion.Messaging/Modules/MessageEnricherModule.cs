using System.Collections.Generic;
using Autofac;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging.Config;
using NetFusion.Messaging.Enrichers;

namespace NetFusion.Messaging.Modules
{
    public class MessageEnricherModule : PluginModule
    {
        private MessagingConfig MessagingConfig { get; set; }

        public override void Initialize()
        {
            this.MessagingConfig = this.Context.Plugin.GetConfig<MessagingConfig>();
        }

        public override void RegisterComponents(ContainerBuilder builder)
        {
            // Register all of the message publishers that determine how a given
            // message is delivered.
            builder.RegisterTypes(this.MessagingConfig.EnricherTypes)
                .As<IMessageEnricher>()
                .InstancePerLifetimeScope();
        }

        public override void Log(IDictionary<string, object> moduleLog)
        {
           
        }
    }
}

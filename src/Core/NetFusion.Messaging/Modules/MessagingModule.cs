using Autofac;
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
        public override void RegisterDefaultComponents(ContainerBuilder builder)
        {
            // Register the common messaging service used to publish messages.
            builder.RegisterType<MessagingService>()
                .As<IMessagingService>()
                .InstancePerLifetimeScope();
        }
    }
}

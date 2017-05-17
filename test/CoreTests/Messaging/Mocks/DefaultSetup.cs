using Autofac;
using CoreTests.Messaging.Mocks;
using NetFusion.Bootstrap.Container;
using NetFusion.Domain.Scripting;
using NetFusion.Messaging;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;

namespace CoreTests.Messaging
{
    /// <summary>
    /// Provides a basic set up for testing publishing of domain-events.
    /// If a specific test requires additional configuration, it should
    /// define its own domain-event and consumer types.
    /// </summary>
    public static class DefaultSetup
    {
        /// <summary>
        /// Returns a configured container for testing the default configured
        /// event consumer.
        /// </summary>
        public static ContainerTest EventConsumer => ContainerSetup
            .Arrange((TestTypeResolver config) =>
            {
                config.AddPlugin<MockAppHostPlugin>()
                    .AddPluginType<MockDomainEvent>()
                    .AddPluginType<MockDomainEventConsumer>();

                config.AddPlugin<MockCorePlugin>()
                    .UseMessagingPlugin();
                            
            }, c =>
            {
                c.WithConfig<AutofacRegistrationConfig>(regConfig =>
                {
                    regConfig.Build = builder =>
                    {
                        builder.RegisterType<NullEntityScriptingService>()
                            .As<IEntityScriptingService>()
                            .SingleInstance();
                    };
                });
            });
    }
}

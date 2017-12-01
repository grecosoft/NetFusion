using Autofac;
using NetFusion.Base.Scripting;
using NetFusion.Bootstrap.Container;
using NetFusion.Messaging.Config;
using NetFusion.RabbitMQ;
using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Core;
using NetFusion.Test.Plugins;
using NetFusion.Testing.Logging;

namespace InfrastructureTests.RabbitMQ
{
    public static class TestFixtureExtensions
    {
        public static TestTypeResolver WithRabbitConfiguredHost(this TestTypeResolver resolver)
        {
            resolver.AddPlugin<MockAppHostPlugin>();

            resolver.AddPlugin<MockCorePlugin>()
                .UseRabbitMqPlugin();

            return resolver;
        }

        public static IAppContainer AddRabbitMqPublisher(this IAppContainer container)
        {
            container.WithConfig((MessagingConfig config) =>
            {
                config.AddMessagePublisher<RabbitMqMessagePublisher>();
            });

            return container;
        }

        public static IAppContainer UsingServices(this IAppContainer container, 
            BrokerSettings settings = null, 
            IMessageBroker broker = null)
        {
            container.UseTestLogger();

            container.WithConfig<AutofacRegistrationConfig>(config =>
            {
                config.Build = builder =>
                {
                    builder.RegisterType<NullEntityScriptingService>()
                        .As<IEntityScriptingService>()
                        .SingleInstance();

                    if (settings != null)
                    {
                        builder.RegisterInstance(settings)
                        .As<BrokerSettings>()
                        .SingleInstance();
                    }

                    if (broker != null)
                    {
                        builder.RegisterInstance(broker)
                        .As<IMessageBroker>()
                        .SingleInstance();
                    }
                };
            });

            return container;
        }
    }
}

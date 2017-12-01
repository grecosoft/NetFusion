using Autofac;
using NetFusion.Base.Scripting;
using NetFusion.Bootstrap.Container;
using NetFusion.Domain.Modules;
using NetFusion.Domain.Patterns.UnitOfWork;
using NetFusion.Messaging;
using NetFusion.Test.Plugins;
using System;

namespace DomainTests.UnitOfWork
{
    public static class TestFixtureExtensions
    {
        public static TestTypeResolver WithDispatchConfiguredHost(this TestTypeResolver resolver,
            params Type[] pluginTypes)
        {
            // Configure Core Plugin with messaging and the 
            // unit -of-work module.
            resolver.AddPlugin<MockCorePlugin>()
                .AddPluginType<UnitOfWorkModule>()
                .AddPluginType<EntityBehaviorModule>()
                .UseMessagingPlugin();

            // Add host plugin with the plugin-types to be used
            // for the unit-test.
            resolver.AddPlugin<MockAppHostPlugin>()
                .AddPluginType(pluginTypes);

            return resolver;
        }

        public static IAppContainer UsingDefaultServices(this IAppContainer container)
        {
            container.WithConfig<AutofacRegistrationConfig>(regConfig =>
            {
                regConfig.Build = builder =>
                {
                    builder.RegisterType<NullEntityScriptingService>()
                        .As<IEntityScriptingService>()
                        .SingleInstance();
                };
            });

            return container;
        }
    }
}

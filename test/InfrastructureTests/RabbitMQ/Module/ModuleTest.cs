using System;
using NetFusion.Bootstrap.Container;
using NetFusion.RabbitMQ;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using NetFusion.Testing.Logging;

namespace InfrastructureTests.RabbitMQ.Module
{
    public class ModuleTest
    {
        // Creates a configured mock plug-in with the needed RabbitMQ plug-in components.
        public static ContainerTest Arrange(Action<TestTypeResolver> config)
        {
            var resolver = new TestTypeResolver();
            AppContainer container = new AppContainer(resolver, setGlobalReference: false);

            resolver.AddPlugin<MockCorePlugin>()
                .UseRabbitMqPlugin();

            config(resolver);
            container.UseTestLogger();

            return new ContainerTest(container);
        }

        // Creates a configured mock plug-in with the needed RabbitMQ plug-in components.
        public static ContainerTest Arrange(Action<TestTypeResolver, IAppContainer> setup)
        {
            var resolver = new TestTypeResolver();
            AppContainer container = new AppContainer(resolver, setGlobalReference: false);

            container.UseTestLogger();

            resolver.AddPlugin<MockCorePlugin>()
                .UseRabbitMqPlugin();

            setup(resolver, container);

            return new ContainerTest(container);
        }

        public static IAppContainer CreateContainer()
        {
            var resolver = new TestTypeResolver();
            AppContainer container = new AppContainer(resolver, setGlobalReference: false);

            container.UseTestLogger();

            resolver.AddPlugin<MockCorePlugin>()
                .UseRabbitMqPlugin();

            return container;
        }
    }
}
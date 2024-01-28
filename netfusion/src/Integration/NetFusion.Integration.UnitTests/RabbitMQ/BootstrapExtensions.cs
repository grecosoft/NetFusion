using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Common.Base.Serialization;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Core.TestFixtures.Plugins;
using NetFusion.Integration.Bus.Entities;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Integration.UnitTests.Bus.Mocks;
using NetFusion.Integration.UnitTests.RabbitMQ.Mocks;
using NetFusion.Messaging.Plugin;

namespace NetFusion.Integration.UnitTests.RabbitMQ;

public static class BootstrapExtensions
{
    public static ContainerArrange TestRabbitMqBus(this ContainerArrange arrange)
    {
        var hostPlugin = new MockHostPlugin();
        hostPlugin.AddPluginType<TestRabbitRouter>();
        
        var pluginUnderTest = new MockCorePlugin();
        pluginUnderTest.AddModule<TestRabbitEntityModule>();
        pluginUnderTest.AddModule<TestRabbitBusModule>();

        arrange.Services(s => s.AddSingleton<ISerializationManager, TestSerializationManager>());
        arrange.Container(c =>
        {
            c.RegisterPlugins(hostPlugin, pluginUnderTest, new MessagingPlugin());
        });

        return arrange;
    }

    public static void AssertStrategies(this IEnumerable<IBusEntityStrategy> strategies,
        params Type[] expectedStrategyTypes)
    {
        var busEntityStrategies = strategies as IBusEntityStrategy[] ?? strategies.ToArray();
        busEntityStrategies.Should().HaveCount(expectedStrategyTypes.Length, "Number strategies incorrect");

        expectedStrategyTypes.Where(st => busEntityStrategies.Any(s => s.GetType() != st))
            .Should().BeEmpty("Incorrect strategies");
    }

    public static T GetBusEntity<T>(this TestRabbitEntityModule module, string entityName)
        where T : BusEntity
    {
        return GetBusEntity<T>(module.Entities, entityName);
    }
    
    public static T GetBusEntity<T>(this IEnumerable<BusEntity> busEntities, string entityName)
        where T : BusEntity
    {
        var entities = busEntities.OfType<T>().Where(e => e.EntityName == entityName).ToArray();
        if (entities.Length > 1)
        {
            throw new InvalidOperationException($"More than one bus entity of type {typeof(T)}");
        }

        if (entities.Length == 0)
        {
            throw new InvalidOperationException($"Bus entity of type {typeof(T)} not found");
        }

        return entities[0];
    }
}
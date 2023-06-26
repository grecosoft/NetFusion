using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.Bootstrap.Exceptions;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Core.TestFixtures.Extensions;
using NetFusion.Core.TestFixtures.Plugins;
using NetFusion.Integration.Bus;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Integration.UnitTests.Bus.Mocks;

namespace NetFusion.Integration.UnitTests.Bus;

public class BootstrapUnitTests
{
    /// <summary>
    /// When bootstrapped, all defined routes are located and instantiated to determine
    /// the set of configured entities corresponding to queue, topics, and subscriptions.
    /// The metadata is used during bootstrap to create bus entities and also during
    /// execution to publish and route messages to consumers. 
    /// </summary>
    [Fact]
    public void WhenBootstrapped_EntityPluginModule_FindsAllBusRouters()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.TestMessageBus()
                .Act.OnCompositeApp(a => a.Start())
                .Assert.PluginModule((TestEntityPluginModule m) =>
                {
                    m.TestBusMessageRouters.Should().HaveCount(1);
                    m.TestBusMessageRouters.First().Should().BeOfType<TestBusRouter>();
                });
        });
    }   

    /// <summary>
    /// When the microservice bootstraps, all the metadata used to create the corresponding
    /// broker entities on startup and how a given message is published and routed is cached.
    /// </summary>
    [Fact]
    public void WhenBootstrapped_EntityPluginModule_CachesAllBusEntities()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.TestMessageBus()
                .Act.OnCompositeApp(a => a.Start())
                .Assert.PluginModule((TestEntityPluginModule m) =>
                {
                    m.TestBusEntities.Should().HaveCount(2);
                    m.TestBusEntities.OfType<TestQueueBusEntity>().Should().HaveCount(1);
                    m.TestBusEntities.OfType<TestSubscriptionBusEntity>().Should().HaveCount(1);
                });
        });
    }

    /// <summary>
    /// A microservice can connect to multiple buses but there can be only one Bus-Router class
    /// defined for each broker.
    /// </summary>
    [Fact]
    public void WhenBootstrapped_MultipleRoutersPerBus_ExceptionRaised()
    {
        ContainerFixture.Test(fixture =>
        {
            var appPlugin = new MockAppPlugin();
            appPlugin.AddPluginType<DuplicateBusRouter>();  // Adds the same router twice so two have same bus name.
            
            fixture
                .Arrange.TestMessageBus(appPlugin)
                .Act.RecordException()
                .OnCompositeApp(a => a.Start())
             
                .Assert.Exception((BootstrapException ex) =>
                {
                    ex.InnerException.Should().NotBeNull();
                    ex.InnerException.Should().BeOfType<BusException>();
                    
                    ((BusException)ex.InnerException!).ExceptionId.Should().Be("DUPLICATE_BUS_ROUTERS");
                });
        });
    }

    /// <summary>
    /// When a microservice subscribes to a queue/topic and receives a message, a child
    /// lifetime scope is created in which the consumer is invoked to handle the message.
    /// </summary>
    [Fact]
    public void WhenBootstrapped_AllConsumers_RegisteredAsScopedServices()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.TestMessageBus()
                .Act.OnCompositeApp(a => a.Start())
                .Assert.ServiceCollection(sc =>
                {
                    sc.HasRegistration<TestCommandConsumer>(ServiceLifetime.Scoped);
                });
        });
    }

    /// <summary>
    /// A bus specific context class is set on every strategy providing access
    /// to common services and bus specific service implementations.
    /// </summary>
    [Fact]
    public void WhenBootstrapped_AllEntityStrategies_HaveContextSet()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.TestMessageBus()
                .Act.OnCompositeApp(a => a.Start())
                .Assert.PluginModule((TestEntityPluginModule m) =>
                {
                    m.AssertStrategiesOfType<TestQueueStrategy>(
                        strategy => strategy.StrategyContext.Should().NotBeNull());
                    
                    m.AssertStrategiesOfType<TestSubscriptionStrategy>(
                        strategy => strategy.StrategyContext.Should().NotBeNull());
                });
        });
    }

    /// <summary>
    /// Allows the microservice to create all defined bus entities such as queues/topics.
    /// </summary>
    [Fact]
    public void WhenStarted_AllEntityCreationStrategies_Executed()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.TestMessageBus()
                .Act.OnCompositeApp(a => a.Start())
                .Assert.PluginModule((TestEntityPluginModule m) =>
                {
                    m.AssertStrategiesOfType<TestQueueStrategy>(
                        strategy => strategy.CreationStrategyExecuted.Should().BeTrue());

                    m.AssertStrategiesOfType<TestSubscriptionStrategy>(
                        strategy => strategy.CreationStrategyExecuted.Should().BeTrue());
                });
        });
    }
    
    /// <summary>
    /// Allows the microservice to subscribe consumer message handlers to queue/topics.
    /// </summary>
    [Fact]
    public void WhenStarted_AllEntitySubscriptionStrategies_Executed()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.TestMessageBus()
                .Act.OnCompositeApp(a => a.Start())
                .Assert.PluginModule((TestEntityPluginModule m) =>
                {
                    m.AssertStrategiesOfType<TestSubscriptionStrategy>(
                        strategy => strategy.SubscriptionStrategyExecuted.Should().BeTrue());
                });
        });
    }
    
    /// <summary>
    /// A publish strategy is only executed when a message is published and occurs
    /// after the microservice is bootstrapped.
    /// </summary>
    [Fact]
    public void WhenStarted_AllEntityPublishStrategies_NotExecuted()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.TestMessageBus()
                .Act.OnCompositeApp(a => a.Start())
                .Assert.PluginModule((TestEntityPluginModule m) =>
                {
                    m.AssertStrategiesOfType<TestQueueStrategy>(
                        strategy => strategy.PublishStrategyExecuted.Should().BeFalse());
                });
        });
    }
    
    /// <summary>
    /// The only time Disposal strategies are call is when the microservice is stopped.
    /// </summary>
    [Fact]
    public void WhenStarted_AllEntityDisposalStrategies_NotExecuted()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.TestMessageBus()
                .Act.OnCompositeApp(a => a.Start())
                .Assert.PluginModule((TestEntityPluginModule m) =>
                {
                    m.AssertStrategiesOfType<TestSubscriptionStrategy>(
                        strategy => strategy.DisposalStrategyExecuted.Should().BeFalse());
                });
        });
    }
    
    /// <summary>
    /// When the microservice is stopped, all Disposal strategies are invoked.  This usually
    /// allows the service to unsubscribe from the given bus entity.
    /// </summary>
    [Fact]
    public void WhenStopped_AllEntityDisposalStrategies_Executed()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.TestMessageBus()
                .Act.OnCompositeApp(a =>
                {
                    a.Start();
                    a.Stop();
                })
                .Assert.PluginModule((TestEntityPluginModule m) =>
                {
                    m.AssertStrategiesOfType<TestSubscriptionStrategy>(
                        strategy => strategy.DisposalStrategyExecuted.Should().BeTrue());
                });
        });
    }
    
    /// <summary>
    /// When the plugin module is started, it invokes all of the Create strategies followed
    /// by the Subscribe strategies.  Based on the bus implementation (RabbitMQ, Azure Service
    /// Bus, Redis...), The create strategy will create the corresponding Queue/Topic and the
    /// subscription strategy will subscribe and route received message to the local consumer.
    /// </summary>
    [Fact]
    public void WhenBootstrapped_AllEntityStrategies_ExecutedInCorrectOrder()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.TestMessageBus()
                .Act.OnCompositeApp(a =>
                {
                    a.Start();
                    a.Stop();
                })
                .Assert.PluginModule((TestEntityPluginModule m) =>
                {
                    m.TestBusEntities.OfType<TestQueueBusEntity>().Single().InvokedStrategies
                        .Should()
                        .BeEquivalentTo(new List<string>{ "CreateEntity" });
                    
                    m.TestBusEntities.OfType<TestSubscriptionBusEntity>().Single().InvokedStrategies
                        .Should()
                        .BeEquivalentTo(new List<string>{ "CreateEntity", "SubscribeEntity", "OnDispose" });
                });
        });
    }

    /// <summary>
    /// When message published, a lookup is done to see if a publish-strategy exists for the type of message.
    /// Based on the bus implementation (RabbitMQ, Azure Service Bus, Redis...), the publish strategy is
    /// responsible for delivering message to the correct Queue or Topic.
    /// </summary>
    [Fact]
    public void AfterBootstrapped_EntityModule_ReturnsPublisherStrategy()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.TestMessageBus()
                .Act.OnCompositeApp(a => a.Start())
                .Assert.PluginModule((TestEntityPluginModule m) =>
                {
                    m.TryGetPublishEntityForMessage(typeof(TestCommand), out var entity).Should().BeTrue();
                    var publishStrategy = entity!.GetStrategies<IBusEntityPublishStrategy>().FirstOrDefault();
                    publishStrategy.Should().NotBeNull();
                });
        });
    }
}
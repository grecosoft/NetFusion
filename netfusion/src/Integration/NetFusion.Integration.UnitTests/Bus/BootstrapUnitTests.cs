using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.Bootstrap.Exceptions;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Core.TestFixtures.Extensions;
using NetFusion.Core.TestFixtures.Plugins;
using NetFusion.Integration.Bus;
using NetFusion.Integration.UnitTests.Bus.Mocks;

namespace NetFusion.Integration.UnitTests.Bus;

public class BootstrapUnitTests
{
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
    
    [Fact]
    public void WhenStopped_AllEntityDisposalStrategies_Executed()
    {
        
    }

    [Fact]
    public void AfterBootstrapped_EntityModule_ReturnsPublisherStrategy()
    {
        
    }
}
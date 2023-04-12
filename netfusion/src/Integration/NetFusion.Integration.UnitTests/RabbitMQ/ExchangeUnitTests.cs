using EasyNetQ;
using FluentAssertions;
using Moq;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Integration.RabbitMQ.Exchanges;
using NetFusion.Integration.RabbitMQ.Exchanges.Metadata;
using NetFusion.Integration.RabbitMQ.Exchanges.Strategies;
using NetFusion.Integration.RabbitMQ.Queues.Metadata;
using NetFusion.Integration.UnitTests.RabbitMQ.Mocks;

namespace NetFusion.Integration.UnitTests.RabbitMQ;

public class ExchangeUnitTests
{
    /// <summary>
    /// When a microservice notifies other services of changes, it publishes a domain-event.
    /// The following tests a routing specifying the metadata of the exchange to be created
    /// and the type of domain-event delivered when published. 
    /// </summary>
    [Fact]
    public void Publisher_Defines_Exchange()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.TestRabbitMqBus()
                .Assert.PluginModule((TestRabbitEntityModule m) =>
                {
                    var exchangeEntity = m.GetBusEntity<ExchangeEntity>("TestExchange");
                
                    exchangeEntity.BusName.Should().Be("testRabbitBus");
                    exchangeEntity.EntityName.Should().Be("TestExchange");
                    exchangeEntity.ExchangeMeta.Should().NotBeNull("Exchange metadata not set");
                    exchangeEntity.ExchangeMeta.PublishOptions.Should().NotBeNull();
                    
                    exchangeEntity.DomainEventType.Should().Be(typeof(TestDomainEvent), "Domain-Event type not set.");
                    exchangeEntity.Dispatchers.Should().BeEmpty("Dispatchers only set by subscribing microservice");
                    exchangeEntity.Strategies.AssertStrategies(typeof(ExchangeCreationStrategy));
                });
        });
    }

    /// <summary>
    /// When the microservice subscribing to the domain-event is bootstrapped, it specifies a routing
    /// indicating the topic and route-key to which a consumer should be subscribed.
    /// </summary>
    [Fact]
    public void Subscriber_Defines_QueueAndConsumer()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.TestRabbitMqBus()
                .Assert.PluginModule((TestRabbitEntityModule m) =>
                {
                    var subscriptionEntity = m.GetBusEntity<SubscriptionEntity>("ReceivedTestExchangeEvents");
                
                    subscriptionEntity.BusName.Should().Be("testRabbitBus");
                    subscriptionEntity.EntityName.Should().Be("ReceivedTestExchangeEvents");
                    subscriptionEntity.ExchangeName.Should().Be("TestExchange");
                    subscriptionEntity.Strategies.AssertStrategies(typeof(ExchangeSubscriptionStrategy));
                    subscriptionEntity.QueueMeta.Should().NotBeNull("Metadata for queue to bind to exchange not set");
                    subscriptionEntity.RouteKeys.Should().BeEquivalentTo("10.20", "50.100");
                    
                    subscriptionEntity.MessageDispatcher.Should().NotBeNull("Dispatcher not set");
                    subscriptionEntity.MessageDispatcher.MessageType.Should().Be(typeof(TestDomainEvent));
                    subscriptionEntity.MessageDispatcher.ConsumerType.Should().Be(typeof(TestDomainEventHandler));
                    subscriptionEntity.MessageDispatcher.MessageHandlerMethod.Name.Should().Be("OnDomainEvent");
                });
        });
    }

    /// <summary>
    /// Validates that the creation-strategy for the exchange entity under test
    /// results in the expected exchange and alternate exchange being created
    /// by the bus connection.
    /// </summary>
    [Fact]
    public void Publisher_Creates_Exchange()
    {
        // Arrange:
        var fixture = new EntityContextFixture();
        var router = new TestRabbitRouter();
        var exchangeEntity = router.DefinedEntities.GetBusEntity<ExchangeEntity>("TestExchange");
        var creationStrategies = exchangeEntity.GetStrategies<IBusEntityCreationStrategy>().ToArray();
        
        // Act:
        var strategy = creationStrategies.First();
        strategy.SetContext(fixture.CreateContext());
        strategy.CreateEntity();

        // Assert:
        fixture.MockConnection.Verify(m => m.CreateExchangeAsync(
            It.Is<ExchangeMeta>(v => v == exchangeEntity.ExchangeMeta)),
            Times.Once);
        
        fixture.MockConnection.Verify(m => m.CreateAlternateExchange(
            It.Is<string>(v => v == "AltTestExchange")), 
            Times.Once);
    }
    
    /// <summary>
    /// Validates that the publish-strategy for the exchange entity under test
    /// results in a message being published to the correct exchange by the bus
    /// connection.
    /// </summary>
    [Fact]
    public void Publisher_Publishes_ToExchange()
    {
        // Arrange:
        var fixture = new EntityContextFixture();
        var router = new TestRabbitRouter();
        var exchangeEntity = router.DefinedEntities.GetBusEntity<ExchangeEntity>("TestExchange");
        var creationStrategies = exchangeEntity.GetStrategies<IBusEntityPublishStrategy>().ToArray();
        var domainEvent = new TestDomainEvent(100, 200);

        fixture.MockSerializationMgr.Setup(m => m.Serialize(
            It.IsAny<object>(),
            It.IsAny<string>(),
            It.IsAny<string>())).Returns(new byte[] { 1, 2 });
        
        // Act:
        var strategy = creationStrategies.First();
        strategy.SetContext(fixture.CreateContext());
        strategy.SendToEntityAsync(domainEvent, default);
        
        // Assert:
        fixture.MockConnection.Verify(m => m.PublishToExchange(
            It.Is<string>(v => v == exchangeEntity.EntityName),
            It.Is<string>(v => v == "100.200"),
            It.Is<bool>(v => v == false), 
            It.IsAny<MessageProperties>(), 
            It.Is<byte[]>(v => v.Length == 2), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Validates that the subscription-strategy for the subscription entity under test results
    /// in the bus connection creating a queue that is bound to the correct exchange.
    /// </summary>
    [Fact]
    public void Subscriber_Creates_ExchangeBoundQueue()
    {
        // Arrange:
        var fixture = new EntityContextFixture();
        var router = new TestRabbitRouter();
        var exchangeEntity = router.DefinedEntities.GetBusEntity<SubscriptionEntity>("ReceivedTestExchangeEvents");
        var creationStrategies = exchangeEntity.GetStrategies<IBusEntityCreationStrategy>().ToArray();
        
        // Act:
        var strategy = creationStrategies.First();
        strategy.SetContext(fixture.CreateContext());
        strategy.CreateEntity();

        // Assert:
        fixture.MockConnection.Verify(m => m.CreateQueueAsync(
            It.Is<QueueMeta>(v => v == exchangeEntity.QueueMeta)), 
            Times.Once);

        fixture.MockConnection.Verify(m => m.BindQueueToExchange(
            It.Is<string>(v => v == exchangeEntity.EntityName),
            It.Is<string>(v => v == exchangeEntity.ExchangeName),
            It.Is<string[]>(v => v[0] == "10.20" && v[1] == "50.100")), 
            Times.Once);
    }

    /// <summary>
    /// Validates that the subscription-strategy for the subscription entity under test
    /// results in the bus connection consuming the correct queue.
    /// </summary>
    [Fact]
    public void Subscriber_Subscribes_ToExchangeBoundQueue()
    {
        // Arrange:
        var fixture = new EntityContextFixture();
        var router = new TestRabbitRouter();
        var exchangeEntity = router.DefinedEntities.GetBusEntity<SubscriptionEntity>("ReceivedTestExchangeEvents");
        var creationStrategies = exchangeEntity.GetStrategies<IBusEntitySubscriptionStrategy>().ToArray();
        
        // Act:
        var strategy = creationStrategies.First();
        strategy.SetContext(fixture.CreateContext());
        strategy.SubscribeEntity();
        
        // Assert:
        fixture.MockConnection.Verify(m => m.ConsumeQueue(
            It.Is<QueueMeta>(v => v == exchangeEntity.QueueMeta),
            It.IsAny<Func<byte[], MessageProperties, CancellationToken, Task>>()), 
            Times.Once);
    }

    [Fact]
    public void Subscriber_Dispatches_DomainEvent()
    {
        
    }
}
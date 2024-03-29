using EasyNetQ;
using FluentAssertions;
using Moq;
using NetFusion.Common.Base;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Integration.RabbitMQ.Exchanges;
using NetFusion.Integration.RabbitMQ.Exchanges.Metadata;
using NetFusion.Integration.RabbitMQ.Exchanges.Strategies;
using NetFusion.Integration.RabbitMQ.Queues.Metadata;
using NetFusion.Integration.UnitTests.RabbitMQ.Mocks;
using NetFusion.Messaging.Internal;
using JsonSerializer = System.Text.Json.JsonSerializer;
using IMessage = NetFusion.Messaging.Types.Contracts.IMessage;

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
                    exchangeEntity.ExchangeMeta.Should().NotBeNull("Metadata defines the exchange to be created");
                    exchangeEntity.ExchangeMeta.PublishOptions.Should().NotBeNull("Publish options used when publishing");
                    
                    exchangeEntity.DomainEventType.Should().Be(typeof(TestDomainEvent), "Domain-Events published to exchange");
                    exchangeEntity.Dispatchers.Should().BeEmpty("Dispatchers only set by subscribing microservice");
                    exchangeEntity.Strategies.AssertStrategies(typeof(ExchangeCreationStrategy));
                });
        });
    }

    /// <summary>
    /// When the microservice subscribing to an exchange is bootstrapped, it specifies a routing indicating 
    /// the topic and route-key used to bind to a consumer defined queue to which is subscribes.
    /// </summary>
    [Fact]
    public void Subscriber_DefinesTopicBound_QueueAndConsumer()
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
                    subscriptionEntity.QueueMeta.Should().NotBeNull("Metadata defines the queue to be created");
                    subscriptionEntity.RouteKeys.Should().BeEquivalentTo("10.20", "50.100");
                    
                    subscriptionEntity.MessageDispatcher.Should().NotBeNull("Dispatcher specifies consumer of queue");
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
            It.IsAny<CancellationToken>()), Times.Once, 
            "Published domain-event not published to expected exchange");
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
            Times.Once,
            "Queue was not created for exchange");

        fixture.MockConnection.Verify(m => m.BindQueueToExchange(
            It.Is<string>(v => v == exchangeEntity.EntityName),
            It.Is<string>(v => v == exchangeEntity.ExchangeName),
            It.Is<string[]>(v => v[0] == "10.20" && v[1] == "50.100")), 
            Times.Once, "Queue was not bound to exchange");
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
            Times.Once, "Queue bound to exchange was not consumed");
    }

    /// <summary>
    /// Validates when a domain-event message is received that it is dispatched
    /// to the correct consumer.
    /// </summary>
    [Fact]
    public async Task Subscriber_Dispatches_DomainEvent()
    {
        // Arrange:
        var fixture = new EntityContextFixture();
        var router = new TestRabbitRouter();
        var exchangeEntity = router.DefinedEntities.GetBusEntity<SubscriptionEntity>("ReceivedTestExchangeEvents");
        var creationStrategies = exchangeEntity.GetStrategies<ExchangeSubscriptionStrategy>().ToArray();
        
        var domainEvent = new TestDomainEvent(100, 200);
        var messageData = JsonSerializer.SerializeToUtf8Bytes(domainEvent);

        fixture.MockSerializationMgr
            .Setup(m => m.Deserialize(ContentTypes.Json, typeof(TestDomainEvent), messageData, null))
            .Returns(domainEvent);
        
        // Act:
        var strategy = creationStrategies.First();
        strategy.SetContext(fixture.CreateContext());
        await strategy.OnMessageReceived(messageData, new MessageProperties { ContentType = ContentTypes.Json}, default);
        
        // Assert:
        fixture.MockDispatcher.Verify(m => m.InvokeDispatcherInNewLifetimeScopeAsync(
            It.Is<MessageDispatcher>(d => d == exchangeEntity.MessageDispatcher),
            It.Is<IMessage>(msg => msg == domainEvent),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
using EasyNetQ;
using FluentAssertions;
using Moq;
using NetFusion.Common.Base;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Integration.Bus.Strategies;
using NetFusion.Integration.RabbitMQ.Queues;
using NetFusion.Integration.RabbitMQ.Queues.Metadata;
using NetFusion.Integration.RabbitMQ.Queues.Strategies;
using NetFusion.Integration.UnitTests.RabbitMQ.Mocks;
using NetFusion.Messaging.Internal;
using IMessage = NetFusion.Messaging.Types.Contracts.IMessage;

namespace NetFusion.Integration.UnitTests.RabbitMQ;

public class QueueUnitTests
{
    // ---------------------------------------------------------------------------
    // ----- MESSAGE PATTERN: Microservice defines queue to which other
    // microservices send commands for processing not expecting a response.
    // ---------------------------------------------------------------------------
    
    /// <summary>
    /// A microservice defines a queue to which other services can send commands for processing. 
    /// </summary>
    [Fact]
    public void Subscriber_Defines_QueueAndConsumer()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.TestRabbitMqBus()
                .Assert.PluginModule((TestRabbitEntityModule m) =>
                {
                    var queueEntity = m.GetBusEntity<QueueEntity>("TestQueue");
                    
                    queueEntity.BusName.Should().Be("testRabbitBus");
                    queueEntity.EntityName.Should().Be("TestQueue");
                    queueEntity.Strategies.AssertStrategies(typeof(QueueCreationStrategy));
                    queueEntity.QueueMeta.Should().NotBeNull("Queue Metadata defines queue to create");

                    queueEntity.MessageDispatcher.Should().NotBeNull("Dispatcher sends command to consumer");
                    queueEntity.MessageDispatcher.MessageType.Should().Be(typeof(TestCommand));
                    queueEntity.MessageDispatcher.ConsumerType.Should().Be(typeof(TestCommandHandler));
                    queueEntity.MessageDispatcher.MessageHandlerMethod.Name.Should().Be("OnCommand");
                });
        });
    }

    /// <summary>
    /// A microservice defines a queue to which a given command should be delivered when sent
    /// for processing by another service.
    /// </summary>
    [Fact]
    public void Publisher_Routes_CommandsToQueue()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.TestRabbitMqBus()
                .Assert.PluginModule((TestRabbitEntityModule m) =>
                {
                    var queueEntity = m.GetBusEntity<QueueReferenceEntity>("TestQueue");
                
                    queueEntity.BusName.Should().Be("testRabbitBus");
                    queueEntity.EntityName.Should().Be("TestQueue");
                    queueEntity.Strategies.AssertStrategies(typeof(QueuePublishStrategy));
                    queueEntity.PublishOptions.Should().NotBeNull("Publish options specify how command is delivered");
                });
        });
    }
    
    /// <summary>
    /// Microservice creates queue to which other microservices publish commands for processing.
    /// </summary>
    [Fact]
    public void Subscriber_Creates_Queue()
    {
        // Arrange:
        var fixture = new EntityContextFixture();
        var router = new TestRabbitRouter();
        var queueEntity = router.DefinedEntities.GetBusEntity<QueueEntity>("TestQueue");
        var creationStrategies = queueEntity.GetStrategies<IBusEntityCreationStrategy>().ToArray();
        
        // Act:
        var strategy = creationStrategies.First();
        strategy.SetContext(fixture.CreateContext());
        strategy.CreateEntity();

        // Assert:
        fixture.MockConnection.Verify(m => m.CreateQueueAsync(
                It.Is<QueueMeta>(v => v == queueEntity.QueueMeta)),
            Times.Once);
        
        fixture.MockConnection.Verify(m => m.CreateDeadLetterExchange(
                It.Is<string>(v => v == "DeadExchangeName")), 
            Times.Once);
    }
    
    /// <summary>
    /// Microservice consumes queue for processing commands sent by other microservices.
    /// </summary>
    [Fact]
    public void Subscriber_Subscribes_ToQueue()
    {
        // Arrange:
        var fixture = new EntityContextFixture();
        var router = new TestRabbitRouter();
        var queueEntity = router.DefinedEntities.GetBusEntity<QueueEntity>("TestQueue");
        var creationStrategies = queueEntity.GetStrategies<IBusEntitySubscriptionStrategy>().ToArray();
        
        // Act:
        var strategy = creationStrategies.First();
        strategy.SetContext(fixture.CreateContext());
        strategy.SubscribeEntity();
        
        // Assert:
        fixture.MockConnection.Verify(m => m.ConsumeQueue(
                It.Is<QueueMeta>(v => v == queueEntity.QueueMeta),
                It.IsAny<Func<byte[], MessageProperties, CancellationToken, Task>>()), 
            Times.Once);
    }

    /// <summary>
    /// When a command is sent by a microservice, the command is serialized and sent
    /// to the bus connection for delivery.  
    /// </summary>
    [Fact]
    public void Publisher_Publishes_ToQueue()
    {
        // Arrange:
        var fixture = new EntityContextFixture();
        var router = new TestRabbitRouter();
        var queueEntity = router.DefinedEntities.GetBusEntity<QueueReferenceEntity>("TestQueue");
        var creationStrategies = queueEntity.GetStrategies<IBusEntityPublishStrategy>().ToArray();
        
        fixture.MockSerializationMgr.Setup(m => m.Serialize(
            It.IsAny<object>(),
            It.IsAny<string>(),
            It.IsAny<string>())).Returns(new byte[] { 1, 2 });
        
        // Act:
        var strategy = creationStrategies.First();
        strategy.SetContext(fixture.CreateContext());
        strategy.SendToEntityAsync(new TestCommand(), default);
        
        // Assert:
        fixture.MockConnection.Verify(m => m.PublishToQueue(
            It.Is<string>(v => v == queueEntity.EntityName),
            It.Is<bool>(v => v == true), 
            It.IsAny<MessageProperties>(), 
            It.Is<byte[]>(v => v.Length == 2), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// When the subscribing microservice receives a command on it defined queue,
    /// it is dispatched to the specified consumer.
    /// </summary>
    [Fact]
    public async Task Subscriber_Dispatches_Command()
    {
        // Arrange:
        var fixture = new EntityContextFixture();
        var router = new TestRabbitRouter();
        var queueEntity = router.DefinedEntities.GetBusEntity<QueueEntity>("TestQueue");
        var creationStrategies = queueEntity.GetStrategies<QueueCreationStrategy>().ToArray();
        
        var command = new TestCommand();
        var messageData = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(command);

        fixture.MockSerializationMgr
            .Setup(m => m.Deserialize(ContentTypes.Json, typeof(TestCommand), messageData, null))
            .Returns(command);
        
        // Act:
        var strategy = creationStrategies.First();
        strategy.SetContext(fixture.CreateContext());
        await strategy.OnMessageReceived(messageData, new MessageProperties { ContentType = ContentTypes.Json}, default);
        
        // Assert:
        fixture.MockDispatcher.Verify(m => m.InvokeDispatcherInNewLifetimeScopeAsync(
            It.Is<MessageDispatcher>(d => d == queueEntity.MessageDispatcher),
            It.Is<IMessage>(msg => msg == command),
            It.IsAny<CancellationToken>()), Times.Once);
    }
    
    // ---------------------------------------------------------------------------
    // ----- MESSAGE PATTERN: Microservice defines queue to which other
    // microservices send commands for processing.  After the command
    // is processed, a response is returned to the original publishing
    // microservice on specified reply queue.
    // ---------------------------------------------------------------------------
    
    /// <summary>
    /// A microservice can define a queue with a consumer returning a response
    /// returned to the calling service on a reply queue.
    /// </summary>
    [Fact]
    public void Subscriber_Defines_QueueAndConsumerWithResponse()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.TestRabbitMqBus()
                .Assert.PluginModule((TestRabbitEntityModule m) =>
                {
                    var queueEntity = m.GetBusEntity<QueueEntity>("TestQueueWithResponse");

                    queueEntity.BusName.Should().Be("testRabbitBus");
                    queueEntity.EntityName.Should().Be("TestQueueWithResponse");
                    queueEntity.Strategies.AssertStrategies(typeof(QueueCreationStrategy));
                    queueEntity.QueueMeta.Should().NotBeNull("Metadata defines queue to be created");
                    
                    queueEntity.MessageDispatcher.Should().NotBeNull("Dispatcher sends command to consumer");
                    queueEntity.MessageDispatcher.MessageType.Should().Be(typeof(TestCommandWithResponse));
                    queueEntity.MessageDispatcher.ConsumerType.Should().Be(typeof(TestCommandHandlerWithResponse));
                    queueEntity.MessageDispatcher.MessageHandlerMethod.ReturnType.Should().Be(typeof(TestCommandResponse));
                    queueEntity.MessageDispatcher.MessageHandlerMethod.Name.Should().Be("OnCommand");
                });
        });
    }

    /// <summary>
    /// Publishing microservice specifies queue to which command should be sent.  Since the service also expects a
    /// response, they define the reply queue and the consumer to which the response is delivered.
    /// </summary>
    [Fact]
    public void Publisher_Routes_CommandsToQueueWithResponse()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.TestRabbitMqBus()
                .Assert.PluginModule((TestRabbitEntityModule m) =>
                {
                    var queueEntity = m.GetBusEntity<QueueReferenceEntity>("TestQueueWithResponse");
                
                    // Assert queue to which command is sent:
                    queueEntity.BusName.Should().Be("testRabbitBus");
                    queueEntity.EntityName.Should().Be("TestQueueWithResponse");
                    queueEntity.Strategies.AssertStrategies(typeof(QueuePublishStrategy));
                    queueEntity.PublishOptions.Should().NotBeNull();
                    
                    // Assert reply queue to which response is sent:
                    var replyQueueEntity = m.GetBusEntity<QueueEntity>("TestCommandReplyQueue");
                    replyQueueEntity.BusName.Should().Be("testRabbitBus");
                    replyQueueEntity.EntityName.Should().Be("TestCommandReplyQueue");
                    replyQueueEntity.Strategies.AssertStrategies(typeof(QueueCreationStrategy));
                    replyQueueEntity.MessageDispatcher.Should().NotBeNull("Dispatcher sends command to consumer");
                    replyQueueEntity.MessageDispatcher.MessageType.Should().Be(typeof(TestCommandResponse));
                    replyQueueEntity.MessageDispatcher.ConsumerType.Should().Be(typeof(TestReplyQueueHandler));
                    replyQueueEntity.MessageDispatcher.MessageHandlerMethod.Name.Should().Be("OnReply");
                });
        });
    }

    /// <summary>
    /// When a microservice sends a command to another microservice, it can specify
    /// that a response should be delivered to a reply queue that it creates.
    /// </summary>
    [Fact]
    public void Publisher_CanReceiveResponse_OnCreatedReplyQueue()
    {
        // Arrange:
        var fixture = new EntityContextFixture();
        var router = new TestRabbitRouter();
        var queueEntity = router.DefinedEntities.GetBusEntity<QueueEntity>("TestCommandReplyQueue");
        var creationStrategies = queueEntity.GetStrategies<IBusEntityCreationStrategy>().ToArray();
        
        // Act:
        var strategy = creationStrategies.First();
        strategy.SetContext(fixture.CreateContext());
        strategy.CreateEntity();
        
        // Assert:
        fixture.MockConnection.Verify(m => m.CreateQueueAsync(
                It.Is<QueueMeta>(v => v == queueEntity.QueueMeta)),
            Times.Once);

    }
    
    /// <summary>
    /// when a microservice sends a command to another microservice, it can specify
    /// that a response should be delivered to a reply queue to which it consumes.
    /// </summary>
    [Fact]
    public void Publisher_ConsumesReplyQueue_ToReceiveResponse()
    {
        // Arrange:
        var fixture = new EntityContextFixture();
        var router = new TestRabbitRouter();
        var queueEntity = router.DefinedEntities.GetBusEntity<QueueEntity>("TestCommandReplyQueue");
        var creationStrategies = queueEntity.GetStrategies<IBusEntitySubscriptionStrategy>().ToArray();
        
        // Act:
        var strategy = creationStrategies.First();
        strategy.SetContext(fixture.CreateContext());
        strategy.SubscribeEntity();
        
        // Assert:
        fixture.MockConnection.Verify(m => m.ConsumeQueue(
                It.Is<QueueMeta>(v => v == queueEntity.QueueMeta),
                It.IsAny<Func<byte[], MessageProperties, CancellationToken, Task>>()), 
            Times.Once);
    }

    /// <summary>
    /// Validates that that when a subscriber's queue consumer returns a response
    /// and the publisher of the command specified a reply queue, that the response
    /// message is published to the specified reply queue.
    /// </summary>
    [Fact]
    public async Task Subscriber_SendsResponse_OnReplyQueue()
    {
        // Arrange:
        var fixture = new EntityContextFixture();
        var router = new TestRabbitRouter();
        var queueEntity = router.DefinedEntities.GetBusEntity<QueueEntity>("TestQueue");
        var creationStrategies = queueEntity.GetStrategies<QueueCreationStrategy>().ToArray();
        
        var command = new TestCommandWithResponse();
        var response = new TestCommandResponse();
        var messageData = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(command);

        fixture.MockSerializationMgr
            .Setup(m => m.Deserialize(ContentTypes.Json, typeof(TestCommand), messageData, null))
            .Returns(command);

        fixture.MockDispatcher.Setup(m => m.InvokeDispatcherInNewLifetimeScopeAsync(
            It.Is<MessageDispatcher>(d => d == queueEntity.MessageDispatcher),
            It.Is<IMessage>(msg => msg == command),
            It.IsAny<CancellationToken>())).ReturnsAsync(response);

        // Act:
        var strategy = creationStrategies.First();
        strategy.SetContext(fixture.CreateContext());
        
        await strategy.OnMessageReceived(messageData,
            new MessageProperties { ContentType = ContentTypes.Json, ReplyTo = "testBus:TestReplyQueue"},
            default);
        
        // Assert:
        fixture.MockConnection.Verify(m => m.PublishToQueue(
            It.Is<string>(v => v == "TestReplyQueue"),
            It.Is<bool>(v => v == false), 
            It.IsAny<MessageProperties>(), 
            It.IsAny<byte[]>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
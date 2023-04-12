using FluentAssertions;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Integration.RabbitMQ.Queues;
using NetFusion.Integration.RabbitMQ.Queues.Strategies;
using NetFusion.Integration.UnitTests.RabbitMQ.Mocks;

namespace NetFusion.Integration.UnitTests.RabbitMQ;

public class QueueUnitTests
{
    // ----- MESSAGE PATTERN: Microservice defines queue to which other
    // microservices send commands for processing.
    
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
                    queueEntity.QueueMeta.Should().NotBeNull("Queue Metadata should be set");

                    queueEntity.MessageDispatcher.Should().NotBeNull("Dispatcher not set");
                    queueEntity.MessageDispatcher.MessageType.Should().Be(typeof(TestCommand));
                    queueEntity.MessageDispatcher.ConsumerType.Should().Be(typeof(TestCommandHandler));
                    queueEntity.MessageDispatcher.MessageHandlerMethod.Name.Should().Be("OnCommand");
                });
        });
    }

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
                    queueEntity.PublishOptions.Should().NotBeNull();
                });
        });
    }

    // ----- MESSAGE PATTERN: Microservice defines queue to which other
    // microservices send commands for processing.  After the command
    // is processed, a response is returned to the original publishing
    // microservice on specified reply queue.
    
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
                    queueEntity.QueueMeta.Should().NotBeNull("Queue Metadata should be set");
                    
                    queueEntity.MessageDispatcher.Should().NotBeNull("Dispatcher not set");
                    queueEntity.MessageDispatcher.MessageType.Should().Be(typeof(TestCommandWithResponse));
                    queueEntity.MessageDispatcher.ConsumerType.Should().Be(typeof(TestCommandHandlerWithResponse));
                    queueEntity.MessageDispatcher.MessageHandlerMethod.Name.Should().Be("OnCommand");
                });
        });
    }

    [Fact]
    public void Publisher_Routes_CommandsToQueueWithResponse()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange.TestRabbitMqBus()
                .Assert.PluginModule((TestRabbitEntityModule m) =>
                {
                    var queueEntity = m.GetBusEntity<QueueReferenceEntity>("TestQueueWithResponse");
                
                    queueEntity.BusName.Should().Be("testRabbitBus");
                    queueEntity.EntityName.Should().Be("TestQueueWithResponse");
                    queueEntity.Strategies.AssertStrategies(typeof(QueuePublishStrategy));
                    queueEntity.PublishOptions.Should().NotBeNull();
                    
                    var replyQueueEntity = m.GetBusEntity<QueueEntity>("TestCommandReplyQueue");
                    replyQueueEntity.MessageDispatcher.Should().NotBeNull("Dispatcher not set");
                    replyQueueEntity.MessageDispatcher.MessageType.Should().Be(typeof(TestCommandResponse));
                    replyQueueEntity.MessageDispatcher.ConsumerType.Should().Be(typeof(TestReplyQueueHandler));
                    replyQueueEntity.MessageDispatcher.MessageHandlerMethod.Name.Should().Be("OnReply");
                });
        });
    }
}
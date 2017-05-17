using FluentAssertions;
using Moq;
using NetFusion.Domain.Messaging;
using NetFusion.Messaging;
using NetFusion.Messaging.Core;
using NetFusion.RabbitMQ;
using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Consumers;
using NetFusion.Tests.Infrastructure.RabbitMQ.Mocks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace InfrastructureTests.RabbitMQ.Broker
{
    public class MessageBrokerTests
    {
        /// <summary>
        /// An exchange declaration specifies the RabbitMq server on which it should 
        /// be created by setting the BrokerName property.  The value of this property
        /// must existing in the set of configured connections.
        /// </summary>
        [Fact(DisplayName = nameof(ExchangeBrokerNameNotConfigured_Exception))] 
        public void ExchangeBrokerNameNotConfigured_Exception()
        {
            var emptySettings = new BrokerSettings();
            var stateSetup = BrokerTest.SetupBrokerState(emptySettings);
            var brokerSetup = BrokerTest.SetupMessageBroker();
            var exchange = new MockExchange();
           
            exchange.InitializeSettings();
            stateSetup.BrokerState.Exchanges = new[] { exchange };
            stateSetup.BrokerState.BrokerSettings.Connections.Clear();

            Assert.Throws<BrokerException>(() => brokerSetup.Initialize(stateSetup.BrokerState));
        }

        /// <summary>
        /// A consumer can determine if a given type of message is associated with an 
        /// exchange.  If the message is associated with an exchange, the message will
        /// be delivered to that exchange when published using the common IMessagingService
        /// implementation.
        /// </summary>
        [Fact (DisplayName = nameof(Event_AssociatedWith_Exchange))]
        public void Event_AssociatedWith_Exchange()
        {
            var brokerSetup = BrokerTest.SetupMessageBroker();
            var exchange = new MockExchange();
            var stateSetup = BrokerTest.SetupBrokerState();

            // Setup of test DirectExchange instance that can be asserted.
            exchange.InitializeSettings();
            stateSetup.BrokerState.Exchanges = new[] { exchange };

            brokerSetup.Initialize(stateSetup.BrokerState);

            var msg = new MockDomainEvent();
            brokerSetup.Instance.IsExchangeMessage(msg).Should().BeTrue();
        }

        /// <summary>
        /// For each defined exchange contained within the meta-data a call should
        /// be made to the RabbitMq server to define the exchange.
        /// </summary>
        [Fact(DisplayName = nameof(DiscoveredExchange_CreatedByBroker))]
        public void DiscoveredExchange_CreatedByBroker()
        {
            var brokerSetup = BrokerTest.SetupMessageBroker();
            var exchange = new MockExchange { IsDurable = true, IsAutoDelete = true };
            var stateSetup = BrokerTest.SetupBrokerState();

            // Setup of test DirectExchange instance that can be asserted.
            exchange.InitializeSettings();
            stateSetup.BrokerState.Exchanges = new[] { exchange };

            // Assert that the RabbitMq client was called with the correct values:
            BrokerTest.AssertDeclaredExchange(stateSetup.MockConnMgr.MockChannel, v => {

                v.ExchangeName.Should().Be("MockDirectExchangeName");
                v.AutoDelete.Should().BeTrue();
                v.Durable.Should().BeTrue();
                v.Type.Should().Be("direct");
            });

            brokerSetup.Initialize(stateSetup.BrokerState);
        }

        /// <summary>
        /// For each exchange defined queue contained within the meta-data a call should
        /// be made to the RabbitMq server to define the queue.
        /// </summary>
        [Fact (DisplayName = nameof(DiscoveredQueue_CreatedByBroker))]
        public void DiscoveredQueue_CreatedByBroker()
        {
            var brokerSetup = BrokerTest.SetupMessageBroker();
            var exchange = new MockExchange();
            var stateSetup = BrokerTest.SetupBrokerState();

            // Setup of test DirectExchange instance that can be asserted.
            exchange.InitializeSettings();
            stateSetup.BrokerState.Exchanges = new[] { exchange };

            BrokerTest.AssertDeclaredQueue(stateSetup.MockConnMgr.MockChannel, v =>
            {
                v.QueueName.Should().Be("MockTestQueueName");
                v.AutoDelete.Should().BeFalse();
                v.Exclusive.Should().BeFalse();
                v.Durable.Should().BeTrue();
            });

            brokerSetup.Initialize(stateSetup.BrokerState);
        }

        /// <summary>
        /// A consumer of a delivered RabbitMq message is the same as any other message consumer.
        /// The only difference is that the class with the message handler must be decorated with
        /// the Broker attribute.  This specifies the broker connection on which the message handler
        /// method should be bound.  When a consumer joins an existing exchange queue, the handler
        /// method will be invoked round-robin with all other consumer bound methods defined in 
        /// all connected clients.
        /// </summary>
        [Fact (DisplayName = nameof(ConsumerJoinsNonFanoutQueue_NewBindingCreatedForConsumer))] 
        public void ConsumerJoinsNonFanoutQueue_NewBindingCreatedForConsumer()
        {
            var brokerSetup = BrokerTest.SetupMessageBroker();
            var exchange = new MockExchange();
            var stateSetup = BrokerTest.SetupBrokerState();
            var msgConsumer = BrokerTest.SetupJoiningConsumer();

            // Setup of test DirectExchange instance that can be asserted.
            exchange.InitializeSettings();
            stateSetup.BrokerState.Exchanges = new[] { exchange };

            // Initialize the broker.
            brokerSetup.Initialize(stateSetup.BrokerState, msgConsumer);

            // Assert call made to the RabbitMq client:
            stateSetup.MockConnMgr.MockChannel.Verify(m => m.QueueBind(
                It.Is<string>(v => v == "MockTestQueueName"), 
                It.Is<string>(v => v == "MockDirectExchangeName"), 
                It.Is<string>(v => v == ""), 
                It.IsAny<IDictionary<string, object>>()), Times.Once());
        }

        /// <summary>
        /// A consumer of a delivered RabbitMq message is the same as any other message consumer.
        /// The only difference is that the class with the message handler must be decorated with
        /// the Broker attribute.  This specifies the broker connection on which the message handler
        /// method should be bound.  When a consumer adds a queue to an existing exchange, the handler
        /// method will be invoked for each message delivered to the queue. 
        /// </summary>
        [Fact (DisplayName = nameof(ConsumerAddsQueue_NewQueueCreatedExclusivelyForConsumer))] 
        public void ConsumerAddsQueue_NewQueueCreatedExclusivelyForConsumer()
        {
            var brokerSetup = BrokerTest.SetupMessageBroker();
            var exchange = new MockExchange();
            var stateSetup = BrokerTest.SetupBrokerState();
            var msgConsumer = BrokerTest.SetupExclusiveConsumer();

            // Setup of test DirectExchange instance that can be asserted.
            exchange.InitializeSettings();
            stateSetup.BrokerState.Exchanges = new[] { exchange };

            // Records all calls made to the RabbitMq client:
            var clientCalls = new List<BrokerTest.DeclarationValues>();
            BrokerTest.AssertDeclaredQueue(stateSetup.MockConnMgr.MockChannel, v =>
            {
                clientCalls.Add(v);
            });

            // Initialize the broker.
            brokerSetup.Initialize(stateSetup.BrokerState, msgConsumer);

            // Assert call made to the RabbitMq client:
            // An exclusive queue is created for the consumer:
            clientCalls.Should().HaveCount(2);
            var addedQueueValuses = clientCalls.FirstOrDefault(c => c.Exclusive);

            addedQueueValuses.Should().NotBeNull();
            addedQueueValuses.QueueName.Should().Be("MockTestQueueName");

            // After creating exclusive queue, the consumer is bound:
            stateSetup.MockConnMgr.MockChannel.Verify(m => m.QueueBind(
                It.Is<string>(v => v == "MockTestQueueName"),
                It.Is<string>(v => v == "MockDirectExchangeName"),
                It.Is<string>(v => v == "MOCKKEY"), 
                It.IsAny<IDictionary<string, object>>()), Times.Once());
        }
    
        // If the queue requires it's delivered messages to be acknowledged the message header
        // is checked.  The consumer event handler specifies if the message is acknowledged by 
        // calling a method that stores an indicator in the header of the message.  If message
        // is acknowledged, RabbitMq is notified.
        [Fact (DisplayName = nameof(MessageDeliveryRequiresAck_AndConsumerAck_RabbitNotifiedBasicAck))] 
        public void MessageDeliveryRequiresAck_AndConsumerAck_RabbitNotifiedBasicAck()
        {
            var stateSetup = BrokerTest.SetupBrokerState();
            var brokerSetup = BrokerTest.SetupMessageBroker();

            var exchange = new MockExchange();            
            var msgConsumer = BrokerTest.SetupExclusiveConsumer();
            var domainEvt = new MockDomainEvent();

            // Setup of test DirectExchange instance that can be asserted.
            exchange.InitializeSettings();
            stateSetup.BrokerState.Exchanges = new[] { exchange };

            // Initialize the broker.
            brokerSetup.Initialize(stateSetup.BrokerState, msgConsumer);

            // Provide a mock consumer that acknowledges the received message,
            brokerSetup.MockMsgModule.Setup(m => m.InvokeDispatcherAsync(
                It.IsAny<MessageDispatchInfo>(), 
                It.IsAny<IMessage>(), 
                It.IsAny<CancellationToken>()))

                .Returns((MessageDispatchInfo di, IMessage m, CancellationToken t) => {
                    m.SetAcknowledged();
                    return Task.FromResult<object>(null);
                });

            // Simulate receiving of a message and verify that the RabbitMq
            // client was called correctly.
            brokerSetup.SimulateMessageReceived(domainEvt);
            stateSetup.MockConnMgr.MockChannel.Verify(m => m.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once());
        }

        // If the queue requires it's delivered messages to be acknowledged the message header
        // is checked.  The consumer event handler specifies if the message is rejected by 
        // calling a method that stores an indicator in the header of the message.  Or simply
        // by not acknowledging the message.  If message is not-acknowledged, RabbitMq is notified.
        [Fact( DisplayName = nameof(MessageDeliveryRequiresAck_AndConsumerNoAck_RabbitNotifiedBasicReject))]
        public void MessageDeliveryRequiresAck_AndConsumerNoAck_RabbitNotifiedBasicReject()
        {
            var stateSetup = BrokerTest.SetupBrokerState();
            var brokerSetup = BrokerTest.SetupMessageBroker();

            var exchange = new MockExchange();
            var msgConsumer = BrokerTest.SetupExclusiveConsumer();
            var domainEvt = new MockDomainEvent();

            // Setup of test DirectExchange instance that can be asserted.
            exchange.InitializeSettings();
            stateSetup.BrokerState.Exchanges = new[] { exchange };

            // Initialize the broker.
            brokerSetup.Initialize(stateSetup.BrokerState, msgConsumer);

            // Provide a mock consumer that rejects the received message,
            brokerSetup.MockMsgModule.Setup(m => m.InvokeDispatcherAsync(
                It.IsAny<MessageDispatchInfo>(),
                It.IsAny<IMessage>(), 
                It.IsAny<CancellationToken>()))
                .Returns((MessageDispatchInfo di, IMessage m) => {
                    m.SetRejected();
                    return Task.FromResult<object>(null);
                });


            // Simulate receiving of a message and verify that the RabbitMq
            // client was called correctly.
            brokerSetup.SimulateMessageReceived(domainEvt);
            stateSetup.MockConnMgr.MockChannel.Verify(m => m.BasicReject(It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once());
        }
    }

    [Broker("MockTestBrokerName")]
    public class MockBindingConsumer : IMessageConsumer
    {
        [JoinQueue("MockTestQueueName", "MockDirectExchangeName")]
        public void OnJoin(MockDomainEvent evt)
        {

        }

        [AddQueue("MockDirectExchangeName", RouteKey = "#",
            IsAutoDelete = true, IsExclusive = true, IsNoAck = true)]
        public void OnAdd(MockDomainEvent evt)
        {

        }
    }
}

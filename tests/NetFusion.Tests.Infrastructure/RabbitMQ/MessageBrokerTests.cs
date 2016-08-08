using FluentAssertions;
using Moq;
using NetFusion.Bootstrap.Logging;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Messaging;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Modules;
using NetFusion.RabbitMQ;
using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Consumers;
using NetFusion.RabbitMQ.Core;
using NetFusion.RabbitMQ.Serialization;
using NetFusion.Tests.Infrastructure.RabbitMQ.Mocks;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NetFusion.Tests.Infrastructure.RabbitMQ
{
    public class MessageBrokerTests
    {
        /// <summary>
        /// An exchange declaration specifies the RabbitMq server on which it should 
        /// be created by setting the BrokerName property.  The value of this property
        /// must existing in the set of configured collections.
        /// </summary>
        [Fact]
        public void ExceptionIfExchangeBrokerNameNoConfigured()
        {
            var metadata = GetBrokerMetadata();
            var mockMsgModule = new Mock<IMessagingModule>();
            var mockConn = new Mock<IConnection>();
            var exchange = new MockExchange();

            exchange.InitializeSettings();
            metadata.Exchanges = new[] { exchange };
            metadata.Connections.Clear();

            var messageBroker = new MockMessageBroker(mockMsgModule, mockConn);
            messageBroker.Initialize(metadata);

            Assert.Throws<InvalidOperationException>(() => messageBroker.DefineExchanges());
        }

        /// <summary>
        /// A consumer can determine if a given type of message is associated with an 
        /// exchange.  If the message is associated with an exchange, the message will
        /// be delivered to that exchange when published using the common IMessagingService
        /// implementation.
        /// </summary>
        [Fact]
        public void CanDetermineIfEventAssociatedWithExchange()
        {
            var metadata = GetBrokerMetadata();
            var mockMsgModule = new Mock<IMessagingModule>();
            var mockConn = new Mock<IConnection>();
            var exchange = new MockExchange();

            exchange.InitializeSettings();
            metadata.Exchanges = new[] { exchange };

            var messageBroker = new MockMessageBroker(mockMsgModule, mockConn);
            messageBroker.Initialize(metadata);

            var msg = new MockDomainEvent();
            messageBroker.IsExchangeMessage(msg).Should().BeTrue();
        }

        /// <summary>
        /// For each defined exchange contained withing the meta-data a call should
        /// be made to the RabbitMq server to define the exchange.
        /// </summary>
        [Fact]
        public void DiscoveredExchangeIsCreatedOnRabbitServer()
        {
            var metadata = GetBrokerMetadata();
            var mockMsgModule = new Mock<IMessagingModule>();
            var mockConn = new Mock<IConnection>();
            var mockModule = new Mock<IModel>();
            var exchange = new MockExchange { IsDurable = true, IsAutoDelete = true };

            mockConn.Setup(c => c.CreateModel()).Returns(mockModule.Object);

            // Setup of test DirectExchange instance that can be asserted.
            exchange.InitializeSettings();
            metadata.Exchanges = new[] { exchange };

            // Initialize the broker with the meta-data.
            var messageBroker = new MockMessageBroker(mockMsgModule, mockConn);
            messageBroker.Initialize(metadata);

            // Assert that the RabbitMq client was called with the correct values:
            AssertDeclaredExchange(mockModule, v => {

                v.ExchangeName.Should().Be("MockDirectExchangeName");
                v.AutoDelete.Should().BeTrue();
                v.Durable.Should().BeTrue();
                v.Type.Should().Be("direct");
            });

            messageBroker.DefineExchanges();

        }

        /// <summary>
        /// For each exchange defined queue contained withing the meta-data a call should
        /// be made to the RabbitMq server to define the queue.
        /// </summary>
        [Fact]
        public void DiscoveredQueueIsCreatedOnRabbitServer()
        {
            var metadata = GetBrokerMetadata();
            var mockMsgModule = new Mock<IMessagingModule>();
            var mockConn = new Mock<IConnection>();
            var mockModule = new Mock<IModel>();
            var exchange = new MockExchange();

            mockConn.Setup(c => c.CreateModel()).Returns(mockModule.Object);

            // Setup of test DirectExchange instance that can be asserted.
            exchange.InitializeSettings();
            metadata.Exchanges = new[] { exchange };

            // Initialize the broker with the meta-data.
            var messageBroker = new MockMessageBroker(mockMsgModule, mockConn);
            messageBroker.Initialize(metadata);

            AssertDeclaredQueue(mockModule, v =>
            {
                v.QueueName.Should().Be("MockTestQueueName");
                v.AutoDelete.Should().BeFalse();
                v.Exclusive.Should().BeFalse();
                v.Durable.Should().BeTrue();
            });

            messageBroker.DefineExchanges();
        }

        /// <summary>
        /// A consumer of a delivered RabbitMq message is the same as any other message consumer.
        /// The only difference is that the class with the message handler must be decorated with
        /// the Broker attribute.  This specifies the broker connection on which the message handler
        /// method should be bound.  When a consumer joins an existing exchange queue, the handler
        /// method will be invoked round-robin with all other consumer bound methods defined in 
        /// all connected clients.
        /// </summary>
        [Fact]
        public void IfConsumerJoinsNonFanoutQueueNewBindingCreatedForConsumer()
        {
            var metadata = GetBrokerMetadata();
            var mockMsgModule = new Mock<IMessagingModule>();
            var mockConn = new Mock<IConnection>();
            var mockModule = new Mock<IModel>();
            var exchange = new MockExchange();

            mockConn.Setup(c => c.CreateModel()).Returns(mockModule.Object);

            // Setup of test DirectExchange instance that can be asserted.
            exchange.InitializeSettings();
            metadata.Exchanges = new[] { exchange };

            // Initialize the broker with the meta-data and specify
            // a joining consumer.
            var consumer = new MessageConsumer(
                new BrokerAttribute("MockTestBrokerName"), 
                new JoinQueueAttribute("MockTestQueueName", "MockDirectExchangeName"), 
                new MessageDispatchInfo());

            // This simulates the calls made by the module during bootstrapping:
            var messageBroker = new MockMessageBroker(mockMsgModule, mockConn);
            messageBroker.Initialize(metadata);
            messageBroker.DefineExchanges();
            messageBroker.BindConsumers(new[] { consumer });

            // Assert call made to the RabbitMq client:
            mockModule.Verify(m => m.QueueBind(
                It.Is<string>(v => v == "MockTestQueueName"), 
                It.Is<string>(v => v == "MockDirectExchangeName"), 
                It.Is<string>(v => v == "")), Times.Once());
        }

        /// <summary>
        /// A consumer of a delivered RabbitMq message is the same as any other message consumer.
        /// The only difference is that the class with the message handler must be decorated with
        /// the Broker attribute.  This specifies the broker connection on which the message handler
        /// method should be bound.  When a consumer adds a queue to an existing exchange, the handler
        /// method will be invoked for each message delivered to the queue. 
        /// </summary>
        [Fact]
        public void IfConsumerAddsQueueNewQueueIsCreatedExclusivelyForConsumer()
        {
            var metadata = GetBrokerMetadata();
            var mockMsgModule = new Mock<IMessagingModule>();
            var mockConn = new Mock<IConnection>();
            var mockModule = new Mock<IModel>();
            var exchange = new MockExchange();

            mockConn.Setup(c => c.CreateModel()).Returns(mockModule.Object);

            // Setup of test DirectExchange instance that can be asserted.
            exchange.InitializeSettings();
            metadata.Exchanges = new[] { exchange };

            var consumer = SetupMessageConsumer();

            // Records all calls made to the RabbitMq client:
            var clientCalls = new List<DeclarationValues>();
            AssertDeclaredQueue(mockModule, v =>
            {
                clientCalls.Add(v);
            });

            // This simulates the calls made by the module during bootstrapping:
            var messageBroker = new MockMessageBroker(mockMsgModule, mockConn);
            messageBroker.Initialize(metadata);
            messageBroker.DefineExchanges();
            messageBroker.BindConsumers(new[] { consumer });

            // Assert call made to the RabbitMq client:
            // An exclusive queue is created for the consumer:
            clientCalls.Should().HaveCount(2);
            var addedQueueValuses = clientCalls.FirstOrDefault(c => c.Exclusive);

            addedQueueValuses.Should().NotBeNull();
            addedQueueValuses.QueueName.Should().Be("MockTestQueueName");

            // After creating exclusive queue, the consumer is bound:
            mockModule.Verify(m => m.QueueBind(
                It.Is<string>(v => v == "MockTestQueueName"),
                It.Is<string>(v => v == "MockDirectExchangeName"),
                It.Is<string>(v => v == "MockKey")), Times.Once());
        }

        // If the queue requires it's delivered messages to be acknowledged the message header
        // is checked.  The consumer event handler specifies if the message is acknowledged by 
        // calling a method that stores an indicator in the header of the message.  If message
        // is acknowledged, RabbitMq is notified.
        [Fact]
        public void IfMessageDeliveryRequiresAck_RabbitToldStatusIfAck()
        {
            var metadata = GetBrokerMetadata();
            var mockMsgModule = new Mock<IMessagingModule>();
            var mockConn = new Mock<IConnection>();
            var mockModule = new Mock<IModel>();
            var exchange = new MockExchange();

            mockConn.Setup(c => c.CreateModel()).Returns(mockModule.Object);
            
            // Setup of test DirectExchange instance that can be asserted.
            exchange.InitializeSettings();
            metadata.Exchanges = new[] { exchange };
            Plugin.Log = new NullLogger();

            var consumer = SetupMessageConsumer();

            // This simulates the calls made by the module during bootstrapping:
            var messageBroker = new MockMessageBroker(mockMsgModule, mockConn);
            messageBroker.Initialize(metadata);
            messageBroker.DefineExchanges();
            messageBroker.BindConsumers(new[] { consumer });

            var domainEvt = new MockDomainEvent();
            var serializer = new JsonEventMessageSerializer();
            var body = serializer.Serialize(domainEvt);

            // Simulate the receiving of a message:
            var mockProps = new Mock<IBasicProperties>();
            mockProps.Setup(p => p.ContentType).Returns(serializer.ContentType);

            mockMsgModule.Setup(m => m.DispatchConsumer(It.IsAny<IMessage>(), It.IsAny<MessageDispatchInfo>()))
                .Returns((IMessage m, MessageDispatchInfo di) => {
                    m.SetAcknowledged();
                    return Task.FromResult((IMessage)m);
                });

            consumer.Consumer.HandleBasicDeliver("", 0, false, "", "", mockProps.Object, body);
            mockModule.Verify(m => m.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once());
        }

        // If the queue requires it's delivered messages to be acknowledged the message header
        // is checked.  The consumer event handler specifies if the message is rejected by 
        // calling a method that stores an indicator in the header of the message.  Or simply
        // by not acknowledging the message.  If message is not-acknowledged, RabbitMq is notified.
        [Fact]
        public void IfMessageDeliveryRequiresAck_RabbitToldStatusIfNotAck()
        {
            var metadata = GetBrokerMetadata();
            var mockMsgModule = new Mock<IMessagingModule>();
            var mockConn = new Mock<IConnection>();
            var mockModule = new Mock<IModel>();
            var exchange = new MockExchange();

            mockConn.Setup(c => c.CreateModel()).Returns(mockModule.Object);

            // Setup of test DirectExchange instance that can be asserted.
            exchange.InitializeSettings();
            metadata.Exchanges = new[] { exchange };
            Plugin.Log = new NullLogger();

            var consumer = SetupMessageConsumer();

            // This simulates the calls made by the module during bootstrapping:
            var messageBroker = new MockMessageBroker(mockMsgModule, mockConn);
            messageBroker.Initialize(metadata);
            messageBroker.DefineExchanges();
            messageBroker.BindConsumers(new[] { consumer });

            var domainEvt = new MockDomainEvent();
            var serializer = new JsonEventMessageSerializer();
            var body = serializer.Serialize(domainEvt);

            // Simulate the receiving of a message:
            var mockProps = new Mock<IBasicProperties>();
            mockProps.Setup(p => p.ContentType).Returns(serializer.ContentType);

            mockMsgModule.Setup(m => m.DispatchConsumer(It.IsAny<IMessage>(), It.IsAny<MessageDispatchInfo>()))
                .Returns((IMessage m, MessageDispatchInfo di) => {
                    m.SetRejected();
                    return Task.FromResult((IMessage)m);
                });

            consumer.Consumer.HandleBasicDeliver("", 0, false, "", "", mockProps.Object, body);
            mockModule.Verify(m => m.BasicReject(It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once());
        }

        [Fact]
        public void IfMessageAssociatedWithExchange_PublishedToRabbitMq()
        {

        }

        private MessageBrokerMetadata GetBrokerMetadata()
        {
            var metadata = new MessageBrokerMetadata
            {
                Settings = new BrokerSettings
                {
                    Connections = new BrokerConnection[]
                        {
                            new BrokerConnection { BrokerName = "MockTestBrokerName", HostName = "TestHost" }
                        }
                }
            };

            metadata.Connections = metadata.Settings.Connections.ToDictionary(c => c.BrokerName);
            return metadata;
        }

        private void AssertDeclaredExchange(Mock<IModel> mockedModuled, Action<DeclarationValues> values)
        {
            mockedModuled.Setup(m => m.ExchangeDeclare(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()))

                .Callback((string exchange, string type, bool durable, bool autoDelete, IDictionary<string, object> arguments) => {

                    values(new DeclarationValues
                    {
                        ExchangeName = exchange,
                        Type = type,
                        Durable = durable,
                        AutoDelete = autoDelete,
                        Arguments = arguments
                    });

                });
        }

        private void AssertDeclaredQueue(Mock<IModel> mockedModuled, Action<DeclarationValues> values)
        {
            mockedModuled.Setup(m => m.QueueDeclare(
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(), 
                It.IsAny<IDictionary<string, object>>()))

                .Callback((string queue, bool durable, bool exclusive, bool autoDelete, IDictionary<string, object> arguments) => {

                    values(new DeclarationValues
                    {
                        QueueName = queue,
                        Exclusive = exclusive,
                        Durable = durable,
                        AutoDelete = autoDelete,
                        Arguments = arguments
                    });

                });
        }

        private MessageConsumer SetupMessageConsumer()
        {
            return new MessageConsumer(
                new BrokerAttribute("MockTestBrokerName"),
                new AddQueueAttribute("MockTestQueueName", "MockDirectExchangeName") { RouteKey = "MockKey", IsExclusive = true },
                new MessageDispatchInfo {
                    MessageType = typeof(MockDomainEvent),
                    MessageHandlerMethod = typeof(MockMessageConsumer).GetMethod("OnJoin"),
                    DispatchRuleTypes = new Type[] { } });
        }
    }

    public class DeclarationValues
    {
        public string ExchangeName { get; set; }
        public string QueueName { get; set; }
        public string Type { get; set; }
        public bool Durable { get; set; }
        public bool AutoDelete { get; set; }
        public bool Exclusive { get; set; }
        public IDictionary<string, object> Arguments { get; set; }
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

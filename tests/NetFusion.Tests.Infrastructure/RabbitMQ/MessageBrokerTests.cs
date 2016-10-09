using FluentAssertions;
using Moq;
using NetFusion.Bootstrap.Logging;
using NetFusion.Messaging;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Modules;
using NetFusion.RabbitMQ;
using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Consumers;
using NetFusion.RabbitMQ.Core;
using NetFusion.RabbitMQ.Core.Initialization;
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
        // [Fact]
        public void ExceptionIfExchangeBrokerNameNoConfigured()
        {
            // TODO: REFACTOR TEST
            var broker = new MessageBrokerTestSetup();
            var exchange = new MockExchange();
            var brokerConfig = SetupBrockerConfig();

            exchange.InitializeSettings();
            //brokerConfig.Broker.Exchanges = new[] { exchange };
           // brokerConfig.BrokerConfig.Connections.Clear();

            Assert.Throws<BrokerException>(() => broker.Initialize(brokerConfig.BrokerConfig));
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
            var broker = new MessageBrokerTestSetup();
            var exchange = new MockExchange();
            var brokerConfig = SetupBrockerConfig();

            // Setup of test DirectExchange instance that can be asserted.
            exchange.InitializeSettings();
            brokerConfig.BrokerConfig.Exchanges = new[] { exchange };

            broker.Initialize(brokerConfig.BrokerConfig);

            var msg = new MockDomainEvent();
            broker.Instance.IsExchangeMessage(msg).Should().BeTrue();
        }

        /// <summary>
        /// For each defined exchange contained within the meta-data a call should
        /// be made to the RabbitMq server to define the exchange.
        /// </summary>
        [Fact]
        public void DiscoveredExchangeIsCreatedOnRabbitServer()
        {
            var broker = new MessageBrokerTestSetup();
            var exchange = new MockExchange { IsDurable = true, IsAutoDelete = true };
            var brokerConfig = SetupBrockerConfig();
            
            // Setup of test DirectExchange instance that can be asserted.
            exchange.InitializeSettings();
            brokerConfig.BrokerConfig.Exchanges = new[] { exchange };

            // Assert that the RabbitMq client was called with the correct values:
            AssertDeclaredExchange(broker.MockModule, v => {

                v.ExchangeName.Should().Be("MockDirectExchangeName");
                v.AutoDelete.Should().BeTrue();
                v.Durable.Should().BeTrue();
                v.Type.Should().Be("direct");
            });

            broker.Initialize(brokerConfig.BrokerConfig);
        }

        /// <summary>
        /// For each exchange defined queue contained within the meta-data a call should
        /// be made to the RabbitMq server to define the queue.
        /// </summary>
        [Fact]
        public void DiscoveredQueueIsCreatedOnRabbitServer()
        {
            var broker = new MessageBrokerTestSetup();
            var exchange = new MockExchange();
            var brokerConfig = SetupBrockerConfig();

            // Setup of test DirectExchange instance that can be asserted.
            exchange.InitializeSettings();
            brokerConfig.BrokerConfig.Exchanges = new[] { exchange };

            AssertDeclaredQueue(broker.MockModule, v =>
            {
                v.QueueName.Should().Be("MockTestQueueName");
                v.AutoDelete.Should().BeFalse();
                v.Exclusive.Should().BeFalse();
                v.Durable.Should().BeTrue();
            });

            broker.Initialize(brokerConfig.BrokerConfig);
        }

        /// <summary>
        /// A consumer of a delivered RabbitMq message is the same as any other message consumer.
        /// The only difference is that the class with the message handler must be decorated with
        /// the Broker attribute.  This specifies the broker connection on which the message handler
        /// method should be bound.  When a consumer joins an existing exchange queue, the handler
        /// method will be invoked round-robin with all other consumer bound methods defined in 
        /// all connected clients.
        /// </summary>
       // [Fact]
        public void IfConsumerJoinsNonFanoutQueueNewBindingCreatedForConsumer()
        {
            // TODO: REFACTOR TEST
            var broker = new MessageBrokerTestSetup();
            var exchange = new MockExchange();
            var brokerConfig = SetupBrockerConfig();
            var msgConsumer = SetupJoiningConsumer();

            // Setup of test DirectExchange instance that can be asserted.
            exchange.InitializeSettings();
            brokerConfig.BrokerConfig.Exchanges = new[] { exchange };

            // Initialize the broker.
            broker.Initialize(brokerConfig.BrokerConfig, msgConsumer);

            // Assert call made to the RabbitMq client:
            broker.MockModule.Verify(m => m.QueueBind(
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
        //[Fact]
        public void IfConsumerAddsQueueNewQueueIsCreatedExclusivelyForConsumer()
        {
            // TODO: REFACTOR TEST
            var broker = new MessageBrokerTestSetup();
            var exchange = new MockExchange();
            var brokerConfig = SetupBrockerConfig();
            var msgConsumer = SetupExclusiveConsumer();

            // Setup of test DirectExchange instance that can be asserted.
            exchange.InitializeSettings();
            brokerConfig.BrokerConfig.Exchanges = new[] { exchange };

            // Records all calls made to the RabbitMq client:
            var clientCalls = new List<DeclarationValues>();
            AssertDeclaredQueue(broker.MockModule, v =>
            {
                clientCalls.Add(v);
            });

            // Initialize the broker.
            broker.Initialize(brokerConfig.BrokerConfig, msgConsumer);

            // Assert call made to the RabbitMq client:
            // An exclusive queue is created for the consumer:
            clientCalls.Should().HaveCount(2);
            var addedQueueValuses = clientCalls.FirstOrDefault(c => c.Exclusive);

            addedQueueValuses.Should().NotBeNull();
            addedQueueValuses.QueueName.Should().Be("MockTestQueueName");

            // After creating exclusive queue, the consumer is bound:
            broker.MockModule.Verify(m => m.QueueBind(
                It.Is<string>(v => v == "MockTestQueueName"),
                It.Is<string>(v => v == "MockDirectExchangeName"),
                It.Is<string>(v => v == "MockKey"), 
                It.IsAny<IDictionary<string, object>>()), Times.Once());
        }
    
        // If the queue requires it's delivered messages to be acknowledged the message header
        // is checked.  The consumer event handler specifies if the message is acknowledged by 
        // calling a method that stores an indicator in the header of the message.  If message
        // is acknowledged, RabbitMq is notified.
        //[Fact]
        public void IfMessageDeliveryRequiresAck_RabbitToldStatusIfAck()
        {
            // TODO: REFACTOR TEST
            var broker = new MessageBrokerTestSetup();
            var exchange = new MockExchange();
            var brokerConfig = SetupBrockerConfig();
            var msgConsumer = SetupExclusiveConsumer();
            var domainEvt = new MockDomainEvent();

            // Setup of test DirectExchange instance that can be asserted.
            exchange.InitializeSettings();
            brokerConfig.BrokerConfig.Exchanges = new[] { exchange };

            // Initialize the broker.
            broker.Initialize(brokerConfig.BrokerConfig, msgConsumer);

            // Provide a mock consumer that acknowledges the received message,
            broker.MockMsgModule.Setup(m => m.InvokeDispatcherAsync(It.IsAny<MessageDispatchInfo>(), It.IsAny<IMessage>()))
                .Returns((MessageDispatchInfo di, IMessage m) => {
                    m.SetAcknowledged();
                    return Task.FromResult<object>(null);
                });

            // Simulate receiving of a message and verify that the RabbitMq
            // client was called correctly.
            broker.SimulateMessageReceived(domainEvt);
            broker.MockModule.Verify(m => m.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once());
        }

        // If the queue requires it's delivered messages to be acknowledged the message header
        // is checked.  The consumer event handler specifies if the message is rejected by 
        // calling a method that stores an indicator in the header of the message.  Or simply
        // by not acknowledging the message.  If message is not-acknowledged, RabbitMq is notified.
        //[Fact]
        public void IfMessageDeliveryRequiresAck_RabbitToldStatusIfNotAck()
        {
            // TODO: REFACTOR TEST
            var broker = new MessageBrokerTestSetup();
            var exchange = new MockExchange();
            var brokerConfig = SetupBrockerConfig();
            var msgConsumer = SetupExclusiveConsumer();
            var domainEvt = new MockDomainEvent();

            // Setup of test DirectExchange instance that can be asserted.
            exchange.InitializeSettings();
            brokerConfig.BrokerConfig.Exchanges = new[] { exchange };

            // Initialize the broker.
            broker.Initialize(brokerConfig.BrokerConfig, msgConsumer);

            // Provide a mock consumer that rejects the received message,
            broker.MockMsgModule.Setup(m => m.InvokeDispatcherAsync(It.IsAny<MessageDispatchInfo>(), It.IsAny<IMessage>()))
                .Returns((MessageDispatchInfo di, IMessage m) => {
                    m.SetRejected();
                    return Task.FromResult<object>(null);
                });


            // Simulate receiving of a message and verify that the RabbitMq
            // client was called correctly.
            broker.SimulateMessageReceived(domainEvt);
            broker.MockModule.Verify(m => m.BasicReject(It.IsAny<ulong>(), It.IsAny<bool>()), Times.Once());
        }

        [Fact]
        public void IfMessageAssociatedWithExchange_PublishedToRabbitMq()
        {

        }

        private MessageBrokerConfigSetup SetupBrockerConfig()
        {
            var serializer = new JsonBrokerSerializer();
            var metadata = new MessageBrokerSetup
            {
                BrokerSettings = new BrokerSettings
                {
                    Connections = new BrokerConnectionSettings[]
                        {
                            new BrokerConnectionSettings { BrokerName = "MockTestBrokerName", HostName = "TestHost" }
                        }
                }
            };

            var serializers = new Dictionary<string, IBrokerSerializer>
            {
                { serializer.ContentType, serializer }
            };

           // metadata.Connections = metadata.Settings.Connections.ToDictionary(c => c.BrokerName);
            var mockConnMgr = new MockConnectionManager(new NullLogger(), metadata.BrokerSettings, new Dictionary<string, object>());

            metadata.ConnectionMgr = mockConnMgr;
            metadata.SerializationMgr = new SerializationManager(serializers);
            return new MessageBrokerConfigSetup
            {
                BrokerConfig = metadata,
                MockConnMgr = mockConnMgr
            };
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

        private MessageConsumer SetupExclusiveConsumer()
        {
            return new MessageConsumer(
                new BrokerAttribute("MockTestBrokerName"),
                new AddQueueAttribute("MockTestQueueName", "MockDirectExchangeName") { RouteKey = "MockKey", IsExclusive = true },
                new MessageDispatchInfo {
                    MessageType = typeof(MockDomainEvent),
                    MessageHandlerMethod = typeof(MockMessageConsumer).GetMethod("OnJoin"),
                    DispatchRuleTypes = new Type[] { } });
        }

        private MessageConsumer SetupJoiningConsumer()
        {
            return new MessageConsumer(
                new BrokerAttribute("MockTestBrokerName"),
                new JoinQueueAttribute("MockTestQueueName", "MockDirectExchangeName"),
                new MessageDispatchInfo());
        }
    }

    public class MessageBrokerConfigSetup
    {
        public MessageBrokerSetup BrokerConfig { get; set; }
        public MockConnectionManager MockConnMgr { get; set; }
    }

    public class MessageBrokerTestSetup
    {
        private MessageConsumer _messageConsumer;
        private IBrokerSerializer _messageSerializer;

        public NullLogger Logger { get; }
        public Mock<IMessagingModule> MockMsgModule { get; }
        public Mock<IConnection> MockConn { get; }
        public Mock<IModel> MockModule { get; }
        public IMessageBroker Instance { get; }

        public MessageBrokerTestSetup()
        {
            _messageSerializer = new JsonBrokerSerializer();

            this.Logger = new NullLogger();
            this.MockMsgModule = new Mock<IMessagingModule>();
            this.MockConn = new Mock<IConnection>();
            this.MockModule = new Mock<IModel>();

            this.MockConn.Setup(c => c.CreateModel())
                .Returns(MockModule.Object);

            this.Instance = new MockMessageBroker(this.Logger, this.MockMsgModule, this.MockConn);
        }

        public void SimulateMessageReceived(IMessage message)
        {
            // TEST REFACTOR
            //var mockProps = new Mock<IBasicProperties>();
            //mockProps.Setup(p => p.ContentType).Returns(_messageSerializer.ContentType);

            //var body = _messageSerializer.Serialize(message);
            //_messageConsumer.Consumer.HandleBasicDeliver("", 0, false, "", "", mockProps.Object, body);
        }

        public void Initialize(NetFusion.RabbitMQ.Core.MessageBrokerSetup brokerConfig, MessageConsumer consumer = null)
        {
            _messageConsumer = consumer;

            this.Instance.Initialize(brokerConfig);
            this.Instance.ConfigureBroker();

            if (_messageConsumer != null)
            {
                this.Instance.BindConsumers(new[] { _messageConsumer });
            } 
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

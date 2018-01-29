using Microsoft.Extensions.Logging;
using Moq;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Modules;
using NetFusion.Messaging.Types;
using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Consumers;
using NetFusion.RabbitMQ.Core;
using NetFusion.RabbitMQ.Core.Initialization;
using NetFusion.RabbitMQ.Serialization;
using NetFusion.Testing.Logging;
using NetFusion.Tests.Infrastructure.RabbitMQ.Mocks;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace InfrastructureTests.RabbitMQ.Broker
{
    public class BrokerTest
    {
        // -------------------------- UNIT-TEST SETUP -------------------------------------------

        // Returns a test setup containing a configured MessageBrokerState instance and mocked
        // ConnectionManager.  This simulates the state built by the MessageBrokerModule that is
        // used to initialize a MessageBroker instance and passed to the internal initialization
        // classes to which it delegates.
        public static BrokerStateSetup SetupBrokerState(BrokerSettings brokerSettings = null)
        {
            var serializer = new JsonBrokerSerializer();
            var brokerState = new MessageBrokerState
            {
                BrokerSettings = brokerSettings ?? new BrokerSettings
                {
                    Connections = new List<BrokerConnectionSettings>
                        {
                            new BrokerConnectionSettings { BrokerName = "MockTestBrokerName", HostName = "TestHost" }
                        }
                }
            };

            var serializers = new Dictionary<string, IBrokerSerializer>
            {
                { serializer.ContentType, serializer }
            };

            var mockConnMgr = new MockConnectionManager(
                new TestLoggerFactory(), 
                brokerState.BrokerSettings, 
                new Dictionary<string, object>());

            brokerState.ConnectionMgr = mockConnMgr;
            brokerState.SerializationMgr = new SerializationManager(serializers);
            return new BrokerStateSetup
            {
                BrokerState = brokerState,
                MockConnMgr = mockConnMgr
            };
        }

        public static MessageBrokerSetup SetupMessageBroker()
        {
            return new MessageBrokerSetup();
        }

        public static MessageConsumer SetupExclusiveConsumer()
        {
            return new MessageConsumer(
                new BrokerAttribute("MockTestBrokerName"),
                new AddQueueAttribute("MockTestQueueName", "MockDirectExchangeName") { RouteKey = "MockKey", IsExclusive = true },
                new MessageDispatchInfo
                {
                    MessageType = typeof(MockDomainEvent),
                    MessageHandlerMethod = typeof(MockMessageConsumer).GetTypeInfo().GetMethod("OnJoin"),
                    DispatchRuleTypes = new Type[] { }
                });
        }

        public static MessageConsumer SetupJoiningConsumer()
        {
            return new MessageConsumer(
                new BrokerAttribute("MockTestBrokerName"),
                new JoinQueueAttribute("MockTestQueueName", "MockDirectExchangeName"),
                new MessageDispatchInfo());
        }

        // -------------------------- UNIT-TEST CLASSES -------------------------------------------

        // Setup default broker state and connection manager used for testing.
        public class BrokerStateSetup
        {
            public MessageBrokerState BrokerState { get; set; }
            public MockConnectionManager MockConnMgr { get; set; }
        }

        // Properly sets up a IMessageBroker instance mocking the components 
        // on which it depends.  The mocked components are exposed so they
        // can be setup and asserted by the tests.
        public class MessageBrokerSetup
        {
            private MessageConsumer _messageConsumer;
            private IBrokerSerializer _messageSerializer;

            public Mock<IMessageDispatchModule> MockMsgModule { get; }

            // Instance to the configured message broker.
            public IMessageBroker Instance { get; }

            public MessageBrokerSetup()
            {
                _messageSerializer = new JsonBrokerSerializer();

                ILoggerFactory loggerFactory = new TestLoggerFactory();

                this.MockMsgModule = new Mock<IMessageDispatchModule>();

                this.Instance = new MockMessageBroker(loggerFactory, this.MockMsgModule);
            }

            // Initializes the message broker instance with the specified broker-state
            // and optional message-consumer.
            public void Initialize(MessageBrokerState brokerState, MessageConsumer consumer = null)
            {
                _messageConsumer = consumer;

                this.Instance.Initialize(brokerState);
                this.Instance.ConfigurePublishers();

                if (_messageConsumer != null)
                {
                    this.Instance.BindConsumers(new[] { _messageConsumer });
                } 
            }

            public void SimulateMessageReceived(IMessage message)
            {
                // TEST REFACTOR
                var mockProps = new Mock<IBasicProperties>();
                mockProps.Setup(p => p.ContentType).Returns(_messageSerializer.ContentType);

                var body = _messageSerializer.Serialize(message);
                _messageConsumer.MessageHandlers.First().EventConsumer.HandleBasicDeliver("", 0, false, "", "", mockProps.Object, body);
            }
        }

        // -------------------------- UNIT-TEST ASSERTIONS  -------------------------------------------

        public static void AssertDeclaredExchange(Mock<IModel> mockedModuled, Action<DeclarationValues> values)
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

        public static void AssertDeclaredQueue(Mock<IModel> mockedModuled, Action<DeclarationValues> values)
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
    }
}

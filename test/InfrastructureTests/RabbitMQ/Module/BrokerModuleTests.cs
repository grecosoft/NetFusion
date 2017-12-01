using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NetFusion.Base;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.RabbitMQ;
using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Consumers;
using NetFusion.RabbitMQ.Core;
using NetFusion.Test.Container;
using NetFusion.Testing.Logging;
using NetFusion.Tests.Infrastructure.RabbitMQ.Mocks;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace InfrastructureTests.RabbitMQ.Module
{
    /// <summary>
    /// These tests asset the correct initialization of the message broker module.  This module determines the 
    /// settings that should be used by the IMessageBroker implementation.  The initialization inputs to the 
    /// message broker are tested by mocking an instance of IMessageBroker.  The module is responsible for 
    /// gathering the meta-data that the message broker uses to configure the RabbitMq broker.
    /// </summary>
    public class MessageBrokerModuleTests
    {
        /// <summary>
        /// A warning is logged if there are defined exchanges but the RabbitMqMessagePublisher 
        /// message publisher has not been registered.
        /// </summary>
        [Fact(DisplayName = "Exchanges Declared RabbitMQ Publisher must be Registered")]
        public void ExchangesDecared_RabbitMqPublisher_MustBeRegistered()
        {
            ContainerFixture.Test(fixture => { fixture
                .Arrange
                    .Resolver(r => {
                        r.WithRabbitConfiguredHost();
                        r.GetHostPlugin()
                            .AddPluginType<MockDirectExchange>();
                    })
                    .Container(c => c.UsingServices())
                .Act.OnContainer(c =>
                {
                    c.Build().Start();
                })
                .Assert.Container(c =>
                {
                    c.GetTestLogger()
                        .HasSingleEntryFor(RabbitMqLogEvents.BROKER_CONFIGURATION, LogLevel.Warning)
                        .Should().BeTrue();
                });
            });
        }

        /// <summary>
        /// A broker name identifies a RabbitMQ instance.  The broker name specified on an exchange declaration determines 
        /// the RabbitMQ server on which the exchange and is associated queues are created.  The broker name must be unique.
        /// </summary>
        [Fact(DisplayName = "Settings Broker Name must be Unique")]
        public void SettingsBrokerName_MustBeUnique()
        {
            var settings = new BrokerSettings
            {
                Connections = new List<BrokerConnectionSettings>
                {
                    new BrokerConnectionSettings { BrokerName = "Broker1" },
                    new BrokerConnectionSettings { BrokerName = "Broker1" }
                }
            };

            ContainerFixture.Test(fixture => { fixture
                .Arrange
                    .Resolver(r => r.WithRabbitConfiguredHost())
                    .Container(c => c.UsingServices(settings))
                .Act.OnContainer(c =>
                {
                    c.Build().Start();
                })
                .Assert.Exception<ContainerException>(ex =>
                {
                    ex.InnerException.Should().NotBeNull();
                    ex.InnerException.Message.Should().Contain("The following broker names are specified more than once:");
                });
            });
        }

        /// <summary>
        /// The MessageBrokerModule determines all of the defined exchanges and initializes them using their configured settings.  
        /// Also, it adds any externally defined route keys contained in the configuration.  The initialized exchange configurations 
        /// are then passed to the message broker for creation on the RabbitMq server.
        /// </summary>
        [Fact(DisplayName = "Exchange Definitions Discovered")]
        public void ExchangeDefinitions_Discovered()
        {
            var settings = new BrokerSettings
            {
                Connections = new List<BrokerConnectionSettings>
                {
                    new BrokerConnectionSettings {
                        BrokerName = "MockTestBrokerName",
                        QueueProperties = new QueuePropertiesSettings[] {
                            new QueuePropertiesSettings { QueueName = "MockTestQueueName", RouteKeys = new [] {"RouteKeyTwo"} }
                        }
                    }
                }
            };

            // Capture the state sent to the message broker so the discovered exchanges can be asserted.
            var mockBroker = new Mock<IMessageBroker>();
            MessageBrokerState brokerState = null;

            mockBroker.Setup(b => b.Initialize(It.IsAny<MessageBrokerState>()))
                .Callback<MessageBrokerState>(s => brokerState = s);

            ContainerFixture.Test(fixture => { fixture
                .Arrange
                    .Resolver(r => {
                        r.WithRabbitConfiguredHost();
                        r.GetHostPlugin()
                            .AddPluginType<MockExchange>()
                            .AddPluginType<MockTopicExchange>();
                    })
                    .Container(c => {
                        c.UsingServices(settings, mockBroker.Object);
                        c.AddRabbitMqPublisher();
                    })
                .Act.OnContainer(c =>
                {
                    c.Build().Start();
                })
                .Assert.Container(c =>
                {
                    // Verify that module initialized the message broker with the exchanges:
                    brokerState.Should().NotBeNull();
                    brokerState.BrokerSettings.Should().NotBeNull();
                    brokerState.Exchanges.Should().NotBeNull();

                    // Verify that the correct exchanges were determined.
                    brokerState.Exchanges.Should().HaveCount(2);
                    brokerState.Exchanges.Should().Contain(e => e.ExchangeName == "MockDirectExchangeName");
                    brokerState.Exchanges.Should().Contain(e => e.ExchangeName == "MockTopicExchangeName");

                    // Verify that the externally defined route key was added to the queue:
                    var directExchange = brokerState.Exchanges.First(e => e.ExchangeName == "MockDirectExchangeName");
                    var queue = directExchange.Queues.FirstOrDefault();

                    queue.Should().NotBeNull();
                    queue.RouteKeys.Should().HaveCount(1);
                    queue.RouteKeys.Should().Contain(k => k == "RouteKeyTwo");
                });
            });
        }

        // Messages consumers are just classes implementing the IMessageConsumer interface that have
        // message handlers marked with the derived QueueConsumerAttribute attributes.  The class
        // implementing the IMessageConsumer interface must also be marked with the BrokerAttribute.
        // The broker attribute specifies the RabbitMQ server on which the consumer's event handlers
        // are bound.   
        [Fact (DisplayName = "Message Consumers Determined")]
        public void MessageConsumers_Determined()
        {
            var settings = new BrokerSettings
            {
                Connections = new List<BrokerConnectionSettings>
                {
                    new BrokerConnectionSettings { BrokerName = "MockTestBrokerName" }
                }
            };

            // Capture the state sent to the message broker so the discovered exchanges can be asserted.
            var mockBroker = new Mock<IMessageBroker>();
            IEnumerable<MessageConsumer> consumers = null;

            mockBroker.Setup(b => b.BindConsumers(It.IsAny<IEnumerable<MessageConsumer>>()))
                .Callback<IEnumerable<MessageConsumer>>(mc => consumers = mc);


            ContainerFixture.Test(fixture => { fixture
                .Arrange
                    .Resolver(r => {
                        r.WithRabbitConfiguredHost();
                        r.GetHostPlugin()
                            .AddPluginType<MockExchange>()
                            .AddPluginType<MockMessageConsumer>();
                    })
                    .Container(c => {
                        c.UsingServices(settings, mockBroker.Object);
                        c.AddRabbitMqPublisher();
                    })
                .Act.OnContainer(c =>
                {
                    c.Build().Start();
                })
                .Assert.Container(container =>
                {
                    consumers.Should().NotBeNull();
                    consumers.Should().HaveCount(2);

                    // Verify that the consumer event hander that is joining an existing queue on the exchange.
                    var joinedConsumer = consumers.FirstOrDefault(c => c.BindingType == QueueBindingTypes.Join);
                    joinedConsumer.Should().NotBeNull();
                    joinedConsumer.ExchangeName.Should().Be("MockDirectExchangeName");
                    joinedConsumer.QueueName.Should().Be("MockTestQueueName");

                    // Verify the consumer event handler that is creating an exclusive queue on the exchange.
                    var addConsumer = consumers.FirstOrDefault(c => c.BindingType == QueueBindingTypes.Create);
                    addConsumer.Should().NotBeNull();
                    addConsumer.ExchangeName.Should().Be("MockDirectExchangeName");
                    addConsumer.QueueName.Should().BeEmpty(); // The server will assign name.
                });
            });
        }

        [Fact (DisplayName="Default Serializers added when not specified by Host")]
        public void DefaultSerializersAdded_WhenNotSpecifiedByHost()
        {
            var settings = new BrokerSettings();
            var mockBroker = new Mock<IMessageBroker>();
            MessageBrokerState brokerState = null;

            // Capture the state sent to the message broker so the discovered exchanges can be asserted.
            mockBroker.Setup(b => b.Initialize(It.IsAny<MessageBrokerState>()))
                .Callback<MessageBrokerState>(s => brokerState = s);

            ContainerFixture.Test(fixture => { fixture
                .Arrange
                    .Resolver(r => r.WithRabbitConfiguredHost())
                    .Container(c => c.UsingServices(settings, mockBroker.Object))
                .Act.OnContainer(c =>
                {
                    c.Build().Start();
                })
                .Assert.Container(c =>
                {
                    #if NETCOREAPP1_0
                    brokerState.SerializationMgr.Serializers.Should().HaveCount(1);
                    brokerState.SerializationMgr.Serializers.Select(s => s.ContentType).Should().Contain(SerializerTypes.Json);
                    #endif

                    #if NET461
                    brokerState.SerializationMgr.Serializers.Should().HaveCount(3);
                    brokerState.SerializationMgr.Serializers.Select(s => s.ContentType).Should().Contain(SerializerTypes.Json);
                    brokerState.SerializationMgr.Serializers.Select(s => s.ContentType).Should().Contain(SerializerTypes.Binary);
                    brokerState.SerializationMgr.Serializers.Select(s => s.ContentType).Should().Contain(SerializerTypes.MessagePack);
                    #endif
                });
            });
        }     
    }
}

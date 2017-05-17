using Autofac;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NetFusion.Base;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Messaging.Config;
using NetFusion.RabbitMQ;
using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Consumers;
using NetFusion.RabbitMQ.Core;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using NetFusion.Testing.Logging;
using NetFusion.Tests.Infrastructure.RabbitMQ.Mocks;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace InfrastructureTests.RabbitMQ.Module
{
    /// <summary>
    /// These tests asset the correct initialization of the message broker module.
    /// This module determines the settings that should be used by the IMessageBroker
    /// implementation.  The initialization inputs to the message broker are tested
    /// by mocking an instance of IMessageBroker.  The module is responsible for 
    /// gathering the meta-data that the message broker uses to configure the
    /// RabbitMq broker.
    /// </summary>
    public class MessageBrokerModuleTests
    {
        /// <summary>
        /// A warning is logged if there are defined exchanges but the 
        /// RabbitMqMessagePublisher message publisher has not been registered.
        /// </summary>
        [Fact (DisplayName = nameof(ExchangesDecared_RabbitMqPublisher_MustBeRegistered))]
        public void ExchangesDecared_RabbitMqPublisher_MustBeRegistered()
        {
            var settings = new BrokerSettings();
            var mockExchange = new Mock<IMessageBroker>();

            ModuleTest.Arrange(config =>
            {
                config.AddPlugin<MockAppHostPlugin>()
                    .AddPluginType<MockDirectExchange>();
            })
            .Test(c =>
            {
                AddTestMocks(c, settings, mockExchange.Object);
                c.Build().Start();
            },
            (c, e) =>
            {
                c.GetTestLogger()
                    .HasSingleEntryFor(RabbitMqLogEvents.BROKER_CONFIGURATION, LogLevel.Warning)
                    .Should().BeTrue();
            });
        }

        /// <summary>
        /// A broker name identifies a RabbitMQ instance.  The broker name specified on an
        /// exchange declaration determines the RabbitMQ server on which the exchange and
        /// is associated queues are created.  The broker name must be unique.
        /// </summary>
        [Fact (DisplayName = nameof(SettingsBrokerName_MustBeUnique))]
        public void SettingsBrokerName_MustBeUnique()
        {
            var settings = new BrokerSettings();
            var mockExchange = new Mock<IMessageBroker>();

            settings.Connections = new List<BrokerConnectionSettings>
            {
                new BrokerConnectionSettings { BrokerName = "Broker1" },
                new BrokerConnectionSettings { BrokerName = "Broker1" }
            };

            ModuleTest.Arrange(config =>
            {
                config.AddPlugin<MockAppHostPlugin>();
            })
            .Test(c =>
            {
                AddTestMocks(c, settings, mockExchange.Object);
                c.Build().Start();
            },
            (c, e) =>
            {
                e.Should().BeOfType<ContainerException>();
            });
        }

        /// <summary>
        /// The MessageBrokerModule determines all of the defined exchanges and
        /// initializes them using their configured settings.  Also, it adds
        /// any externally defined route keys contained in the configuration.
        /// The initialized exchange configurations are then passed to the 
        /// message broker for creation on the RabbitMq server.
        /// </summary>
        [Fact (DisplayName = nameof(ExchangeDefinitions_Discovered))]
        public void ExchangeDefinitions_Discovered()
        {
            var settings = new BrokerSettings();
            var mockExchange = new Mock<IMessageBroker>();
            MessageBrokerState brokerState = null;

            mockExchange.Setup(b => b.Initialize(It.IsAny<MessageBrokerState>()))
                .Callback<MessageBrokerState>(s => brokerState = s);

            // Define the broker connection and an externally defined queue route key.
            settings.Connections = new List<BrokerConnectionSettings>
            {
                new BrokerConnectionSettings {
                    BrokerName = "MockTestBrokerName",
                    QueueProperties = new QueuePropertiesSettings[] {
                        new QueuePropertiesSettings { QueueName = "MockTestQueueName", RouteKeys = new [] {"RouteKeyTwo"} }
                    }
                }
            };

            ModuleTest.Arrange(config =>
            {
                config.AddPlugin<MockAppHostPlugin>()
                    .AddPluginType<MockTopicExchange>()
                    .AddPluginType<MockExchange>();
            })
            .Act(c =>
            {
                c.WithConfig((MessagingConfig config) =>
                {
                    config.AddMessagePublisher<RabbitMqMessagePublisher>();
                });

                AddTestMocks(c, settings, mockExchange.Object);
                c.Build().Start();
            });

            // Verify that module initialized the message broker with the exchanges:
            brokerState.Should().NotBeNull();
            brokerState.BrokerSettings.Should().NotBeNull();
            brokerState.Exchanges.Should().NotBeNull();
           // metadata.Connections.Should().NotBeNull();

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
        }

        // Messages consumers are just classes implementing the IMessageConsumer interface that have
        // message handlers marked with the derived QueueConsumerAttribute attributes.  The class
        // implementing the IMessageConsumer interface must also be marked with the BrokerAttribute.
        // The broker attribute specifies the RabbitMQ server on which the consumer's event handlers
        // are bound.   
        [Fact (DisplayName = nameof(MessageConsumers_Determined))]
        public void MessageConsumers_Determined()
        {
            var settings = new BrokerSettings();
            var mockExchange = new Mock<IMessageBroker>();
            IEnumerable<MessageConsumer> consumers = null;

            mockExchange.Setup(b => b.BindConsumers(It.IsAny<IEnumerable<MessageConsumer>>()))
                .Callback<IEnumerable<MessageConsumer>>(mc => consumers = mc);

            // Define the broker connection and an externally defined queue route key.
            settings.Connections = new List<BrokerConnectionSettings>
            {
                new BrokerConnectionSettings { BrokerName = "MockTestBrokerName" }
            };

            ModuleTest.Arrange(config =>
            {
                // Add an exchange definition and consumer service.
                config.AddPlugin<MockAppHostPlugin>()
                    .AddPluginType<MockExchange>()
                    .AddPluginType<MockMessageConsumer>();
            })
            .Act(c =>
            {
                c.WithConfig((MessagingConfig config) =>
                {
                    config.AddMessagePublisher<RabbitMqMessagePublisher>();
                });

                AddTestMocks(c, settings, mockExchange.Object);
                c.Build().Start();
            });

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
        }

        [Fact (DisplayName=nameof(DefaultSerializersAdded_WhenNotSpecifiedByHost))]
        public void DefaultSerializersAdded_WhenNotSpecifiedByHost()
        {
            var settings = new BrokerSettings();
            var mockExchange = new Mock<IMessageBroker>();
            MessageBrokerState brokerConfig = null;

            mockExchange.Setup(b => b.Initialize(It.IsAny<MessageBrokerState>()))
                .Callback<MessageBrokerState>(cfg => brokerConfig = cfg);

            ModuleTest.Arrange(config =>
            {
                config.AddPlugin<MockAppHostPlugin>();
                    
            })
            .Act(c =>
            {
                AddTestMocks(c, settings, mockExchange.Object);
                c.Build().Start();
            });

#if NETCOREAPP1_0
            brokerConfig.SerializationMgr.Serializers.Should().HaveCount(1);
            brokerConfig.SerializationMgr.Serializers.Select(s => s.ContentType).Should().Contain(SerializerTypes.Json);
#endif

#if NET461
            brokerConfig.SerializationMgr.Serializers.Should().HaveCount(3);
            brokerConfig.SerializationMgr.Serializers.Select(s => s.ContentType).Should().Contain(SerializerTypes.Json);
            brokerConfig.SerializationMgr.Serializers.Select(s => s.ContentType).Should().Contain(SerializerTypes.Binary);
            brokerConfig.SerializationMgr.Serializers.Select(s => s.ContentType).Should().Contain(SerializerTypes.MessagePack);
#endif

        }   

        // Sets mock broker settings and mock message broker.  These specified instances will be 
        // registered overriding the default instances. 
        private ContainerTest AddTestMocks(IAppContainer container, BrokerSettings settings, IMessageBroker broker)
        {
            container.WithConfig<AutofacRegistrationConfig>(config =>
            {
                config.Build = builder =>
                {
                    builder.RegisterInstance(settings)
                        .As<BrokerSettings>()
                        .SingleInstance();

                    builder.RegisterInstance(broker)
                        .As<IMessageBroker>()
                        .SingleInstance();
                };
            });

            return new ContainerTest((AppContainer)container);
        }     
    }
}

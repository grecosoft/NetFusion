using Autofac;
using FluentAssertions;
using Moq;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Logging;
using NetFusion.Bootstrap.Testing;
using NetFusion.Messaging.Config;
using NetFusion.Messaging.Testing;
using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Consumers;
using NetFusion.RabbitMQ.Core;
using NetFusion.RabbitMQ.Serialization;
using NetFusion.Settings.Testing;
using NetFusion.Tests.Core.Bootstrap.Mocks;
using NetFusion.Tests.Infrastructure.RabbitMQ.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NetFusion.Tests.Infrastructure.RabbitMQ
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
        [Fact]
        public void IfExchangesDecaredRabbitMqPublisherMustBeRegistered()
        {
            var settings = new BrokerSettings();
            var mockExchange = new Mock<IMessageBroker>();

            this.Arrange(config =>
            {
                config.AddPlugin<MockAppHostPlugin>()
                    .AddPluginType<MockDirectExchange>();
            })
            .Act(c =>
            {
                AddTestMocks(c, settings, mockExchange.Object);
                c.Build();
                c.Start();
            })
            .Assert((c, e) =>
            {
                var nullLogger = (NullLogger)c.Logger;
                nullLogger.Messages.Should().Contain(m => m.StartsWith("WARNING"));
            });
        }

        /// <summary>
        /// A broker name identifies a RabbitMQ instance.  The broker name specified on an
        /// exchange declaration determines the RabbitMQ server on which the exchange and
        /// is associated queues are created.  The broker name must be unique.
        /// </summary>
        [Fact]
        public void BrokerNameInSettingMustBeUnique()
        {
            var settings = new BrokerSettings();
            var mockExchange = new Mock<IMessageBroker>();

            settings.Connections = new List<BrokerConnection>
            {
                new BrokerConnection { BrokerName = "Broker1" },
                new BrokerConnection { BrokerName = "Broker1" }
            };

            this.Arrange(config =>
            {
                config.AddPlugin<MockAppHostPlugin>();
            })
            .Act(c =>
            {
                AddTestMocks(c, settings, mockExchange.Object);
                c.Build();
                c.Start();
            })
            .Assert((c, e) =>
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
        [Fact]
        public void AllExchangeDefinitionsAreDiscovered()
        {
            var settings = new BrokerSettings();
            var mockExchange = new Mock<IMessageBroker>();
            MessageBrokerConfig metadata = null;

            mockExchange.Setup(b => b.Initialize(It.IsAny<MessageBrokerConfig>()))
                .Callback<MessageBrokerConfig>(s => metadata = s);

            // Define the broker connection and an externally defined queue route key.
            settings.Connections = new List<BrokerConnection>
            {
                new BrokerConnection {
                    BrokerName = "MockTestBrokerName",
                    QueueProperties = new QueueProperties[] {
                        new QueueProperties { QueueName = "MockTestQueueName", RouteKeys = new [] {"RouteKeyTwo"} }
                    }
                }
            };

            this.Arrange(config =>
            {
                config.AddPlugin<MockAppHostPlugin>()
                    .AddPluginType<MockTopicExchange>()
                    .AddPluginType<MockExchange>();
            })
            .Act(c =>
            {
                c.WithConfig((MessagingConfig config) =>
                {
                    config.AddMessagePublisherType<RabbitMqMessagePublisher>();
                });

                AddTestMocks(c, settings, mockExchange.Object);
                c.Build();
                c.Start();
            });

            // Verify that module initialized the message broker with the exchanges:
            metadata.Should().NotBeNull();
            metadata.Settings.Should().NotBeNull();
            metadata.Exchanges.Should().NotBeNull();
           // metadata.Connections.Should().NotBeNull();

            // Verify that the correct exchanges were determined.
            metadata.Exchanges.Should().HaveCount(2);
            metadata.Exchanges.Should().Contain(e => e.ExchangeName == "MockDirectExchangeName");
            metadata.Exchanges.Should().Contain(e => e.ExchangeName == "MockTopicExchangeName");

            // Verify that the externally defined route key was added to the queue:
            var directExchange = metadata.Exchanges.First(e => e.ExchangeName == "MockDirectExchangeName");
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
        [Fact]
        public void AllMessageConsumersDetermined()
        {
            var settings = new BrokerSettings();
            var mockExchange = new Mock<IMessageBroker>();
            IEnumerable<MessageConsumer> consumers = null;

            mockExchange.Setup(b => b.BindConsumers(It.IsAny<IEnumerable<MessageConsumer>>()))
                .Callback<IEnumerable<MessageConsumer>>(mc => consumers = mc);

            // Define the broker connection and an externally defined queue route key.
            settings.Connections = new List<BrokerConnection>
            {
                new BrokerConnection { BrokerName = "MockTestBrokerName" }
            };

            this.Arrange(config =>
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
                    config.AddMessagePublisherType<RabbitMqMessagePublisher>();
                });

                AddTestMocks(c, settings, mockExchange.Object);
                c.Build();
                c.Start();
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

        [Fact]
        public void DefaultSerializersAddedIfNotSpecifiedByHost()
        {
            var settings = new BrokerSettings();
            var mockExchange = new Mock<IMessageBroker>();
            MessageBrokerConfig brokerConfig = null;

            mockExchange.Setup(b => b.Initialize(It.IsAny<MessageBrokerConfig>()))
                .Callback<MessageBrokerConfig>(cfg => brokerConfig = cfg);

            this.Arrange(config =>
            {
                config.AddPlugin<MockAppHostPlugin>();
                    
            })
            .Act(c =>
            {
                AddTestMocks(c, settings, mockExchange.Object);
                c.Build();
                c.Start();
            });

            brokerConfig.SerializationMgr.Serializers.Should().HaveCount(3);
            brokerConfig.SerializationMgr.Serializers.Select(s => s.ContentType).Should().Contain(
                SerializerTypes.Binary, 
                SerializerTypes.MessagePack,
                SerializerTypes.Json);
        }

        // Creates an initialized container with the needed dependent plug-ins;
        private ContainerAct Arrange(Action<TestTypeResolver> config)
        {
            var typeResolver = new TestTypeResolver();

            typeResolver.AddSettingsPlugin();
            typeResolver.AddMessagingPlugin();
            typeResolver.AddRabbitMqPlugin();

            config(typeResolver);

            return new ContainerAct(new AppContainer(new string[] { }, typeResolver));
        }

        // Sets mock broker settings and mock message broker.  These specified instances will be 
        // registered overriding the default instances. 
        private ContainerAct AddTestMocks(IAppContainer container, BrokerSettings settings, IMessageBroker broker)
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

            return new ContainerAct((AppContainer)container);
        }
    }
}

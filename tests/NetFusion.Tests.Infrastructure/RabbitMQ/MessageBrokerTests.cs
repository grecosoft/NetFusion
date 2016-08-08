using FluentAssertions;
using Moq;
using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Core;
using NetFusion.RabbitMQ.Exchanges;
using NetFusion.Tests.Infrastructure.RabbitMQ.Mocks;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NetFusion.Tests.Infrastructure.RabbitMQ
{
    public class MessageBrokerTests
    {
        [Fact]
        public void ExceptionIfExchangeBrokerNameNoConfigured()
        {
            var metadata = GetBrokerMetadata();
            var mockConn = new Mock<IConnection>();
            var exchange = new MockExchange();

            exchange.InitializeSettings();
            metadata.Exchanges = new[] { exchange };
            metadata.Connections.Clear();

            var messageBroker = new MockMessageBroker(mockConn);
            messageBroker.Initialize(metadata);

            Assert.Throws<InvalidOperationException>(() => messageBroker.DefineExchanges());
        }

        [Fact]
        public void CanDetermineIfEventAssociatedWithExchange()
        {
            var metadata = GetBrokerMetadata();
            var mockConn = new Mock<IConnection>();
            var exchange = new MockExchange();

            exchange.InitializeSettings();
            metadata.Exchanges = new[] { exchange };

            var messageBroker = new MockMessageBroker(mockConn);
            messageBroker.Initialize(metadata);

            var msg = new MockDomainEvent();
            messageBroker.IsExchangeMessage(msg).Should().BeTrue();
        }

        [Fact]
        public void EachDiscoveredExchangeIsCreatedOnRabbitServer()
        {
            var metadata = GetBrokerMetadata();
            var mockConn = new Mock<IConnection>();
            var mockModule = new Mock<IModel>();
            var exchange = new MockExchange { IsDurable = true, IsAutoDelete = true };

            mockConn.Setup(c => c.CreateModel()).Returns(mockModule.Object);

            // Setup of test DirectExchange instance that can be asserted.
            exchange.InitializeSettings();
            metadata.Exchanges = new[] { exchange };

            // Initialize the broker with the meta-data.
            var messageBroker = new MockMessageBroker(mockConn);
            messageBroker.Initialize(metadata);

            // Assert that the RabbitMq client was called with the correct values:
            AssertDeclaredExchange(mockModule, (ev) => {

                ev.Exchange.Should().Be("MockDirectExchangeName");
                ev.AutoDelete.Should().BeTrue();
                ev.Durable.Should().BeTrue();
                ev.Type.Should().Be("direct");
            });

            messageBroker.DefineExchanges();


            // Alerter the exchange settings to make sure default value were not being tested:
            exchange.IsDurable = false;
            exchange.IsAutoDelete = false;
            exchange.InitializeSettings();

            // Assert that the RabbitMq client was called with the correct values:
            AssertDeclaredExchange(mockModule, (ev) => {

                ev.Exchange.Should().Be("MockDirectExchangeName");
                ev.AutoDelete.Should().BeFalse();
                ev.Durable.Should().BeFalse();
                ev.Type.Should().Be("direct");
            });

            messageBroker.DefineExchanges();

        }

        [Fact]
        public void EachDiscoveredQueueIsCreatedOnRabbitServer()
        {
        }

        [Fact]
        public void IfConsumerJoinsNonFanoutQueueNewBindingCreatedForConsumer()
        {

        }

        [Fact]
        public void IfConsumerAddsQueueNewQueueIsCreatedExclusivelyForConsumer()
        {

        }

        // If the queue requires it's delivered messages to be acknowledged the message header
        // is checked.  The consumer event handler specifies if the message is acknowledged by 
        // calling a method that stores an indicator in the header of the message.  If message
        // is acknowledged, RabbitMq notified.
        [Fact]
        public void IfMessageDeliveryRequiresAck_RabbitToldStatusIfAck()
        {

        }

        // If the queue requires it's delivered messages to be acknowledged the message header
        // is checked.  The consumer event handler specifies if the message is rejected by 
        // calling a method that stores an indicator in the header of the message.  Or simply
        // by not acknowledging the message.  If message is not-acknowledged, RabbitMq notified.
        [Fact]
        public void IfMessageDeliveryRequiresAck_RabbitToldStatusIfNotAck()
        {

        }








        [Fact]
        public void WhenMessageArrivesDispatchedToCorrespondingConsumerHandler()
        {

        }

        [Fact]
        public void IfMessageAssociatedWithExchange_PublishedToRabbitMq()
        {

        }

        [Fact]
        public void ThereCanBeOneOneSerializerRegisteryRegisteredByHost()
        {

        }

        [Fact]
        public void IfExhangeSpecifiedSerializerNotFoundThenException()
        {

        }

        [Fact]
        public void DefaultJsonAndBinarySerializersAddedIfNotSpecifiedByHost()
        {

        }



        [Fact]
        public void MessageIsPublishedToAllAssociatedExchanges()
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

        private void AssertDeclaredExchange(Mock<IModel> mockedModuled, Action<ExchangeValues> values)
        {
            mockedModuled.Setup(m => m.ExchangeDeclare(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()))

                .Callback((string exchange, string type, bool durable, bool autoDelete, IDictionary<string, object> arguments) => {

                    values(new ExchangeValues
                    {
                        Exchange = exchange,
                        Type = type,
                        Durable = durable,
                        AutoDelete = autoDelete,
                        Arguments = arguments
                    });

                });
        }
    }

    public class ExchangeValues
    {
        public string Exchange { get; set; }
        public string Type { get; set; }
        public bool Durable { get; set; }
        public bool AutoDelete { get; set; }
        public IDictionary<string, object> Arguments { get; set; }
    }
}

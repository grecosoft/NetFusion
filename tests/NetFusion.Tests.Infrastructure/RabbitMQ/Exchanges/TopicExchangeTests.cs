using FluentAssertions;
using NetFusion.RabbitMQ;
using NetFusion.Tests.Infrastructure.RabbitMQ.Mocks;
using System;
using System.Linq;
using Xunit;

namespace NetFusion.Tests.Infrastructure.RabbitMQ.Exchanges
{
    public class topicxchangeTests
    {
        [Fact] // Should be recreated if the server is restarted.
        public void DirectExchangeIsDurableByDefault()
        {
            var exchange = new MockTopicExchange();
            exchange.Setup();
            exchange.Settings.IsDurable.Should().BeTrue();
        }

        [Fact] // When the exchange no longer has connected clients, it should not be deleted.
        public void DirectExchangeIsNotAutoDeleteByDefault()
        {
            var exchange = new MockTopicExchange();
            exchange.Setup();
            exchange.Settings.IsAutoDelete.Should().BeFalse();
        }

        [Fact] // Clients much acknowledge the completed processing of message.
        public void DirectExchangeQueuesRequresAckByDefault()
        {
            var exchange = new MockTopicExchange { FirstQueueName = "FirstMockTestQueue" };
            exchange.Setup();

            exchange.Queues.First().Settings.IsNoAck.Should().BeFalse();
        }

        [Fact] // Should be recreated if server is restarted.
        public void DirectExchangeQueuesShouldBeDurrableByDefault()
        {
            var exchange = new MockTopicExchange { FirstQueueName = "FirstMockTestQueue" };
            exchange.Setup();

            exchange.Queues.First().Settings.IsDurable.Should().BeTrue();
        }

        [Fact] // Exchange will deliver message to all queues that have a route key matching key of message.
        public void DirectExchangeQueueMustHaveAtLeastOneRouteKey()
        {
            var exchange = new MockTopicExchange { FirstQueueName = "FirstMockTestQueue", FirstQueueRouteKeys = new string[] { } };

            Assert.Throws<BrokerException>(() => exchange.Setup()).Message
                .Should().Contain("must have a route specified");
        }

        [Fact] // Multiple clients can connect to and monitor queue.
        public void DirectExchangeQueueIsNotExclusiveByDefault()
        {
            var exchange = new MockTopicExchange { FirstQueueName = "FirstMockTestQueue" };
            exchange.Setup();

            exchange.Queues.First().Settings.IsExclusive.Should().BeFalse();
        }
    }
}

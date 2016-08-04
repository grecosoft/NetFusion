using FluentAssertions;
using System;
using System.Linq;
using Xunit;

namespace NetFusion.Tests.Infrastructure.RabbitMQ.Mocks
{
    public class DefaultExchangeTests
    {
        [Fact]
        public void DirectExchangeIsDurableByDefault()
        {
            var exchange = new MockDirectExchange();
            exchange.Setup();
            exchange.Settings.IsDurable.Should().BeTrue();
        }

        [Fact]
        public void DirectExchangeIsNotAutoDeleteByDefault()
        {
            var exchange = new MockDirectExchange();
            exchange.Setup();
            exchange.Settings.IsAutoDelete.Should().BeFalse();
        }

        [Fact]
        public void DirectExchangeQueuesRequresAckByDefault()
        {
            var exchange = new MockDirectExchange { FirstQueueName = "FirstMockTestQueue" };
            exchange.Setup();

            exchange.Queues.First().Settings.IsNoAck.Should().BeFalse();
        }

        [Fact]
        public void DirectExchangeQueuesShouldBeDurrableByDefault()
        {
            var exchange = new MockDirectExchange { FirstQueueName = "FirstMockTestQueue" };
            exchange.Setup();

            exchange.Queues.First().Settings.IsDurable.Should().BeTrue();
        }

        [Fact]
        public void DirectExchangeQueueMustHaveAtLeastOneRouteKey()
        {
            var exchange = new MockDirectExchange { FirstQueueName = "FirstMockTestQueue", FirstQueueRouteKeys = new string[] { } };

            Assert.Throws<InvalidOperationException>(() => exchange.Setup()).Message
                .Should().Contain("queues must have a route specified-Exchange");
        }
    }
}

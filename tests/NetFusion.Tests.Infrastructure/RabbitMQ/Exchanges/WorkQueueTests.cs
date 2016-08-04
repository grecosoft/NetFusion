using FluentAssertions;
using NetFusion.Tests.Infrastructure.RabbitMQ.Mocks;
using System.Linq;
using Xunit;

namespace NetFusion.Tests.Infrastructure.RabbitMQ.Exchanges
{
    public class WorkQueueTests
    {
        [Fact]
        
        public void WorkQueueIsNotAssignedToAnExchange()
        {
            var exchange = new MockWorkQueueExchange { FirstQueueName = "FirstMockTestQueue" };
            exchange.Setup();

            exchange.ExchangeName.Should().BeEmpty();
        }

        [Fact] // Clients much acknowledge the completed processing of message.
        public void WorkQueueRequiresAckByDefault()
        {
            var exchange = new MockWorkQueueExchange { FirstQueueName = "FirstMockTestQueue" };
            exchange.Setup();

            exchange.Queues.First().Settings.IsNoAck.Should().BeFalse();
        }

        [Fact] // Should be recreated if server is restarted.
        public void WorkQueueShouldBeDurrableByDefault()
        {
            var exchange = new MockWorkQueueExchange { FirstQueueName = "FirstMockTestQueue" };
            exchange.Setup();

            exchange.Queues.First().Settings.IsDurable.Should().BeTrue();
        }

        [Fact] // Multiple clients can connect to and monitor queue.
        public void WorkQueueIsNotExclusiveByDefault()
        {
            var exchange = new MockWorkQueueExchange { FirstQueueName = "FirstMockTestQueue" };
            exchange.Setup();

            exchange.Queues.First().Settings.IsExclusive.Should().BeFalse();
        }
    }
}


// Add code to assert that a work queue must have one queue...
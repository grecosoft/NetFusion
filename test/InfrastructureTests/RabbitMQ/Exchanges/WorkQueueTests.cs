using FluentAssertions;
using NetFusion.Tests.Infrastructure.RabbitMQ.Mocks;
using System.Linq;
using Xunit;

namespace InfrastructureTests.RabbitMQ.Exchanges
{
    /// <summary>
    /// Unit tests that assert the default configuration settings
    /// for a Workqueue style of exchange.
    /// </summary>
    public class WorkQueueTests
    {
        /// <summary>
        /// Workqueue style queues are associated with the default exchange.
        /// </summary>
        [Fact]
        public void WorkQueue_NotAssignedToExchange()
        {
            var exchange = new MockWorkQueueExchange { FirstQueueName = "FirstMockTestQueue" };
            exchange.Setup();

            exchange.ExchangeName.Should().BeEmpty();
        }

        /// <summary>
        /// Clients much acknowledge the completed processing of message.
        /// </summary>
        [Fact (DisplayName=nameof(WorkQueueRequires_AckByDefault))] 
        public void WorkQueueRequires_AckByDefault()
        {
            var exchange = new MockWorkQueueExchange { FirstQueueName = "FirstMockTestQueue" };
            exchange.Setup();

            exchange.Queues.First().Settings.IsNoAck.Should().BeFalse();
        }

        /// <summary>
        /// Should be recreated if server is restarted.
        /// </summary>
        [Fact(DisplayName = nameof(WorkQueue_DurableByDefault))]
        public void WorkQueue_DurableByDefault()
        {
            var exchange = new MockWorkQueueExchange { FirstQueueName = "FirstMockTestQueue" };
            exchange.Setup();

            exchange.Queues.First().Settings.IsDurable.Should().BeTrue();
        }

        /// <summary>
        /// Multiple clients can connect to and monitor queue.
        /// </summary>
        [Fact(DisplayName = nameof(WorkQueue_NotExclusiveByDefault))] 
        public void WorkQueue_NotExclusiveByDefault()
        {
            var exchange = new MockWorkQueueExchange { FirstQueueName = "FirstMockTestQueue" };
            exchange.Setup();

            exchange.Queues.First().Settings.IsExclusive.Should().BeFalse();
        }
    }
}
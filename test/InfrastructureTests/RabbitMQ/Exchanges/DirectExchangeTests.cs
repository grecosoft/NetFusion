using FluentAssertions;
using NetFusion.RabbitMQ;
using NetFusion.Tests.Infrastructure.RabbitMQ.Mocks;
using System.Linq;
using Xunit;

namespace InfrastructureTests.RabbitMQ.Exchanges
{
    /// <summary>
    /// Unit tests that assert the default configuration settings
    /// for a direct style of exchange.
    /// </summary>
    public class DirectExchangeTests
    {
        /// <summary>
        /// Should be recreated if the server is restarted.
        /// </summary>
        [Fact (DisplayName = nameof(DirectExchange_DurableByDefault))] 
        public void DirectExchange_DurableByDefault()
        {
            var exchange = new MockDirectExchange();
            exchange.Setup();
            exchange.Settings.IsDurable.Should().BeTrue();
        }

        /// <summary>
        /// When the exchange no longer has connected clients, it should not be deleted.
        /// </summary>
        [Fact (DisplayName = nameof(DirectExchange_NotAutoDeleteByDefault))] 
        public void DirectExchange_NotAutoDeleteByDefault()
        {
            var exchange = new MockDirectExchange();
            exchange.Setup();
            exchange.Settings.IsAutoDelete.Should().BeFalse();
        }

        /// <summary>
        /// Clients much acknowledge the completed processing of message.
        /// </summary>
        [Fact (DisplayName = nameof(DirectExchangeQueues_RequiresAckByDefault))] 
        public void DirectExchangeQueues_RequiresAckByDefault()
        {
            var exchange = new MockDirectExchange { FirstQueueName = "FirstMockTestQueue" };
            exchange.Setup();

            exchange.Queues.First().Settings.IsNoAck.Should().BeFalse();
        }

        /// <summary>
        /// Should be recreated if server is restarted.
        /// </summary>
        [Fact(DisplayName = nameof(DirectExchangeQueues_DurableByDefault))] 
        public void DirectExchangeQueues_DurableByDefault()
        {
            var exchange = new MockDirectExchange { FirstQueueName = "FirstMockTestQueue" };
            exchange.Setup();

            exchange.Queues.First().Settings.IsDurable.Should().BeTrue();
        }

        /// <summary>
        /// Exchange will deliver message to all queues that have a route key matching key of message.
        /// </summary>
        [Fact (DisplayName = nameof(DirectExchangeQueues_MustHaveAtLeastOneRouteKey))] 
        public void DirectExchangeQueues_MustHaveAtLeastOneRouteKey()
        {
            var exchange = new MockDirectExchange { FirstQueueName = "FirstMockTestQueue", FirstQueueRouteKeys = new string[] { } };

            Assert.Throws<BrokerException>(() => exchange.Setup()).Message
                .Should().Contain("must have a route specified");
        }

         /// <summary>
         /// Multiple clients can connect to and monitor queue.
         /// </summary>
        [Fact (DisplayName = nameof(DirectExchangeQueues_NotExclusiveByDefault))]
        public void DirectExchangeQueues_NotExclusiveByDefault()
        {
            var exchange = new MockDirectExchange { FirstQueueName = "FirstMockTestQueue" };
            exchange.Setup();

            exchange.Queues.First().Settings.IsExclusive.Should().BeFalse();
        }
    }
}

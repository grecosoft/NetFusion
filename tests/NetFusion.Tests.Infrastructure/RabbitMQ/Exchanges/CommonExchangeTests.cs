using FluentAssertions;
using NetFusion.Tests.Infrastructure.RabbitMQ.Mocks;
using System.Linq;
using Xunit;

namespace NetFusion.Tests.Infrastructure.RabbitMQ.Exchanges
{
    public class CommonExchangeTests
    {
        [Fact]
        public void JsonSerializationUsedByDefault()
        {
            var exchange = new MockDirectExchange();
            exchange.Setup();
            exchange.Settings.ContentType.Should().Be("application/json; charset=utf-8");
        }

        [Fact]
        public void BrokerNameIsSet()
        {
            var exchange = new MockDirectExchange();
            exchange.Setup();
            exchange.Settings.BrokerName.Should().Be("MockTestBrokerName");
        }

        [Fact]
        public void QueuesCanBeCreatedWhenExchangeCreated()
        {
            var exchange = new MockDirectExchange { FirstQueueName = "FirstMockTestQueue" };
            exchange.Setup();
            exchange.Queues.Should().HaveCount(1);
            exchange.Queues.First().QueueName.Should().Be("FirstMockTestQueue");
        }

        [Fact]
        public void ExchangeCanHaveMultipleDefaultQueues()
        {
            var exchange = new MockDirectExchange { FirstQueueName = "FirstMockTestQueue", SecondQueueName = "SecondMockTestQueue" };
            exchange.Setup();
            exchange.Queues.Should().HaveCount(2);
        }
    }
}

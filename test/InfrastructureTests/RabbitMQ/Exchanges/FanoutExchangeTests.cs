using FluentAssertions;
using NetFusion.Tests.Infrastructure.RabbitMQ.Mocks;
using Xunit;

namespace InfrastructureTests.RabbitMQ.Exchanges
{
    /// <summary>
    /// Unit tests that assert the default configuration settings
    /// for a Fanout style of exchange.
    /// </summary>
    public class FanoutExchangeTests
    {
        /// <summary>
        /// Should be recreated if the server is restarted.
        /// </summary>
        [Fact (DisplayName = nameof(FanoutExchange_DurableByDefault))]
        public void FanoutExchange_DurableByDefault()
        {
            var exchange = new MockFanoutExchange();
            exchange.Setup();
            exchange.Settings.IsDurable.Should().BeTrue();
        }

        /// <summary>
        /// When the exchange no longer has connected clients, it should not be deleted.
        /// </summary>
        [Fact (DisplayName = nameof(FanoutExchange_NotAutoDeleteByDefault))] 
        public void FanoutExchange_NotAutoDeleteByDefault()
        {
            var exchange = new MockFanoutExchange();
            exchange.Settings.IsAutoDelete.Should().BeFalse();
            exchange.Setup();
        }
    }
}

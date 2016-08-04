using FluentAssertions;
using NetFusion.Tests.Infrastructure.RabbitMQ.Mocks;
using Xunit;

namespace NetFusion.Tests.Infrastructure.RabbitMQ.Exchanges
{
    public class FanoutExchangeTests
    {
        [Fact] // Should be recreated if the server is restarted.
        public void FanoutExchangeIsDurableByDefault()
        {
            var exhange = new MockFanoutExchange();
            exhange.Setup();

            exhange.Settings.IsDurable.Should().BeTrue();
        }

        [Fact] // When the exchange no longer has connected clients, it should not be deleted.
        public void FanoutExchangeIsNotAutoDeleteByDefault()
        {
            var exhange = new MockFanoutExchange();
            exhange.Settings.IsAutoDelete.Should().BeFalse();
            exhange.Setup();
        }
    }
}

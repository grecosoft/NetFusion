using Moq;
using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Core;
using RabbitMQ.Client;

namespace InfrastructureTests.RabbitMQ.Exchanges
{
    /// <summary>
    /// Exchange unit-test initialization methods.
    /// </summary>
    public static class ExchangeTestExtensions
    {
        /// <summary>
        /// Makes sure the exchange has been initialized.  This method
        /// should be invoked before asserting the exchange in a test.
        /// </summary>
        /// <param name="value"></param>
        public static void Setup(this IMessageExchange value)
        {
            var mockConn = new Mock<IModel>();

            value.InitializeSettings(new BrokerSettings());
            value.Declare(mockConn.Object);
        }
    }
}

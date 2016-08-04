using Moq;
using NetFusion.RabbitMQ.Exchanges;
using RabbitMQ.Client;

namespace NetFusion.Tests.Infrastructure.RabbitMQ
{
    public static class ExchangeTestExtensions
    {
        public static void Setup(this IMessageExchange value)
        {
            var mockConn = new Mock<IModel>();

            value.InitializeSettings();
            value.Declare(mockConn.Object);
        }
    }
}

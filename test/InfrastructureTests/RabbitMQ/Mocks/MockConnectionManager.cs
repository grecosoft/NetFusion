using Microsoft.Extensions.Logging;
using Moq;
using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Core.Initialization;
using RabbitMQ.Client;
using System.Collections.Generic;

namespace NetFusion.Tests.Infrastructure.RabbitMQ.Mocks
{
    public class MockConnectionManager : ConnectionManager
    {
        public Mock<IConnection> MockConn { get; }
        public Mock<IModel> MockChannel { get; }

        public MockConnectionManager(
            ILoggerFactory logger,
            BrokerSettings brokerSettings,
            IDictionary<string, object> clientProperties) :
            base(logger, brokerSettings, clientProperties)
        {
            this.MockConn = new Mock<IConnection>();
            this.MockChannel = new Mock<IModel>();

            this.MockConn.Setup(c => c.CreateModel())
               .Returns(MockChannel.Object);
        }

        protected override void ConnectToBroker(BrokerConnection brokerConn)
        {
            brokerConn.Connection = MockConn.Object;
        }
    }
}

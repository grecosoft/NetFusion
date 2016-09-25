using Moq;
using NetFusion.Bootstrap.Logging;
using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Core.Initialization;
using RabbitMQ.Client;
using System.Collections.Generic;

namespace NetFusion.Tests.Infrastructure.RabbitMQ.Mocks
{
    public class MockConnectionManager : ConnectionManager
    {
        public Mock<IConnection> MockConn { get; }
        public Mock<IModel> MockModule { get; }

        public MockConnectionManager(
            IContainerLogger logger,
            BrokerSettings brokerSettings,
            IDictionary<string, object> clientProperties) :
            base(logger, brokerSettings, clientProperties)
        {
            this.MockConn = new Mock<IConnection>();
            this.MockModule = new Mock<IModel>();

            this.MockConn.Setup(c => c.CreateModel())
               .Returns(MockModule.Object);
        }

        protected override void ConnectToBroker(BrokerConnection brokerConn)
        {
            brokerConn.Connection = MockConn.Object;
        }
    }
}

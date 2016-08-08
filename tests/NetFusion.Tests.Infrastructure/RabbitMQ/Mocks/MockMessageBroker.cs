using Moq;
using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Core;
using RabbitMQ.Client;

namespace NetFusion.Tests.Infrastructure.RabbitMQ.Mocks
{
    public class MockMessageBroker : MessageBroker
    {
        private readonly Mock<IConnection> _mockConnection;
    
        public MockMessageBroker(Mock<IConnection> mockConnection)
        {
            _mockConnection = mockConnection;
        }

        protected override void ConnectToBroker(BrokerConnection brokerConn)
        {
            brokerConn.Connection = _mockConnection.Object;
        }
    }
}

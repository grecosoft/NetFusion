using Moq;
using NetFusion.Messaging.Modules;
using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Core;
using RabbitMQ.Client;

namespace NetFusion.Tests.Infrastructure.RabbitMQ.Mocks
{
    public class MockMessageBroker : MessageBroker
    {
        private readonly Mock<IConnection> _mockConnection;
    
        public MockMessageBroker(Mock<IMessagingModule> mockMsgModule, Mock<IConnection> mockConnection) :
            base(mockMsgModule.Object)
        {
            _mockConnection = mockConnection;
        }

        protected override void ConnectToBroker(BrokerConnection brokerConn)
        {
            brokerConn.Connection = _mockConnection.Object;
        }
    }
}

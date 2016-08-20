using Moq;
using NetFusion.Bootstrap.Logging;
using NetFusion.Domain.Scripting;
using NetFusion.Messaging.Modules;
using NetFusion.RabbitMQ.Configs;
using NetFusion.RabbitMQ.Core;
using NetFusion.RabbitMQ.Core;
using RabbitMQ.Client;

namespace NetFusion.Tests.Infrastructure.RabbitMQ.Mocks
{
    public class MockMessageBroker : MessageBroker
    {
        private readonly Mock<IConnection> _mockConnection;
    
        public MockMessageBroker(
            NullLogger logger, 
            Mock<IMessagingModule> mockMsgModule, 
            Mock<IConnection> mockConnection) :

            base(logger, mockMsgModule.Object,
                new Mock<IBrokerMetaRepository>().Object,
                new Mock<IEntityScriptingService>().Object)
        {
            _mockConnection = mockConnection;
        }

        protected override void ConnectToBroker(BrokerConnection brokerConn)
        {
            brokerConn.Connection = _mockConnection.Object;
        }
    }
}

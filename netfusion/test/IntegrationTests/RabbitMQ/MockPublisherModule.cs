using Moq;
using NetFusion.RabbitMQ.Plugin;
using NetFusion.RabbitMQ.Publisher.Internal;

namespace IntegrationTests.RabbitMQ
{
    using NetFusion.RabbitMQ.Metadata;

    public class MockPublisherModule : PublisherModule
    {
       protected override IRpcClient CreateRpcClient(ExchangeMeta definition)
       {
           Mock<IRpcClient> mockRpcClient = new Mock<IRpcClient>();
           return mockRpcClient.Object;
       }
    }
}
//using Moq;
//using NetFusion.RabbitMQ.Modules;
//using NetFusion.RabbitMQ.Publisher;
//using NetFusion.RabbitMQ.Publisher.Internal;
//
//namespace IntegrationTests.RabbitMQ
//{
//    public class MockPublisherModule : PublisherModule
//    {
//       protected override IRpcClient CreateRpcClient(ExchangeDefinition definition)
//       {
//           Mock<IRpcClient> mockRpcClient = new Mock<IRpcClient>();
//           return mockRpcClient.Object;
//       }
//    }
//}
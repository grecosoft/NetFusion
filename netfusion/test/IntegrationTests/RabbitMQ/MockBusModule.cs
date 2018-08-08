//using System.Collections.Generic;
//using EasyNetQ;
//using Moq;
//using NetFusion.RabbitMQ.Modules;
//
//namespace IntegrationTests.RabbitMQ
//{
//    public class MockBusModule : BusModule
//    {
//        public List<ConnectionConfiguration> ConnConfigs { get; }
//        private Mock<IBus> MockBus = new Mock<IBus>();
//
//        public MockBusModule()
//        {
//            ConnConfigs = new List<ConnectionConfiguration>();
//            BusFactory = CreateBus;
//        }
//
//        public IBus CreateBus(ConnectionConfiguration config)
//        {
//            ConnConfigs.Add(config);
//            return MockBus.Object;
//        }
//    }
//}
using System.Collections.Generic;
using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NetFusion.Base.Serialization;
using NetFusion.RabbitMQ.Plugin.Modules;
using NetFusion.Serialization;

namespace IntegrationTests.RabbitMQ
{
    public class MockBusModule : BusModule
    {
        public List<ConnectionConfiguration> ConnConfigs { get; }
        private readonly Mock<IBus> _mockBus = new Mock<IBus>();

        public MockBusModule()
        {
            ConnConfigs = new List<ConnectionConfiguration>();
            BusFactory = CreateBus;
        }
        
        public override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<ISerializationManager, SerializationManager>();
        }

        public IBus CreateBus(ConnectionConfiguration config)
        {
            ConnConfigs.Add(config);
            return _mockBus.Object;
        }
    }
}
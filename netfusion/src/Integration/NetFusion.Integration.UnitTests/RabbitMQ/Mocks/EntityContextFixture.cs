using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NetFusion.Common.Base.Serialization;
using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.TestFixtures.Plugins;
using NetFusion.Integration.RabbitMQ.Bus;
using NetFusion.Integration.RabbitMQ.Plugin;
using NetFusion.Integration.RabbitMQ.Plugin.Configs;
using NetFusion.Integration.RabbitMQ.Plugin.Settings;
using NetFusion.Messaging;
using NetFusion.Messaging.Logging;

namespace NetFusion.Integration.UnitTests.RabbitMQ.Mocks;

public class EntityContextFixture
{
    // Mocking Infrastructure:
    private readonly Mock<IBusModule> _mockBusModule = new();

    public Mock<IBusConnection> MockConnection { get; } = new();
    public Mock<ISerializationManager> MockSerializationMgr { get; } = new();
    public Mock<IMessageDispatcherService> MockDispatcher { get; } = new();

    private readonly IServiceCollection _serviceCollection = new ServiceCollection();
    
    public EntityContextFixture()
    {
        _serviceCollection.AddSingleton<ILoggerFactory, LoggerFactory>();
        _mockBusModule.Setup(m => m.RabbitMqConfig).Returns(new RabbitMqConfig { IsAutoCreateEnabled = true });
    }

    public EntityContext CreateContext(ConnectionSettings? settings = null)
    {
        var builder = new Mock<ICompositeAppBuilder>();
        builder.Setup(m => m.HostPlugin).Returns(new MockHostPlugin());
        
        var externalSettings = new ExternalEntitySettings(settings ?? new ConnectionSettings());

        MockConnection.Setup(m => m.ExternalSettings).Returns(externalSettings);
        
        // Have the Bus Module return the connection:
        _mockBusModule.Setup(m => m.GetConnection(It.IsAny<string>())).Returns(MockConnection.Object);
     
        // Add the dependent 
        _serviceCollection.AddSingleton(_mockBusModule.Object);
        _serviceCollection.AddSingleton(MockSerializationMgr.Object);
        _serviceCollection.AddSingleton(MockDispatcher.Object);
        _serviceCollection.AddSingleton(new Mock<IMessageLogger>().Object);

        return new EntityContext(new MockHostPlugin(), _serviceCollection.BuildServiceProvider());
    }
}

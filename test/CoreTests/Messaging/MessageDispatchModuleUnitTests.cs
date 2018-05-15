using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Messaging;
using NetFusion.Messaging.Core;
using NetFusion.Messaging.Enrichers;
using NetFusion.Messaging.Modules;
using NetFusion.Messaging.Types;
using NetFusion.Test.Modules;
using Xunit;

namespace CoreTests.Messaging
{
    public class MessageDispatchModuleUnitTests
    {
        [Fact]
        public void AllMessageConsumers_Discovered()
        {
            // Arrange
            var module = ModuleTestFixture.SetupModule<MessageDispatchModule>(
                typeof(MessageConsumerOne), 
                typeof(MessageConsumerTwo));
          
            // Act
            module.Initialize();
            
            // Assert
            Assert.NotNull(module.AllMessageTypeDispatchers);
            Assert.True(module.AllMessageTypeDispatchers.Contains(typeof(MockCommand)));
            Assert.True(module.AllMessageTypeDispatchers.Contains(typeof(MockDomainEvent)));
            
            Assert.NotNull(module.InProcessDispatchers);
            Assert.True(module.InProcessDispatchers.Contains(typeof(MockCommand)));
            Assert.True(module.InProcessDispatchers.Contains(typeof(MockDomainEvent)));
          
        }

        [Fact]
        public void InProcessMessageDispatcher_AddedByDefault()
        {
            // Arrange
            var module = ModuleTestFixture.SetupModule<MessageDispatchModule>();

            // Act
            module.Initialize();
            
            // Assert:
            Assert.Equal(1, module.DispatchConfig.PublisherTypes.Count);
            Assert.Equal(typeof(InProcessMessagePublisher), module.DispatchConfig.PublisherTypes.First());
        }

        [Fact]
        public void DateRecievedEnricher_AddedByDefault()
        {
            // Arrange
            var module = ModuleTestFixture.SetupModule<MessageDispatchModule>();

            // Act
            module.Initialize();
            
            // Assert
            Assert.True(module.DispatchConfig.EnricherTypes.Contains(typeof(DateReceivedEnricher)));
        }

        [Fact]
        public void CorrelationEnricher_AddedByDefault()
        {
            // Arrange
            var module = ModuleTestFixture.SetupModule<MessageDispatchModule>();

            // Act
            module.Initialize();
            
            // Assert
            Assert.True(module.DispatchConfig.EnricherTypes.Contains(typeof(CorrelationEnricher)));
        }
        
        [Fact]
        public void AllMessagePublishes_AddedAsScoped_ToServiceCollection()
        {
            // Arrange
            var module = ModuleTestFixture.SetupModule<MessageDispatchModule>();

            var services = new ServiceCollection();
            
            // Act
            module.Initialize();
            module.RegisterServices(services);
            
            // Assert
            var dispatchConfig = module.DispatchConfig;

            var registeredServices = services.Where(sd => 
                    dispatchConfig.PublisherTypes.Contains(sd.ImplementationType) && 
                    sd.Lifetime == ServiceLifetime.Scoped &&
                    sd.ServiceType == typeof(IMessagePublisher))
                .ToArray();
            
            Assert.Equal(module.DispatchConfig.PublisherTypes.Count, registeredServices.Length);            
        }

        [Fact]
        public void AllMessageConsumers_AddedAsScoped_ToServiceCollection()
        {
            // Arrange
            var module = ModuleTestFixture.SetupModule<MessageDispatchModule>();
            
            var catalog = CatalogTestFixture.Setup(
                typeof(MessageConsumerOne), 
                typeof(MessageConsumerTwo));
            
            // Act
            module.ScanAllOtherPlugins(catalog);
            
            // Assert
            Assert.Equal(2, catalog.Services.Count);
            Assert.True(catalog.Services.All(sd => sd.Lifetime == ServiceLifetime.Scoped));
            
            Assert.True(catalog.Services.Any(sd => 
                sd.ServiceType == typeof(MessageConsumerOne) && 
                sd.ImplementationType == typeof(MessageConsumerOne)));
            
            Assert.True(catalog.Services.Any(sd => 
                sd.ServiceType == typeof(MessageConsumerTwo) && 
                sd.ImplementationType == typeof(MessageConsumerTwo)));
        }

        public class MessageConsumerOne : IMessageConsumer
        {
            [InProcessHandler]
            public int OnCommand(MockCommand command)
            {
                Assert.NotNull(command);
                return 1000;
            }
        }
        
        public class MessageConsumerTwo : IMessageConsumer
        {
            [InProcessHandler]
            public void OnDomainEvent(MockDomainEvent domainEvt)
            {
                Assert.NotNull(domainEvt);
            }
        }


        public class MockCommand : Command<int>
        {
            
        }

        public class MockDomainEvent : DomainEvent
        {
            
        }
    }
}
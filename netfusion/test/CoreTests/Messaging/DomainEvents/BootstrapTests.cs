using System.Linq;
using CoreTests.Messaging.DomainEvents.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Messaging.Plugin.Modules;
using NetFusion.Test.Container;
using Xunit;

// ReSharper disable All

namespace CoreTests.Messaging.DomainEvents
{
    /// <summary>
    /// Unit test asserting that the message types and message consumers are
    /// correctly found by the messaging module during the bootstrap process.
    /// </summary>
    public class BootstrapTests
    {
        /// <summary>
        /// The MessageDispatchModule will discover all defined message handlers within all available plug-ins.
        /// The module's AllMessageTypeDispatchers property contains all of the meta-data required to dispatch a
        /// message at runtime to the correct consumer handlers.
        /// </summary>
        [Fact]
        public void MessagingModule_Discovers_DomainEventsWithConsumers()
        {
            ContainerFixture.Test(fixture => { fixture
                .Arrange.Container(c => c.AddHost().WithDomainEventHandler())
                .Assert.PluginModule<MessageDispatchModule>(m =>
                {
                    var dispatchInfo = m.AllMessageTypeDispatchers[typeof(MockDomainEvent)].FirstOrDefault();
                    dispatchInfo.Should().NotBeNull(); 
                });
            });           
        }

        /// <summary>
        /// The MessageDispatchModule will discover all defined domain event consumer classes with methods that 
        /// handle a given message.  Application components are scanned for event handler methods during the 
        /// bootstrap process by implementing the IMessageConsumer marker interface and marking their messages
        /// handlers with the InProcessHandler attribute.
        /// </summary>
        [Fact]
        public void MessagingModule_Discovers_DomainEventConsumers()
        {
            ContainerFixture.Test(fixture => { fixture
                .Arrange.Container(c => c.AddHost().WithDomainEventHandler())
                .Assert.PluginModule<MessageDispatchModule>(m =>
                {
                    var dispatchInfo = m.InProcessDispatchers[typeof(MockDomainEvent)].FirstOrDefault();
                    dispatchInfo.Should().NotBeNull();

                    dispatchInfo.MessageType.Should().Be(typeof(MockDomainEvent));
                    dispatchInfo.ConsumerType.Should().Be(typeof(MockDomainEventConsumer));
                    
                    Assert.Equal(
                        typeof(MockDomainEventConsumer).GetMethod("OnEventHandlerOne"),
                        dispatchInfo.MessageHandlerMethod);
                });
            });    
        }

        /// <summary>
        /// All discovered messages consumers are registered in the dependency injection container.
        /// When an event is published, the domain event service resolves the consumer type and
        /// obtains a reference from the container using the cached dispatch information.
        /// </summary>
        [Fact]
        public void DomainEventConsumer_Registered()
        {
            ContainerFixture.Test(fixture => { fixture
                .Arrange.Container(c => c.AddHost().WithDomainEventHandler())
                .Assert.ServiceCollection(sc =>
                {
                    sc.FirstOrDefault(s =>
                            s.ImplementationType == typeof(MockDomainEventConsumer) &&
                            s.Lifetime == ServiceLifetime.Scoped)
                        .Should().NotBeNull();
                })
                .Services(s =>
                {
                    s.GetService<MockDomainEventConsumer>().Should().NotBeNull();
                });
            });                
        }
    }
}

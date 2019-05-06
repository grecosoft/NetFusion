using System.Linq;
using CoreTests.Messaging.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Messaging.Plugin.Modules;
using NetFusion.Test.Container;
using Xunit;

namespace CoreTests.Messaging.Bootstrap
{
    /// <summary>
    /// Unit test asserting that the message types and message consumer were
    /// correctly found by the messaging module during the bootstrap process.
    /// </summary>
    public class DiscoveryTests
    {
        /// <summary>
        /// The MessageDispatchModule will discover all defined message handlers within
        /// all available plug-ins.  The module's AllMessageTypeDispatchers property
        /// contains all of the meta-data required to dispatch a message at runtime to
        /// the correct consumer handlers.
        /// </summary>
        [Fact(DisplayName = "Messaging Module discovers Domain Event")]
        public void MessagingModule_DiscoversMessageConsumers()
        {
            ContainerFixture.Test(fixture => { fixture
                .Arrange
                .Container(c => c.WithHostConsumer())

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
        [Fact(DisplayName = nameof(Discovers_DomainEventConsumerHandler))]
        public void Discovers_DomainEventConsumerHandler()
        {
            ContainerFixture.Test(fixture => { fixture
                .Arrange
                .Container(c => c.WithHostConsumer())
   
                .Assert.PluginModule<MessageDispatchModule>(m =>
                {
                    var dispatchInfo = m.InProcessDispatchers[typeof(MockDomainEvent)].FirstOrDefault();
                    dispatchInfo.Should().NotBeNull();

                    if (dispatchInfo == null) return;

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
        [Fact(DisplayName = nameof(DomainEventConsumer_Registered))]
        public void DomainEventConsumer_Registered()
        {
            ContainerFixture.Test(fixture => { fixture
                .Arrange
                .Container(c => c.WithHostConsumer())

                .Assert.Services(s =>
                {
                    var consumer = s.GetService<MockDomainEventConsumer>();
                    consumer.Should().NotBeNull();
                });
            });                
        }
    }
}

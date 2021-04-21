using System.Linq;
using CoreTests.Messaging.DomainEvents;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Messaging;
using NetFusion.Messaging.Internal;
using NetFusion.Messaging.Plugin.Configs;
using NetFusion.Test.Container;
using Xunit;
// ReSharper disable All

namespace CoreTests.Messaging.Bootstrap
{
    /// <summary>
    /// Tests that assert that the messaging plug-in was correctly initialized
    /// by the bootstrap process.
    /// </summary>
    public class InitializationTests
    {
        /// <summary>
        /// Unless cleared, the InProcessEventDispatcher will be used by default
        /// to dispatch events to local consumer event handlers.  This is also the
        /// default configuration if the host does not provide one.
        /// </summary>
        [Fact]
        public void InProcessEventDispatcher_Configured_ByDefault()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture
                    .Arrange.Container(c => c.AddMessagingHost())
                    .Assert.Configuration((MessageDispatchConfig config) =>
                    {
                        // The In-Message message publisher is registered by default.
                        // Other messaging plugins such as RabbitMQ and ServiceBus 
                        // can add corresponding publishers.
                        config.PublisherTypes.Should().HaveCount(1);
                        config.PublisherTypes.Should().Contain(typeof(InProcessMessagePublisher));
                    })
                    .ServiceCollection(sc =>
                    {
                        // Publishers are created per request scope:
                        var service = sc.FirstOrDefault(s => s.ImplementationType == typeof(InProcessMessagePublisher));
                        service.Should().NotBeNull();
                        service.Lifetime.Should().Be(ServiceLifetime.Scoped);
                    })
                    .Services(s =>
                    {
                        // Publishers are registered using common interface:
                        var publisher = s.GetServices<IMessagePublisher>();
                        publisher.Should().NotBeNull();
                    });
                });
        }

        /// <summary>
        /// The plug-in registers a service that can be used to publish messages.
        /// </summary>
        [Fact(DisplayName = nameof(RegistersService_ForPublishingEvents))]
        public void RegistersService_ForPublishingEvents()
        {
            ContainerFixture.Test(fixture => { 
                fixture
                    .Arrange.Container(c => c.AddMessagingHost().WithDomainEventHandler())                
                    .Assert.Services(s =>
                    {
                        var service = s.GetService<IMessagingService>();
                        service.Should().NotBeNull();
                    });
            });
        }
    }
}

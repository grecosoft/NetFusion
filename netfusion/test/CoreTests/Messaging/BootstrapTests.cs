using System.Linq;
using CoreTests.Messaging.DomainEvents;
using CoreTests.Messaging.DomainEvents.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Messaging;
using NetFusion.Messaging.Internal;
using NetFusion.Messaging.Plugin.Configs;
using NetFusion.Messaging.Plugin.Modules;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using Xunit;

// ReSharper disable All

namespace CoreTests.Messaging
{
    /// <summary>
    /// Tests that assert that the messaging plug-in was correctly initialized
    /// by the bootstrap process.
    /// </summary>
    public class BootstrapTests
    {
        /// <summary>
        /// When the messaging plugin is added to the composite application, all messaging
        /// associated modules and configurations are added.
        /// </summary>
        [Fact]
        public void AllMessagingModules_Added_ToContainer()
        {
            ContainerFixture.Test(fixture =>
            {
                fixture
                    .Arrange.Container(c => c.AddMessagingHost())
                    .Assert.Configuration<MessageDispatchConfig>(_ => { })
                    .Configuration<QueryDispatchConfig>(_ => { })
                    .PluginModule<MessagingModule>(_ => { })
                    .PluginModule<MessageDispatchModule>(_ => { })
                    .PluginModule<MessageEnricherModule>(_ => { })
                    .PluginModule<QueryDispatchModule>(_ => { })
                    .PluginModule<QueryFilterModule>(_ => { });
            });
        }

        /// <summary>
        /// Unless cleared, the InProcessEventDispatcher will be used by default
        /// to dispatch events to local consumer event handlers.  This is also the
        /// default configuration if the host does not provide one.
        /// </summary>
        [Fact]
        public void InProcessPublisher_Configured_ByDefault()
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
                        var service = sc.SingleOrDefault(s => s.ImplementationType == typeof(InProcessMessagePublisher));
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
        [Fact]
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

        [Fact]
        public void MessageConsumers_Registered()
        {
            ContainerFixture.Test(fixture => { 
                fixture
                    .Arrange.Container(c => c.AddMessagingHost().WithDomainEventHandler())
                    .Assert.ServiceCollection(sc =>
                    {
                        var service = sc.SingleOrDefault(s => s.ImplementationType == typeof(MockDomainEventConsumer));
                        service.Should().NotBeNull();
                        service.Lifetime.Should().Be(ServiceLifetime.Scoped);
                    })
                    .Services(s =>
                    {
                        var service = s.GetService<MockDomainEventConsumer>();
                        service.Should().NotBeNull();
                    });
            });
        }
    }
}

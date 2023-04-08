using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Messaging.InProcess;
using NetFusion.Messaging.Internal;
using NetFusion.Messaging.Plugin.Configs;
using NetFusion.Messaging.Plugin.Modules;
using NetFusion.Messaging.UnitTests;
using NetFusion.Messaging.UnitTests.DomainEvents;
using NetFusion.Messaging.UnitTests.DomainEvents.Mocks;

// ReSharper disable All

namespace NetFusion.Messaging.Tests;

/// <summary>
/// Tests that assert that the messaging plug-in was correctly initialized by the bootstrap process.  
/// </summary>
public class BootstrapTests
{
    /// <summary>
    /// When the messaging plugin is added to the composite application, all messaging associated modules
    /// and configurations are added. 
    /// </summary>
    [Fact]
    public void AllMessagingModules_Added_ToContainer()
    {
        ContainerFixture.Test(fixture =>
        {
            fixture
                .Arrange.Container(c => c.AddMessagingHost())
                .Assert
                .Configuration<MessageDispatchConfig>(_ => { })
                .PluginModule<MessageDispatchModule>(_ => { })
                .PluginModule<MessageEnricherModule>(_ => { });
        });
    }
        

    /// <summary>
    /// Unless cleared, the InProcessEventDispatcher will be used by default
    /// to dispatch command and domain-event messages to local consumers.  
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
                    // add corresponding publishers.
                    config.MessagePublishers.Should().HaveCount(1);
                    config.MessagePublishers.Should().Contain(typeof(MessagePublisher));
                })
                .ServiceCollection(sc =>
                {
                    // Publishers are created per request scope:
                    var service = sc.SingleOrDefault(s => s.ImplementationType == typeof(MessagePublisher));
                    service.Should().NotBeNull();
                    service.ServiceType.Should().Be(typeof(IMessagePublisher));
                    service.Lifetime.Should().Be(ServiceLifetime.Scoped);
                })
                .Services(s =>
                {
                    // Publishers are registered using common interface:
                    var publisher = s.GetServices<IMessagePublisher>();
                    publisher.Should().NotBeNull().And.HaveCount(1);
                });
        });
    }
        
    // /// <summary>
    // /// The MessageDispatchModule will discover all command and domain-event handlers, invoked at runtime, when a
    // /// command or domain-event is dispatched.  The module's AllMessageTypeDispatchers property contains all of the
    // /// meta-data required to dispatch a message at runtime to the correct consumer handlers. 
    // /// </summary>
    // [Fact]
    // public void MessagingModule_Discovers_MessagesWithConsumers()
    // {
    //     ContainerFixture.Test(fixture => { fixture
    //         .Arrange.Container(c => c.AddMessagingHost().WithSyncDomainEventHandler())
    //         .Assert.PluginModule<MessageDispatchModule>(m =>
    //         {
    //             var dispatchInfo = m.AllMessageTypeDispatchers[typeof(MockDomainEvent)].SingleOrDefault();
    //             dispatchInfo.Should().NotBeNull();
    //         });
    //     });           
    // }

    // /// <summary>
    // /// Components are scanned during the bootstrap process and identified as being a message handler by
    // /// implementing the IMessageConsumer marker interface and specifying the InProcessHandler attribute
    // /// on the method handler.
    // /// </summary>
    // [Fact]
    // public void MessagingModule_Discovers_MessageConsumers()
    // {
    //     ContainerFixture.Test(fixture => { fixture
    //         .Arrange.Container(c => c.AddMessagingHost().WithSyncDomainEventHandler())
    //         .Assert.PluginModule<MessageDispatchModule>(m =>
    //         {
    //             var dispatchInfo = m.InProcessDispatchers[typeof(MockDomainEvent)].FirstOrDefault();
    //             dispatchInfo.Should().NotBeNull();
    //
    //             dispatchInfo.MessageType.Should().Be(typeof(MockDomainEvent));
    //             dispatchInfo.ConsumerType.Should().Be(typeof(MockSyncDomainEventConsumerOne));
    //             dispatchInfo.MessageHandlerMethod.Should().BeSameAs(
    //                 typeof(MockSyncDomainEventConsumerOne).GetMethod("OnEventHandler", new[] {typeof(MockDomainEvent) }));
    //         });
    //     });    
    // }

    /// <summary>
    /// All discovered command and domain-event consumers are registered in the dependency injection container
    /// as scoped components.
    /// </summary>
    [Fact]
    public void MessageConsumer_Registered()
    {
        ContainerFixture.Test(fixture => { fixture
            .Arrange.Container(c => c.AddMessagingHost().WithSyncDomainEventHandler())
            .Assert.ServiceCollection(sc =>
            {
                sc.FirstOrDefault(s =>
                        s.ImplementationType == typeof(MockSyncDomainEventConsumerOne) &&
                        s.Lifetime == ServiceLifetime.Scoped)
                    .Should().NotBeNull();
            })
            .Services(s =>
            {
                s.GetService<MockSyncDomainEventConsumerOne>().Should().NotBeNull();
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
                .Arrange.Container(c => c.AddMessagingHost())                
                .Assert.Services(s =>
                {
                    var service = s.GetService<IMessagingService>();
                    service.Should().NotBeNull();
                });
        });
    }
}
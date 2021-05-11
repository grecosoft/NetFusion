using System.Threading.Tasks;
using CoreTests.Messaging.DomainEvents.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Messaging;
using NetFusion.Test.Container;
using Xunit;

namespace CoreTests.Messaging.DomainEvents
{
    /// <summary>
    /// Tests for asserting the publishing and handling of domain-event messages.
    /// </summary>
    public class DispatchTests
    {
        /// <summary>
        /// A domain-event can be handled by a synchronous message handler. 
        /// </summary>
        [Fact]
        public Task DomainEventConsumer_SyncHandlerInvoked()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost().WithSyncDomainEventHandler())
                    .Act.OnServicesAsync(async s =>
                    {
                        var mockEvt = new MockDomainEvent();
                        await s.GetRequiredService<IMessagingService>()
                            .PublishAsync(mockEvt);
                    });

                testResult.Assert.Service<IMockTestLog>(log =>
                {
                    log.Entries.Should().HaveCount(1);
                    log.Entries.Should().Contain("Sync-DomainEvent-Handler-1");
                });
            });
        }

        /// <summary>
        /// A domain-event can be handled by an asynchronous message handler.
        /// </summary>
        [Fact]
        public Task DomainEventConsumer_AsyncHandlerInvoked()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost().WithAsyncDomainEventHandler())
                    .Act.OnServicesAsync(async s =>
                    {
                        var mockEvt = new MockDomainEvent();
                        await s.GetRequiredService<IMessagingService>()
                            .PublishAsync(mockEvt);
                    });
                
                testResult.Assert.Service<IMockTestLog>(log =>
                {
                    log.Entries.Should().HaveCount(1);
                    log.Entries.Should().Contain("Async-DomainEvent-Handler-1");
                });
            });
        }
        
        /// <summary>
        /// Published domain-events can be handled by multiple application component event handlers.
        /// The invoked handlers can be a combination of synchronous and asynchronous method.
        /// </summary>
        [Fact]
        public Task DomainEvent_CanHaveMultiple_AsyncAndSyncConsumers()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost().WithMultipleDomainEventHandlers())
                    .Act.OnServicesAsync(async s =>
                    {
                        var evt = new MockDomainEvent();
                        await s.GetRequiredService<IMessagingService>()
                            .PublishAsync(evt);
                    });

                testResult.Assert.Service<IMockTestLog>(log =>
                {
                    log.Entries.Should().HaveCount(4);
                    log.Entries.Should().BeEquivalentTo(
                        "Sync-DomainEvent-Handler-1", 
                        "Sync-DomainEvent-Handler-2",
                        "Async-DomainEvent-Handler-1", 
                        "Async-DomainEvent-Handler-2");
                });
            });     
        }

        /// <summary>
        /// A consumer event handler for a base domain event type can be marked with the IncludeDerivedEvents
        /// attribute to indicate it should be called for any derived domain events.
        /// </summary>
        [Fact]
        public Task EventHandlerForBaseType_Invoked_IfAppliedAttribute()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost().WithDerivedDomainEventHandler())
                    .Act.OnServicesAsync(async s =>
                    {
                        var mockEvt = new MockDerivedDomainEvent();
                        await s.GetRequiredService<IMessagingService>()
                            .PublishAsync(mockEvt);
                    });

                testResult.Assert.Service<IMockTestLog>(log =>
                {
                    log.Messages.Should().ContainSingle("OnIncludeBaseEventHandler");
                });
            });
        }
    }
}

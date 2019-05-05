using CoreTests.Messaging.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Messaging;
using NetFusion.Test.Container;
using System.Threading.Tasks;
using Xunit;

namespace CoreTests.Messaging
{
    /// <summary>
    /// Tests for asserting the basic publishing and handling of derived message types.
    /// </summary>
    public class DomainEventDispatchTests
    {
        /// <summary>
        /// When a domain event is published using the service, the corresponding discovered
        /// consumer event handler methods will be invoked.
        /// </summary>
        [Fact (DisplayName = "Domain Event Consumer handler invoked")]
        public Task DomainEventConsumer_HandlerInvoked()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.WithHostConsumer())
                    .Act.OnServices(async s =>
                    {
                        var mockEvt = new MockDomainEvent();
                        await s.GetService<IMessagingService>()
                            .PublishAsync(mockEvt);
                    });

                testResult.Assert.Services(s =>
                {
                    var consumer = s.GetService<MockDomainEventConsumer>();
                    consumer.ExecutedHandlers.Should().Contain("OnEventHandlerOne");
                });
            });
        }

        /// <summary>
        /// A consumer event handler for a base domain event type can be marked
        /// with the IncludeDerivedEvents attribute to indicate it should be 
        /// called for any derived domain events.
        /// </summary>
        [Fact(DisplayName = "Event handler for Base Type invoked if applied attribute")]
        public Task EventHandlerForBaseType_InvokedIfAppliedAttribute()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.WithHost().AddDerivedEventAndConsumer())
                    .Act.OnServices(async s =>
                    {
                        var mockEvt = new MockDerivedDomainEvent();
                        await s.GetService<IMessagingService>()
                            .PublishAsync(mockEvt);
                    });

                testResult.Assert.Services(s =>
                {
                    var consumer = s.GetService<MockBaseMessageConsumer>();
                    consumer.ExecutedHandlers.Should().ContainSingle("OnIncludeBaseEventHandler");
                });
            });
        }
        
        /// <summary>
        /// When a domain-event is published, the event handlers can be asynchronous.  This test 
        /// publishes an event that will be handled by two asynchronous handlers and one synchronous.
        /// </summary>
        [Fact(DisplayName = nameof(DomainEventAsyncHandlers_CanBeInvoked))]
        public Task DomainEventAsyncHandlers_CanBeInvoked()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.WithHostAsyncConsumer())
                    .Act.OnServices(async s =>
                    {
                        var messagingSrv = s.GetService<IMessagingService>();                        
                        var evt = new MockDomainEvent();
                        await messagingSrv.PublishAsync(evt);
                    });

                testResult.Assert.Services(s =>
                {
                    var consumer = s.GetRequiredService<MockAsyncMessageConsumer>();
                    consumer.ExecutedHandlers.Should().HaveCount(3);
                    consumer.ExecutedHandlers.Should().Contain("OnEvent1Async", "OnEvent2Async", "OnEvent3");                    
                });
            });     
        }
    }
}

using System.Threading.Tasks;
using CoreTests.Messaging.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Messaging;
using NetFusion.Messaging.Exceptions;
using NetFusion.Test.Container;
using Xunit;

namespace CoreTests.Messaging.DomainEvents
{
    /// <summary>
    /// Tests for asserting the basic publishing and handling of derived message types.
    /// </summary>
    public class DispatchTests
    {
        /// <summary>
        /// When a domain event is published using the service, the corresponding discovered
        /// consumer event handler methods will be invoked.
        /// </summary>
        [Fact]
        public Task DomainEventConsumer_HandlerInvoked()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost().WithDomainEventHandler())
                    .Act.OnServicesAsync(async s =>
                    {
                        var mockEvt = new MockDomainEvent();
                        await s.GetRequiredService<IMessagingService>()
                            .PublishAsync(mockEvt);
                    });

                testResult.Assert.Services(s =>
                {
                    var consumer = s.GetRequiredService<MockDomainEventConsumer>();
                    consumer.ExecutedHandlers.Should().Contain("OnEventHandlerOne");
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

                testResult.Assert.Services(s =>
                {
                    var consumer = s.GetRequiredService<MockDerivedMessageConsumer>();
                    consumer.ExecutedHandlers.Should().ContainSingle("OnIncludeBaseEventHandler");
                });
            });
        }
        
        /// <summary>
        /// When a domain-event is published, the event handlers can be asynchronous.  This test 
        /// publishes an event that will be handled by two asynchronous handlers and one synchronous.
        /// </summary>
        [Fact]
        public Task DomainEventAsyncHandlers_CanBeInvoked()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.WithHostAsyncConsumer())
                    .Act.OnServicesAsync(async s =>
                    {
                        var messagingSrv = s.GetRequiredService<IMessagingService>();                        
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
        
        /// <summary>
        /// Tests that a message handler handler resulting in an exception
        /// is correctly captured.
        /// </summary>
        [Fact]
        public Task ExceptionsCapture_ForPublished_Message()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost().WithMessageHandlerException())
                    .Act.RecordException().OnServicesAsync(async s =>
                    {
                        var messagingSrv = s.GetRequiredService<IMessagingService>();
                        var domainEvt = new MockDomainEvent();

                        await messagingSrv.PublishAsync(domainEvt);
                    });

                testResult.Assert.Exception<PublisherException>(ex =>
                {
                    // Assert that the inner exception is a dispatch exception:
                    ex.InnerException?.InnerException.Should().NotBeNull();
                    // ex.InnerException?.InnerException.Should().BeOfType<MessageDispatchException>();
                });
            });
        }

        /// <summary>
        /// Tests that a message with multiple handlers resulting in an exception
        /// is correctly captured.
        /// </summary>
        [Fact]
        public void ExceptionsCapture_ForMultiple_MessageHandlers()
        {
            
        }
        
        /// <summary>
        /// Tests the scenario where a parent message handler dispatches a child
        /// message resulting in an exception.
        /// </summary>
        [Fact]
        public Task ExceptionsCaptured_ForChildPublished_Message()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost().WithChildMessageHandlerException())
                    .Act.RecordException().OnServicesAsync(async s =>
                    {
                        var messagingSrv = s.GetRequiredService<IMessagingService>();
                        var domainEvt = new MockDomainEvent();

                        await messagingSrv.PublishAsync(domainEvt);
                    });

                testResult.Assert.Exception<PublisherException>(ex =>
                {
                    
                });
            });
        }
    }
}

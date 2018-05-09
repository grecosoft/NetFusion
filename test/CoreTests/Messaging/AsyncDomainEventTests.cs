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
    /// Unit tests for asynchronous handling of messages.  All messages published are assumed to be 
    /// handled asynchronously.  The message handler can be either synchronous or asynchronous and 
    /// is normalized by using a TaskCompletionSource.  This easily allows the message handler to 
    /// be changed without having to change any of the calling code.
    /// </summary>
    public class AsyncDomainEventTests
    {
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
                    .Resolver(r => r.WithHostAsyncConsumer())
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

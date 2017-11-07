using Autofac;
using CoreTests.Messaging.Mocks;
using FluentAssertions;
using NetFusion.Messaging;
using NetFusion.Test.Container;
using Xunit;

namespace BootstrapTests.Messaging
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
        public void DomainEventAsyncHandlers_CanBeInvoked()
        {

            ContainerFixture.Test(fixture => { fixture
                .Arrange
                    .Resolver(r => {
                        r.WithHostAsyncConsumer();
                    })
                    .Container(c => c.UsingDefaultServices())
                .Act.OnContainer(async c => {
                    c.Build();

                    var domainEventSrv = c.Services.Resolve<IMessagingService>();
                    var evt = new MockDomainEvent();
                    await domainEventSrv.PublishAsync(evt);
                })
                .Result.Assert.Container(c =>
                {
                    var consumer = c.Services.Resolve<MockAsyncMessageConsumer>();
                    consumer.ExecutedHandlers.Should().HaveCount(3);
                    consumer.ExecutedHandlers.Should().Contain("OnEvent1Async", "OnEvent2Async", "OnEvent3");
                });
            });       
        }
    }
}

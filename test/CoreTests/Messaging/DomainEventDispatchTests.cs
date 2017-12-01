using Autofac;
using CoreTests.Messaging.Mocks;
using FluentAssertions;
using NetFusion.Messaging;
using NetFusion.Test.Container;
using NetFusion.Testing.Logging;
using System.Collections.Generic;
using System.Linq;
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
            return ContainerFixture.TestAsync(async fixture => {

                var testResult = await fixture.Arrange
                    .Resolver(r => r.WithHostConsumer())
                    .Container(c => c.UsingDefaultServices())

                .Act.OnContainer(async c => {
                    c.Build();

                    var mockEvt = new MockDomainEvent();
                    await c.Services.Resolve<IMessagingService>()
                        .PublishAsync(mockEvt);
                });

                testResult.Assert.Container(c =>
                {
                    var consumer = c.Services.Resolve<IEnumerable<IMessageConsumer>>()
                        .OfType<MockDomainEventConsumer>()
                        .First();

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
            return ContainerFixture.TestAsync(async fixture => {

                var testResult = await fixture
                .Arrange.Resolver(r => {
                        r.WithHost();
                        r.AddDerivedEventAndConsumer();
                    })
                    .Container(c => c.UsingDefaultServices())
                
                .Act.OnContainer(async c => {
                    c.Build();

                    var mockEvt = new MockDerivedDomainEvent();
                    await c.Services.Resolve<IMessagingService>()
                        .PublishAsync(mockEvt);
                });

                testResult.Assert.Container(c =>
                {
                    var consumer = c.Services.Resolve<IEnumerable<IMessageConsumer>>()
                        .OfType<MockBaseMessageConsumer>()
                        .First();

                    consumer.ExecutedHandlers.Should().ContainSingle("OnIncludeBaseEventHandler");
                });
            });    
        }

        [Fact(DisplayName = "Publisher exception Generic Exception Raised details Logged")]
        public Task PublisherException_GenericExceptionRaised_DetailsLogged()
        {
            return ContainerFixture.TestAsync(async fixture => {

                var testResult = await fixture.Arrange
                    .Resolver(r => {
                        r.WithHost();
                        r.AddEventAndExceptionConsumer();
                    })
                    .Container(c => {
                        c.UseTestLogger();
                        c.UsingDefaultServices(); })

                .Act.OnContainer(async c => {
                    c.Build();

                    var mockEvt = new MockDomainEvent();
                    await c.Services.Resolve<IMessagingService>()
                        .PublishAsync(mockEvt);
                });

                testResult.Assert.Exception<PublisherException>(ex =>
                {
                    ex.Message.Should().Contain("Exception publishing message.  See log for details.");
                })
                .Container(c => {
                    var logger = c.GetTestLogger();
                });
            });    
        }
     }
}

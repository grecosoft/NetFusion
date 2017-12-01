using Autofac;
using CoreTests.Messaging.Mocks;
using FluentAssertions;
using NetFusion.Messaging;
using NetFusion.Test.Container;
using NetFusion.Testing.Logging;
using System.Collections.Generic;
using System.Linq;
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
        public void DomainEventConsumer_HandlerInvoked()
        {
            ContainerFixture.Test(fixture => { fixture
                .Arrange
                    .Resolver(r => {
                        r.WithHostConsumer();
                    })
                    .Container(c => c.UsingDefaultServices())
                .Act.OnContainer(async c => {
                    c.Build();

                    var mockEvt = new MockDomainEvent();
                    await c.Services.Resolve<IMessagingService>()
                        .PublishAsync(mockEvt);
                })
                .Result.Assert.Container(c =>
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
        public void EventHandlerForBaseType_InvokedIfAppliedAttribute()
        {
            ContainerFixture.Test(fixture => { fixture
                .Arrange
                    .Resolver(r => {
                        r.WithHost();
                        r.AddDerivedEventAndConsumer();
                    })
                    .Container(c => c.UsingDefaultServices())
                .Act.OnContainer(async c => {
                    c.Build();

                    var mockEvt = new MockDerivedDomainEvent();
                    await c.Services.Resolve<IMessagingService>()
                        .PublishAsync(mockEvt);
                })
                .Result.Assert.Container(c =>
                {
                    var consumer = c.Services.Resolve<IEnumerable<IMessageConsumer>>()
                        .OfType<MockBaseMessageConsumer>()
                        .First();

                    consumer.ExecutedHandlers.Should().ContainSingle("OnIncludeBaseEventHandler");
                });
            });    
        }

        [Fact(DisplayName = "Publisher exception Generic Exception Raised details Logged")]
        public void PublisherException_GenericExceptionRaised_DetailsLogged()
        {
            ContainerFixture.Test(fixture => { fixture
                .Arrange
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
                })
                .Result.Assert.Exception<PublisherException>(ex =>
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

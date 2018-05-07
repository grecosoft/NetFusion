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
            return ContainerFixture.TestAsync((System.Func<ContainerFixture, Task>)(async fixture =>
            {
                var testResult = await fixture.Arrange
                        .Resolver((System.Action<NetFusion.Test.Plugins.TestTypeResolver>)(r => r.WithHostConsumer()))
                    .Act.OnServices(async s =>
                    {
                        var mockEvt = new MockDomainEvent();
                        await s.GetService<IMessagingService>()
                            .PublishAsync(mockEvt);
                    });

                testResult.Assert.Services2((System.IServiceProvider s) =>
                {
                    var consumer = s.GetService<MockDomainEventConsumer>();
                    consumer.ExecutedHandlers.Should().Contain("OnEventHandlerOne");
                });
            }));
        }

        /// <summary>
        /// A consumer event handler for a base domain event type can be marked
        /// with the IncludeDerivedEvents attribute to indicate it should be 
        /// called for any derived domain events.
        /// </summary>
        [Fact(DisplayName = "Event handler for Base Type invoked if applied attribute")]
        public Task EventHandlerForBaseType_InvokedIfAppliedAttribute()
        {
            return ContainerFixture.TestAsync((System.Func<ContainerFixture, Task>)(async fixture =>
            {
                var testResult = await fixture.Arrange
                        .Resolver((System.Action<NetFusion.Test.Plugins.TestTypeResolver>)(r => r.WithHost().AddDerivedEventAndConsumer()))
                    .Act.OnServices(async s =>
                    {
                        var mockEvt = new MockDerivedDomainEvent();
                        await s.GetService<IMessagingService>()
                           .PublishAsync(mockEvt);
                    });

                testResult.Assert.Services2((System.IServiceProvider s) =>
                {
                    var consumer = s.GetService<MockBaseMessageConsumer>();
                    consumer.ExecutedHandlers.Should().ContainSingle("OnIncludeBaseEventHandler");
                });
            }));
        }
    }
}

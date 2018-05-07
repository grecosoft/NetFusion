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
    /// An in-process message handler method can be decorated with the ApplyScriptPredicate attribute.  
    /// This attribute is used to specify a script name associated with the derived message type and 
    /// the name of a property calculated by the script returning a boolean value  indicating if the
    /// message handler applies to the published message.
    /// </summary>
    public class DispatchEvaluationTests
    {
        [Fact(DisplayName = nameof(HandlerCalled_WhenMessagePassesDispathPredicate))]
        public Task HandlerCalled_WhenMessagePassesDispathPredicate()
        {
            return ContainerFixture.TestAsync((System.Func<ContainerFixture, Task>)(async fixture =>
            {
                var testResult = await fixture.Arrange
                        .Resolver((System.Action<NetFusion.Test.Plugins.TestTypeResolver>)(r => r.WithHostEvalBasedConsumer()))
                        .Services(s => s.UseMockedEvalService())
                    .Act.OnServices(s =>
                    {
                        var mockEvt = new MockEvalDomainEvent { RuleTestValue = 7000 };
                        return s.GetRequiredService<IMessagingService>()
                                         .PublishAsync(mockEvt);
                    });

                testResult.Assert.Services2((System.IServiceProvider s) =>
                {
                    var consumer = s.GetRequiredService<MockDomainEventEvalBasedConsumer>();
                    consumer.ExecutedHandlers.Should().Contain("OnEventPredicatePases");
                });
            }));
        }
    }
}


using Autofac;
using CoreTests.Messaging.Mocks;
using FluentAssertions;
using NetFusion.Messaging;
using NetFusion.Test.Container;
using System.Threading.Tasks;
using Xunit;

namespace BootstrapTests.Messaging
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
            return ContainerFixture.TestAsync(async fixture => {

                var testResult = await fixture.Arrange
                    .Resolver(r => r.WithHostEvalBasedConsumer())
                    .Container(c => {
                        c.UseMockedEvalService();
                    })
                .Act.OnContainer(c => {
                    c.Build();

                    var mockEvt = new MockEvalDomainEvent { RuleTestValue = 7000 };
                    return c.Services.Resolve<IMessagingService>()
                        .PublishAsync(mockEvt);
                });

                testResult.Assert.Container(c =>
                {
                    var consumer = c.Services.Resolve<MockDomainEventEvalBasedConsumer>();
                    consumer.ExecutedHandlers.Should().Contain("OnEventPredicatePases");
                });
            });          
        }
    }
}
    

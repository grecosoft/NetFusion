using Autofac;
using CoreTests.Messaging.Mocks;
using FluentAssertions;
using NetFusion.Messaging;
using NetFusion.Test.Container;
using System.Threading.Tasks;
using Xunit;

namespace CoreTests.Messaging
{
    /// <summary>
    /// One or more rules can be associated with a message handler.  These rules are static
    /// and are evaluated based on the state of the message being published.  If the rule
    /// evaluates to true, the message handler is invoked.
    /// </summary>
    public class DispatchRuleTests
    {
        [Fact(DisplayName = nameof(HandlerCalled_WhenMessagePassesAllDispatchRules))]
        public Task HandlerCalled_WhenMessagePassesAllDispatchRules()
        {
            return ContainerFixture.TestAsync(async fixture => {

                var testResult = await fixture
                .Arrange
                    .Resolver(r => r.WithHostRuleBasedConsumer())
                    .Container(c => c.UsingDefaultServices())

                .Act.OnContainer(c => {
                    c.Build();

                    var mockEvt = new MockRuleDomainEvent { RuleTestValue = 1500 };
                    return c.Services.Resolve<IMessagingService>()
                        .PublishAsync(mockEvt);
                });

                testResult.Assert.Container(c =>
                {
                    var consumer = c.Services.Resolve<MockDomainEventRuleBasedConsumer>();
                    consumer.ExecutedHandlers.Should().Contain("OnEventAllRulesPass");
                });
            });               
        }

        [Fact(DisplayName = nameof(HandlerCalled_WhenMessagePassesAnyDispatchRule))]
        public Task HandlerCalled_WhenMessagePassesAnyDispatchRule()
        {
            return ContainerFixture.TestAsync(async fixture => {

                var testResult = await fixture.Arrange
                    .Resolver(r => r.WithHostRuleBasedConsumer())
                    .Container(c => c.UsingDefaultServices())

                .Act.OnContainer(c => {
                    c.Build();

                    var mockEvt = new MockRuleDomainEvent { RuleTestValue = 3000 };
                    return c.Services.Resolve<IMessagingService>()
                        .PublishAsync(mockEvt);
                });

                testResult.Assert.Container(c =>
                {
                    var consumer = c.Services.Resolve<MockDomainEventRuleBasedConsumer>();
                    consumer.ExecutedHandlers.Should().Contain("OnEventAnyRulePasses");
                });
            });               
        }
    }
}

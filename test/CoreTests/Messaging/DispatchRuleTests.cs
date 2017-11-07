using Autofac;
using CoreTests.Messaging.Mocks;
using FluentAssertions;
using NetFusion.Messaging;
using NetFusion.Test.Container;
using Xunit;

namespace BootstrapTests.Messaging
{
    /// <summary>
    /// One or more rules can be associated with a message handler.  These rules are static
    /// and are evaluated based on the state of the message being published.  If the rule
    /// evaluates to true, the message handler is invoked.
    /// </summary>
    public class DispatchRuleTests
    {
        [Fact(DisplayName = nameof(HandlerCalled_WhenMessagePassesAllDispatchRules))]
        public void HandlerCalled_WhenMessagePassesAllDispatchRules()
        {
            ContainerFixture.Test(fixture => { fixture
                .Arrange
                    .Resolver(r => {
                        r.WithHostRuleBasedConsumer();
                    })
                    .Container(c => c.UsingDefaultServices())
                .Act.OnContainer(async c => {
                    c.Build();

                    var mockEvt = new MockRuleDomainEvent { RuleTestValue = 1500 };
                    await c.Services.Resolve<IMessagingService>()
                        .PublishAsync(mockEvt);
                })
                .Result.Assert.Container(c =>
                {
                    var consumer = c.Services.Resolve<MockDomainEventRuleBasedConsumer>();
                    consumer.ExecutedHandlers.Should().Contain("OnEventAllRulesPass");
                });
            });               
        }

        [Fact(DisplayName = nameof(HandlerCalled_WhenMessagePassesAnyDispatchRule))]
        public void HandlerCalled_WhenMessagePassesAnyDispatchRule()
        {
            ContainerFixture.Test(fixture => { fixture
                .Arrange
                    .Resolver(r => {
                        r.WithHostRuleBasedConsumer();
                    })
                    .Container(c => c.UsingDefaultServices())
                .Act.OnContainer(async c => {
                    c.Build();

                    var mockEvt = new MockRuleDomainEvent { RuleTestValue = 3000 };
                    await c.Services.Resolve<IMessagingService>()
                        .PublishAsync(mockEvt);
                })
                .Result.Assert.Container(c =>
                {
                    var consumer = c.Services.Resolve<MockDomainEventRuleBasedConsumer>();
                    consumer.ExecutedHandlers.Should().Contain("OnEventAnyRulePasses");
                });
            });               
        }
    }
}

using System.Threading.Tasks;
using CoreTests.Messaging.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Messaging;
using NetFusion.Test.Container;
using Xunit;

namespace CoreTests.Messaging.DomainEvents
{
    public class DispatchRuleTests
    {
        [Fact(DisplayName = nameof(HandlerCalled_WhenMessagePassesAllDispatchRules))]
        public Task HandlerCalled_WhenMessagePassesAllDispatchRules()
        { 
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost().WithDomainEventRuleHandler())
                    .Act.OnServicesAsync(s =>
                    {
                        var mockEvt = new MockRuleDomainEvent { RuleTestValue = 1500 };
                        return s.GetRequiredService<IMessagingService>()
                            .PublishAsync(mockEvt);
                    });

                testResult.Assert.Services(s =>
                {
                    var consumer = s.GetRequiredService<MockDomainEventRuleBasedConsumer>();
                    consumer.ExecutedHandlers.Should().Contain("OnEventAllRulesPass");
                });
            });
        }
        
        [Fact(DisplayName = nameof(HandlerCalled_WhenMessagePassesAnyDispatchRule))]
        public Task HandlerCalled_WhenMessagePassesAnyDispatchRule()
        {
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.AddMessagingHost().WithDomainEventRuleHandler())
                    .Act.OnServicesAsync(s =>
                    {
                        var mockEvt = new MockRuleDomainEvent { RuleTestValue = 3000 };
                        return s.GetRequiredService<IMessagingService>()
                            .PublishAsync(mockEvt);
                    });

                testResult.Assert.Services(s =>
                {
                    var consumer = s.GetRequiredService<MockDomainEventRuleBasedConsumer>();
                    consumer.ExecutedHandlers.Should().Contain("OnEventAnyRulePasses");
                });
            });
        }
    }
}
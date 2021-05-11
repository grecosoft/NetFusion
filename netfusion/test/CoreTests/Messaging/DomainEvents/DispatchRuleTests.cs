using System.Threading.Tasks;
using CoreTests.Messaging.DomainEvents.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Messaging;
using NetFusion.Test.Container;
using Xunit;

namespace CoreTests.Messaging.DomainEvents
{
    public class DispatchRuleTests
    {
        [Fact]
        public Task HandlerCalled_WhenMessagePasses_AllDispatchRules()
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

                testResult.Assert.Service<IMockTestLog>(log =>
                {
                    log.Entries.Should().Contain("OnEventAllRulesPass");
                });
            });
        }
        
        [Fact]
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

                testResult.Assert.Service<IMockTestLog>(log =>
                {
                    log.Entries.Should().Contain("OnEventAnyRulePasses");
                });
            });
        }
    }
}
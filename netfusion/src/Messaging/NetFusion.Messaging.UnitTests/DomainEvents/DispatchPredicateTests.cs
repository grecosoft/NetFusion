using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Messaging.UnitTests.DomainEvents.Mocks;

namespace NetFusion.Messaging.UnitTests.DomainEvents;

public class DispatchPredicateTests
{
    [Fact]
    public Task HandlerCalled_WhenMessagePasses_AllDispatchRules()
    { 
        return ContainerFixture.TestAsync(async fixture =>
        {
            var testResult = await fixture.Arrange
                .Container(c => c.AddMessagingHost().WithDomainEventPredicateHandler())
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
                .Container(c => c.AddMessagingHost().WithDomainEventPredicateHandler())
                .Act.OnServicesAsync(s =>
                {
                    var mockEvt = new MockRuleDomainEvent { RuleTestValue = 1500 };
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
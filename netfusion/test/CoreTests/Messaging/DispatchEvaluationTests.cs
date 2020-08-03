using CoreTests.Messaging.Mocks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Messaging;
using NetFusion.Test.Container;
using System.Threading.Tasks;
using Xunit;

namespace CoreTests.Messaging
{
    
    public class DispatchEvaluationTests
    {
//        /// <summary>
//        /// An in-process message handler method can be decorated with the ApplyScriptPredicate attribute.  
//        /// This attribute is used to specify a script name, associated with the derived message type, and 
//        /// the name of a property calculated by the script returning a boolean value.  The returned value
//        /// indicates if the message handler applies to the published message.
//        /// </summary>
//        [Fact(DisplayName = nameof(HandlerCalled_WhenMessagePassesDispatchPredicate))]
//        public Task HandlerCalled_WhenMessagePassesDispatchPredicate()
//        {
//            return ContainerFixture.TestAsync(async fixture =>
//            {
//                var testResult = await fixture.Arrange
//                    .Container(c => c.WithHostEvalBasedConsumer())
//                    .Services(s => s.UseMockedEvalService())
//                    .Act.OnServices(s =>
//                    {
//                        var mockEvt = new MockEvalDomainEvent { RuleTestValue = 7000 };
//                        return s.GetRequiredService<IMessagingService>()
//                            .PublishAsync(mockEvt);
//                    });
//
//                testResult.Assert.Services(s =>
//                {
//                    var consumer = s.GetRequiredService<MockDomainEventEvalBasedConsumer>();
//                    consumer.ExecutedHandlers.Should().Contain("OnEventPredicatePassed");
//                }); 
//            });
//        }
        
        [Fact(DisplayName = nameof(HandlerCalled_WhenMessagePassesAllDispatchRules))]
        public Task HandlerCalled_WhenMessagePassesAllDispatchRules()
        { 
            return ContainerFixture.TestAsync(async fixture =>
            {
                var testResult = await fixture.Arrange
                    .Container(c => c.WithHostRuleBasedConsumer())
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
                    .Container(c => c.WithHostRuleBasedConsumer())
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


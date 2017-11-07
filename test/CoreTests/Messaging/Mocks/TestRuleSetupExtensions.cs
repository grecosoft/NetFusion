using NetFusion.Messaging;
using NetFusion.Messaging.Rules;
using NetFusion.Messaging.Types;
using NetFusion.Messaging.Types.Rules;
using NetFusion.Test.Plugins;

namespace CoreTests.Messaging.Mocks
{
    public static class TestRuleSetupExtensions
    {
        public static TestTypeResolver WithHostRuleBasedConsumer(this TestTypeResolver resolver)
        {
            resolver.AddPlugin<MockAppHostPlugin>()
                .AddPluginType<MockRuleDomainEvent>()
                .AddPluginType<MockDomainEventRuleBasedConsumer>()
                .AddPluginType<MockRoleMin>()
                .AddPluginType<MockRoleMax>();

            resolver.AddPlugin<MockCorePlugin>()
                .UseMessagingPlugin();

            return resolver;
        }
    }

    //-------------------------- MOCKED TYPED --------------------------------------
    public class MockRuleDomainEvent : DomainEvent
    {
        public int RuleTestValue { get; set; } = -1;
    }

    public class MockDomainEventRuleBasedConsumer : MockConsumer,
        IMessageConsumer
    {
        [InProcessHandler, ApplyDispatchRule(typeof(MockRoleMin), typeof(MockRoleMax),
            RuleApplyType = RuleApplyTypes.All)]
        public void OnEventAllRulesPass(MockRuleDomainEvent evt)
        {
            AddCalledHandler(nameof(OnEventAllRulesPass));
        }

        [InProcessHandler, ApplyDispatchRule(typeof(MockRoleMin), typeof(MockRoleMax),
            RuleApplyType = RuleApplyTypes.Any)]
        public void OnEventAnyRulePasses(MockRuleDomainEvent evt)
        {
            AddCalledHandler(nameof(OnEventAnyRulePasses));
        }
    }

    public class MockRoleMin : MessageDispatchRule<MockRuleDomainEvent>
    {
        protected override bool IsMatch(MockRuleDomainEvent message)
        {
            if (message.RuleTestValue == -1) return false;
            return 1000 <= message.RuleTestValue;
        }
    }

    public class MockRoleMax : MessageDispatchRule<MockRuleDomainEvent>
    {
        protected override bool IsMatch(MockRuleDomainEvent message)
        {
            if (message.RuleTestValue == -1) return false;
            return message.RuleTestValue <= 2000;
        }
    }
}

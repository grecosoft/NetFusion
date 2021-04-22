using NetFusion.Messaging.Types;

namespace CoreTests.Messaging.Mocks
{
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
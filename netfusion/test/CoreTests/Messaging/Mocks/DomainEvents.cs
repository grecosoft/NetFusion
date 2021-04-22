using NetFusion.Messaging.Types;

namespace CoreTests.Messaging.Mocks
{
    // --------------------- [Domain Events] ---------------------
    
    public class MockDomainEvent : DomainEvent
    {

    }

    public class MockDomainEventTwo : DomainEvent
    {
        
    }
    
    // ------------------- [Derived Domain Events] ------------------
    
    public class MockBaseDomainEvent : DomainEvent
    {

    }

    public class MockDerivedDomainEvent : MockBaseDomainEvent
    {
    }
    
    public class MockRuleDomainEvent : DomainEvent
    {
        public int RuleTestValue { get; set; } = -1;
    }
}

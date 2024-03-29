﻿using NetFusion.Messaging.Types;

namespace NetFusion.Messaging.UnitTests.DomainEvents.Mocks;

// --------------------- [Domain Events] ---------------------

public class MockDomainEvent : DomainEvent
{
    public List<string> ThrowInHandlers { get; } = new();
}

public class MockDomainEventTwo : DomainEvent;
    
// ------------------- [Derived Domain Events] ------------------
    
public class MockBaseDomainEvent : DomainEvent;

public class MockDerivedDomainEvent : MockBaseDomainEvent;
    
public class MockRuleDomainEvent : DomainEvent
{
    public int RuleTestValue { get; set; } = -1;
}
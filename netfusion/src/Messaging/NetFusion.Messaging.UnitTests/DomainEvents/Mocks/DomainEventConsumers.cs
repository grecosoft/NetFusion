namespace NetFusion.Messaging.UnitTests.DomainEvents.Mocks;
// ------------------- [Message Consumers] ------------------

public class MockSyncDomainEventConsumerOne : MockConsumer
{
    public MockSyncDomainEventConsumerOne(IMockTestLog testLog) : base(testLog) { }
        
    public void OnEventHandler(MockDomainEvent domainEvent)
    {
        if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));

        TestLog
            .AddLogEntry("Sync-DomainEvent-Handler-1")
            .RecordMessage(domainEvent);

        if (domainEvent.ThrowInHandlers.Contains(nameof(MockSyncDomainEventConsumerOne)))
        {
            throw new InvalidOperationException($"{nameof(MockSyncDomainEventConsumerOne)}_Exception");
        }
    }
}
    
public class MockAsyncDomainEventConsumerOne : MockConsumer
{
    public MockAsyncDomainEventConsumerOne(IMockTestLog testLog) : base(testLog) { }
        
    public async Task OnEventHandler(MockDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        TestLog
            .AddLogEntry("Async-DomainEvent-Handler-1")
            .RecordMessage(domainEvent);
            
        await Task.Delay(TimeSpan.FromSeconds(1));
            
        if (domainEvent.ThrowInHandlers.Contains(nameof(MockAsyncDomainEventConsumerOne)))
        {
            throw new InvalidOperationException($"{nameof(MockAsyncDomainEventConsumerOne)}_Exception");
        }
    }
}
    
public class MockSyncDomainEventConsumerTwo : MockConsumer
{
    public MockSyncDomainEventConsumerTwo(IMockTestLog testLog) : base(testLog) { }
        
    public void OnEventHandler(MockDomainEvent domainEvent)
    {
        if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));

        TestLog
            .AddLogEntry("Sync-DomainEvent-Handler-2")
            .RecordMessage(domainEvent);
            
        if (domainEvent.ThrowInHandlers.Contains(nameof(MockSyncDomainEventConsumerTwo)))
        {
            throw new InvalidOperationException($"{nameof(MockSyncDomainEventConsumerTwo)}_Exception");
        }
    }
}
    
public class MockAsyncDomainEventConsumerTwo : MockConsumer
{
    public MockAsyncDomainEventConsumerTwo(IMockTestLog testLog) : base(testLog) { }
        
    public Task OnEventHandler(MockDomainEvent domainEvent)
    {
        TestLog
            .AddLogEntry("Async-DomainEvent-Handler-2")
            .RecordMessage(domainEvent);
            
        if (domainEvent.ThrowInHandlers.Contains(nameof(MockAsyncDomainEventConsumerTwo)))
        {
            throw new InvalidOperationException($"{nameof(MockAsyncDomainEventConsumerTwo)}_Exception");
        }

        return Task.Delay(TimeSpan.FromSeconds(1));
    }
}
    
public class MockDerivedMessageConsumer : MockConsumer
{
    public MockDerivedMessageConsumer(IMockTestLog testLog) : base(testLog) { }
        
    public void OnBaseEventHandler(MockBaseDomainEvent domainEvent)
    {
        if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));

        TestLog
            .AddLogEntry("OnBaseEventHandler")
            .RecordMessage(domainEvent);
    }

    public void OnIncludeBaseEventHandler(MockBaseDomainEvent domainEvent)
    {
        if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));

        TestLog
            .AddLogEntry("OnIncludeBaseEventHandler")
            .RecordMessage(domainEvent);
    }
}
    
    
// ------------------- [Message Consumers with Rules] ------------------
    
public class MockDomainEvenConsumerWithRule : MockConsumer
{
    public MockDomainEvenConsumerWithRule(IMockTestLog testLog) : base(testLog) { }
        
    public void OnEventAllRulesPass(MockRuleDomainEvent domainEvent)
    {
        TestLog
            .AddLogEntry("OnEventAllRulesPass")
            .RecordMessage(domainEvent);
    }
        
    public void OnEventAnyRulePasses(MockRuleDomainEvent domainEvent)
    {
        TestLog
            .AddLogEntry("OnEventAnyRulePasses")
            .RecordMessage(domainEvent);
    }
}
    
    
// ------------------- [Message Consumer Exceptions] ------------------
    
public class MockErrorMessageConsumer
{
    public Task OnEventAsync(MockDomainEvent domainEvent)
    {
        return Task.Run(() => throw new InvalidOperationException(nameof(OnEventAsync)));
    }
}
    
public class MockErrorParentMessageConsumer : MockConsumer
{
    private readonly IMessagingService _messaging;
        
    public MockErrorParentMessageConsumer(IMessagingService messaging)
    {
        _messaging = messaging;
    }
        
    public async Task OnDomainEventAsync(MockDomainEvent domainEvent)
    {
        await _messaging.PublishAsync(new MockDomainEventTwo());
    }
}

public class MockErrorChildMessageConsumer : MockConsumer
{
    public Task OnDomainEventTwoAsync(MockDomainEventTwo domainEvent)
    {
        return Task.Run(
            () => throw new InvalidOperationException($"{nameof(MockErrorChildMessageConsumer)}_Exception"));
    }
}
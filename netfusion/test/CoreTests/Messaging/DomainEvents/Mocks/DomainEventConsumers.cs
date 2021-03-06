using System;
using System.Threading.Tasks;
using NetFusion.Messaging;
using NetFusion.Messaging.Types;

namespace CoreTests.Messaging.DomainEvents.Mocks
{
    // ------------------- [Message Consumers] ------------------
    
    public class MockSyncDomainEventConsumerOne : MockConsumer,
        IMessageConsumer
    {
        public MockSyncDomainEventConsumerOne(IMockTestLog testLog) : base(testLog) { }
        
        [InProcessHandler]
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
    
    public class MockAsyncDomainEventConsumerOne : MockConsumer,
        IMessageConsumer
    {
        public MockAsyncDomainEventConsumerOne(IMockTestLog testLog) : base(testLog) { }
        
        [InProcessHandler]
        public async Task OnEventHandler(MockDomainEvent domainEvent)
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
    
    public class MockSyncDomainEventConsumerTwo : MockConsumer,
        IMessageConsumer
    {
        public MockSyncDomainEventConsumerTwo(IMockTestLog testLog) : base(testLog) { }
        
        [InProcessHandler]
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
    
    public class MockAsyncDomainEventConsumerTwo : MockConsumer,
        IMessageConsumer
    {
        public MockAsyncDomainEventConsumerTwo(IMockTestLog testLog) : base(testLog) { }
        
        [InProcessHandler]
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
    
    public class MockDerivedMessageConsumer : MockConsumer,
        IMessageConsumer
    {
        public MockDerivedMessageConsumer(IMockTestLog testLog) : base(testLog) { }
        
        [InProcessHandler]
        public void OnBaseEventHandler(MockBaseDomainEvent domainEvent)
        {
            if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));

            TestLog
                .AddLogEntry("OnBaseEventHandler")
                .RecordMessage(domainEvent);
        }

        [InProcessHandler]
        public void OnIncludeBaseEventHandler([IncludeDerivedMessages]MockBaseDomainEvent domainEvent)
        {
            if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));

            TestLog
                .AddLogEntry("OnIncludeBaseEventHandler")
                .RecordMessage(domainEvent);
        }
    }
    
    
    // ------------------- [Message Consumers with Rules] ------------------
    
    public class MockDomainEvenConsumerWithRule : MockConsumer,
        IMessageConsumer
    {
        public MockDomainEvenConsumerWithRule(IMockTestLog testLog) : base(testLog) { }
        
        [InProcessHandler, ApplyDispatchRule(typeof(MockRoleMin), typeof(MockRoleMax),
             RuleApplyType = RuleApplyTypes.All)]
        public void OnEventAllRulesPass(MockRuleDomainEvent domainEvent)
        {
            TestLog
                .AddLogEntry("OnEventAllRulesPass")
                .RecordMessage(domainEvent);
        }

        [InProcessHandler, ApplyDispatchRule(typeof(MockRoleMin), typeof(MockRoleMax),
             RuleApplyType = RuleApplyTypes.Any)]
        public void OnEventAnyRulePasses(MockRuleDomainEvent domainEvent)
        {
            TestLog
                .AddLogEntry("OnEventAnyRulePasses")
                .RecordMessage(domainEvent);
        }
    }
    
    
    // ------------------- [Message Consumer Exceptions] ------------------
    
    public class MockErrorMessageConsumer : IMessageConsumer
    {
        [InProcessHandler]
        public Task OnEventAsync(MockDomainEvent domainEvent)
        {
            return Task.Run(() => throw new InvalidOperationException(nameof(OnEventAsync)));
        }
    }
    
    public class MockErrorParentMessageConsumer : MockConsumer,
        IMessageConsumer
    {
        private readonly IMessagingService _messaging;
        
        public MockErrorParentMessageConsumer(IMessagingService messaging)
        {
            _messaging = messaging;
        }
        
        [InProcessHandler]
        public async Task OnDomainEventAsync(MockDomainEvent domainEvent)
        {
            await _messaging.PublishAsync(new MockDomainEventTwo());
        }
    }

    public class MockErrorChildMessageConsumer : MockConsumer,
        IMessageConsumer
    {
        [InProcessHandler]
        public Task OnDomainEventTwoAsync(MockDomainEventTwo domainEvent)
        {
            return Task.Run(
                () => throw new InvalidOperationException($"{nameof(MockErrorChildMessageConsumer)}_Exception"));
        }
    }
}
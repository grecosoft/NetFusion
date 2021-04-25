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

            AddCalledHandler("Sync-DomainEvent-Handler-1");
            RecordReceivedMessage(domainEvent);
        }
    }
    
    public class MockAsyncDomainEventConsumerOne : MockConsumer,
        IMessageConsumer
    {
        public MockAsyncDomainEventConsumerOne(IMockTestLog testLog) : base(testLog) { }
        
        [InProcessHandler]
        public Task OnEventHandler(MockDomainEvent domainEvent)
        {
            AddCalledHandler("Async-DomainEvent-Handler-1");
            return Task.Delay(TimeSpan.FromSeconds(1));
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

            AddCalledHandler("Sync-DomainEvent-Handler-2");
            RecordReceivedMessage(domainEvent);
        }
    }
    
    public class MockAsyncDomainEventConsumerTwo : MockConsumer,
        IMessageConsumer
    {
        public MockAsyncDomainEventConsumerTwo(IMockTestLog testLog) : base(testLog) { }
        
        [InProcessHandler]
        public Task OnEventHandler(MockDomainEvent domainEvent)
        {
            AddCalledHandler("Async-DomainEvent-Handler-2");
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

            AddCalledHandler("OnBaseEventHandler");
            RecordReceivedMessage(domainEvent);
        }

        [InProcessHandler]
        public void OnIncludeBaseEventHandler([IncludeDerivedMessages]MockBaseDomainEvent domainEvent)
        {
            if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));

            AddCalledHandler("OnIncludeBaseEventHandler");
            RecordReceivedMessage(domainEvent);
        }
    }
    
    
    // ------------------- [Message Consumers with Rules] ------------------
    
    public class MockDomainEvenConsumerWithRule : MockConsumer,
        IMessageConsumer
    {
        public MockDomainEvenConsumerWithRule(IMockTestLog testLog) : base(testLog) { }
        
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
            return Task.Run(() => throw new InvalidOperationException(nameof(OnDomainEventTwoAsync)));
        }
    }
}
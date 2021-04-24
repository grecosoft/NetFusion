using System;
using System.Threading.Tasks;
using NetFusion.Messaging;
using NetFusion.Messaging.Types;

namespace CoreTests.Messaging.DomainEvents.Mocks
{
    // ------------------- [Basic Message Consumers] ------------------
    
    public class MockDomainEventConsumer : MockConsumer,
        IMessageConsumer
    {
        [InProcessHandler]
        public void OnEventHandlerOne(MockDomainEvent domainEvent)
        {
            if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));

            AddCalledHandler("OnEventHandlerOne");
            RecordReceivedMessage(domainEvent);
        }
    }
    
    public class MockDerivedMessageConsumer : MockConsumer,
        IMessageConsumer
    {
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
    
    // ------------------- [Asynchronous Message Consumers] ------------------
    
    public class MockAsyncMessageConsumer : MockConsumer,
        IMessageConsumer
    {
        [InProcessHandler]
        public Task OnEvent1Async(MockDomainEvent domainEvent)
        {
            AddCalledHandler(nameof(OnEvent1Async));
            return Task.Delay(TimeSpan.FromSeconds(1));
        }

        [InProcessHandler]
        public Task OnEvent2Async(MockDomainEvent domainEvent)
        {
            AddCalledHandler(nameof(OnEvent2Async));
            return Task.Delay(TimeSpan.FromSeconds(1));
        }

        [InProcessHandler]
        public void OnEvent3(MockDomainEvent domainEvent)
        {
            AddCalledHandler(nameof(OnEvent3));
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
}
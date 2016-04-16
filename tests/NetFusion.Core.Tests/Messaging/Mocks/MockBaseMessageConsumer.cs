using NetFusion.Messaging;

namespace NetFusion.Core.Tests.Messaging.Mocks
{
    public class MockBaseMessageConsumer : MockConsumer,
        IMessageConsumer
    {
      
        public void OnBaseEventHandler(MockBaseDomainEvent domainEvent)
        {
            AddCalledHandler("OnBaseEventHandler");
        }

        public void OnIncludeBaseEventHandler([IncludeDerivedMessages]MockBaseDomainEvent domainEvent)
        {
            AddCalledHandler("OnIncludeBaseEventHandler");
        }
    }
}

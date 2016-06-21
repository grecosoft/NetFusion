using NetFusion.Messaging;

namespace NetFusion.Core.Tests.Messaging.Mocks
{
    public class MockBaseMessageConsumer : MockConsumer,
        IMessageConsumer
    {
      
        [InProcessHandler]
        public void OnBaseEventHandler(MockBaseDomainEvent domainEvent)
        {
            AddCalledHandler("OnBaseEventHandler");
        }

        [InProcessHandler]
        public void OnIncludeBaseEventHandler([IncludeDerivedMessages]MockBaseDomainEvent domainEvent)
        {
            AddCalledHandler("OnIncludeBaseEventHandler");
        }
    }
}

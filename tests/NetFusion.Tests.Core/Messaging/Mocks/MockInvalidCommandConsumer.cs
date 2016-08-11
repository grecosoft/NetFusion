using NetFusion.Messaging;

namespace NetFusion.Core.Tests.Messaging.Mocks
{
    public class MockInvalidCommandConsumer : MockConsumer,
        IMessageConsumer
    {
        [InProcessHandler]
        public void InvalidHandler1(MockInvalidCommand evt)
        {

        }

        [InProcessHandler]
        public void InvalidHandler2(MockInvalidCommand evt)
        {

        }
    }
}

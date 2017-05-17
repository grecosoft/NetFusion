using NetFusion.Messaging;
using System.Threading;
using WebApiHost.Messaging.Messages;

namespace WebApiHost.Messaging.Consumers
{
    public class ExampleSyncHandler1 : IMessageConsumer
    {
        [InProcessHandler]
        public void OnEvent(ExampleSyncDomainEvent evt)
        {
            Thread.Sleep(evt.Seconds * 1000);
        }
    }

    public class ExampleSyncHandler2 : IMessageConsumer
    {
        [InProcessHandler]
        public void OnEvent(ExampleSyncDomainEvent evt)
        {
            Thread.Sleep(evt.Seconds * 1000);
        }
    }
}

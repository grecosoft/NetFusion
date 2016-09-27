using NetFusion.Messaging;
using RefArch.Api.Messaging.Messages;
using System.Threading;

namespace RefArch.Services.Messaging
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

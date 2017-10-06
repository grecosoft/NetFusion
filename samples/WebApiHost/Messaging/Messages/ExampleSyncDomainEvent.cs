using NetFusion.Messaging.Types;
using WebApiHost.Messaging.Models;

namespace WebApiHost.Messaging.Messages
{
    public class ExampleSyncDomainEvent : DomainEvent
    {
        public int Seconds { get; set; }

        public ExampleSyncDomainEvent(MessageInfo info)
        {
            this.Seconds = info.DelayInSeconds;
        }
    }
}

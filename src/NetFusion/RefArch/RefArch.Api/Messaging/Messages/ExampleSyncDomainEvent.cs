using NetFusion.Messaging;
using RefArch.Api.Messaging.Models;

namespace RefArch.Api.Messaging.Messages
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

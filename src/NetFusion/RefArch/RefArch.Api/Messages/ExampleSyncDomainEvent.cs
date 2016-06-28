using NetFusion.Messaging;
using RefArch.Api.Models;

namespace RefArch.Api.Messages
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

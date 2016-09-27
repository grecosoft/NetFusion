using NetFusion.Messaging;
using RefArch.Api.Messaging.Models;

namespace RefArch.Api.Messaging.Messages
{
    public class ExampleAsyncDomainEvent : DomainEvent
    {
        public int Seconds { get; set; }

        public ExampleAsyncDomainEvent(MessageInfo info)
        {
            this.Seconds = info.DelayInSeconds;
        }
    }
}

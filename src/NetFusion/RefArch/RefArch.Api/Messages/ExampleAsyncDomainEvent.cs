using NetFusion.Messaging;
using RefArch.Api.Models;

namespace RefArch.Api.Messages
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

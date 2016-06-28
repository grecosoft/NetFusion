using NetFusion.Messaging;
using RefArch.Api.Models;

namespace RefArch.Api.Messages
{
    public class ExampleAsyncDomainEventException : DomainEvent
    {
        public int Seconds { get; set; }
        public bool ThrowEx { get; set; }

        public ExampleAsyncDomainEventException(MessageExInfo info)
        {
            this.Seconds = info.DelayInSeconds;
            this.ThrowEx = info.ThrowEx;
        }
    }
}

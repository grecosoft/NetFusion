using NetFusion.Messaging.Types;
using WebApiHost.Messaging.Models;

namespace WebApiHost.Messaging.Messages
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

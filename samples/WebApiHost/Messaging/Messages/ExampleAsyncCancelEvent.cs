using NetFusion.Domain.Messaging;
using WebApiHost.Messaging.Models;

namespace WebApiHost.Messaging.Messages
{
    public class ExampleAsyncCancelEvent : DomainEvent
    {
        public int Seconds { get; set; }
        public int CancelationInSeconds { get; set; }

        public ExampleAsyncCancelEvent(MessageCancelInfo info)
        {
            this.Seconds = info.DelayInSeconds;
            this.CancelationInSeconds = info.CancelationInSeconds;
        }
    }
}

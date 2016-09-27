using NetFusion.Messaging;

namespace RefArch.Api.Messaging.Models
{
    public class MessageInfo : DomainEvent
    {
        public int DelayInSeconds { get; set; }
    }
}

using NetFusion.Messaging;

namespace RefArch.Api.Models
{
    public class MessageInfo : DomainEvent
    {
        public int DelayInSeconds = 5;
    }
}

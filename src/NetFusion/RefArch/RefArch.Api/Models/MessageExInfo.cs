using NetFusion.Messaging;

namespace RefArch.Api.Models
{
    public class MessageExInfo : DomainEvent
    {
        public int DelayInSeconds = 5;
        public bool ThrowEx = true;
    }
}

using NetFusion.Messaging.Types;

namespace Demo.Client
{
    public class NotificationDomainEvent : DomainEvent
    {
        public string PartNumber { get; set; }
    }
}
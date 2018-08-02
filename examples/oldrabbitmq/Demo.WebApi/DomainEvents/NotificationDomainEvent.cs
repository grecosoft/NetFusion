using NetFusion.Messaging.Types;

namespace Demo.WebApi.DomainEvents
{
    public class NotificationDomainEvent : DomainEvent
    {
        public string PartNumber { get; }

        public NotificationDomainEvent(string partNumber)
        {
            PartNumber = partNumber;
        }
    }
}
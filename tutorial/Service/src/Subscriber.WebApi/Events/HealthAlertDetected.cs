using NetFusion.Messaging.Types;

namespace Subscriber.WebApi.Events
{
    public class HealthAlertDetected : DomainEvent
    {
        public string AlertName { get; set; }
        public string Value { get; set; }
        
    }
}
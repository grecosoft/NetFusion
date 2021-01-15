using NetFusion.Messaging.Types;

namespace Service.Domain.Events
{
    public class HealthAlertDetected : DomainEvent
    {
        public string AlertName { get; set; }
        public string Value { get; set; }
        
    }
}
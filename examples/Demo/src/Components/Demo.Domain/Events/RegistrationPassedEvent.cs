using NetFusion.Messaging.Types;

namespace Demo.Domain.Events
{
    public class RegistrationPassedEvent : DomainEvent
    {
        public string ReferenceNumber { get; }
        public string State { get; }

        public RegistrationPassedEvent(
            string referenceNumber,
            string state)
        {
            ReferenceNumber = referenceNumber;
            State = state;
        }
    }
}

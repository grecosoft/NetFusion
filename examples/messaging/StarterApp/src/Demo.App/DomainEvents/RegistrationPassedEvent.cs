using NetFusion.Messaging.Types;

namespace Demo.App.DomainEvents
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
using NetFusion.Messaging.Types;

namespace Demo.Domain.Events
{
    public class RegistrationDomainEvent : DomainEvent
    {
        public string FirstName { get; }
        public string LastName { get; }

        public RegistrationDomainEvent(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }
    }
}

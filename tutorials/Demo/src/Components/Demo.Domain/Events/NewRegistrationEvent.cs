using NetFusion.Messaging.Types;
using Demo.Domain.Entities;

namespace Demo.Domain.Events
{
    public class NewRegistrationEvent : DomainEvent
    {
        public Customer Customer { get; }

        public NewRegistrationEvent(Customer customer)
        {
            Customer = customer;
        }
    }
}
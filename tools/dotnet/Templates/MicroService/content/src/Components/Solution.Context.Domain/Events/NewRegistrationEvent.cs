using NetFusion.Messaging.Types;
using Solution.Context.Domain.Entities;

namespace Solution.Context.Domain.Events
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
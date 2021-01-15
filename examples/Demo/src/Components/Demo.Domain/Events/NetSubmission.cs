using NetFusion.Messaging.Types;

namespace Demo.Domain.Events
{
    public class NewSubmission : DomainEvent
    {
        public string CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string State { get; set; }
    }
}
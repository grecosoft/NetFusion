using Demo.Infra;
using NetFusion.Messaging.Types;

namespace Demo.App.DomainEvents
{
    public class RegistrationFailedEvent : DomainEvent
    {
        public string ReferenceNumber { get; }
        public string Make { get; }
        public string Model { get; }
        public int Year { get; set; }

        public RegistrationFailedEvent(
            string referenceNumber, 
            string make,
            string model,
            int year)
        {
            ReferenceNumber = referenceNumber;
            Make = make;
            Model = model;
            Year = year;
        }
    }
}
using NetFusion.Messaging.Types;

namespace Service.Domain.Events
{
    public class NewAccountCreated : DomainEvent
    {
        public string FirstName { get; }
        public string LastName { get; }
        public string AccountNumber { get; }

        public NewAccountCreated(
            string firstName, 
            string lastName,
            string accountNumber)
        {
            FirstName = firstName;
            LastName = lastName;
            AccountNumber = accountNumber;
        }
    }
}
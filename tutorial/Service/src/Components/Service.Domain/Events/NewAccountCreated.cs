using NetFusion.Base.Logging;
using NetFusion.Messaging.Types;

namespace Service.Domain.Events
{
    public class NewAccountCreated : DomainEvent,
        ITypeLog
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

        public object Log()
        {
            return new
            {
                State = AccountNumber,
                Attributes
            };
        }
    }
}
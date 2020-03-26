using NetFusion.Messaging.Types;
using Demo.Domain.Entities;

namespace Demo.Domain.Commands
{
    public class RegisterCustomerCommand : Command<RegistrationStatus>
    {
        public string FirstName { get; }
        public string LastName { get; }
        public string State { get; }

        public RegisterCustomerCommand(
            string firstName,
            string lastName,
            string state)
        {
            FirstName = firstName;
            LastName = lastName;
            State = state;
        }
    }
}

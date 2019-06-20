using NetFusion.Messaging.Types;
using Demo.Domain.Entities;

namespace Demo.Domain.Commands
{
    public class RegisterAutoCommand : Command<RegistrationStatus>
    {
        public string Make { get; }
        public string Model { get; }
        public int Year { get; set; }
        public string State { get; set;}

        public RegisterAutoCommand(
            string make,
            string model,
            int year,
            string state)
        {
            Make = make;
            Model = model;
            Year = year;
            State = state;
        }
    }
}

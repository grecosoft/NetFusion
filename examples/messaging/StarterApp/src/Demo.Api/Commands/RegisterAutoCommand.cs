using NetFusion.Messaging.Types;

namespace Demo.Api.Commands
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
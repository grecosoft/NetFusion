using NetFusion.Messaging.Types;
using Solution.Context.Domain.Entities;

namespace Solution.Context.Domain.Commands
{
    public class RegistrationCommand : Command<Customer>
    {
        public string Prefix { get; set;}
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }
}
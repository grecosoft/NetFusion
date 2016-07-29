using System.Collections.Generic;

namespace RefArch.Api.Models
{
    public class Customer
    {
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public IDictionary<string, object> NameValues { get; set; }
    }
}

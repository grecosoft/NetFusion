using System.Collections.Generic;

namespace WebApiHost.MongoDB.Models
{
    public class CustomerModel
    {
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
    }
}
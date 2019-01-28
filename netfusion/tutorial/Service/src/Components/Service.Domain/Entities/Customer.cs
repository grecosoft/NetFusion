namespace Service.Domain.Entities
{
    using System.Collections.Generic;

    public class Customer
    {
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string JobTitle { get; set; }
        public string CompanyName { get; set; }
        public ICollection<Address> Addresses { get; set; }
    }
}
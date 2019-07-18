using System;

namespace Solution.Context.Domain.Entities
{
    public class Address
    {
        public Guid Id { get; private set; }
        public Guid CustomerId { get; private set; }
        public string AddressLine1 { get; private set; }
        public string AddressLine2 { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string ZipCode { get; private set; }

        public bool IsPrimaryAddress { get; private set; }
        public DateTime? DateBecamePrimary { get; private set; }

        public static Address Define(Guid customerId, string line1, string line2,
            string city,
            string state,
            string zip)
        {
            var address = new Address 
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                IsPrimaryAddress = false
            };

            address.SetAddress(line1, line2);
            address.SetLocation(city, state, zip);

            return address;
        }

        public void SetAddress(string line1, string line2) 
        {
            AddressLine1 = line1;
            AddressLine2 = line2;
        }

        public void SetLocation(string city, string state, string zip) 
        {
            City = city;
            State = state;
            ZipCode = zip;
        }

        public void SetPrimaryIndicator(bool isPrimary)
        {
            IsPrimaryAddress = isPrimary;
            DateBecamePrimary = isPrimary ? (DateTime?)DateTime.UtcNow : null;
        }
    }
}
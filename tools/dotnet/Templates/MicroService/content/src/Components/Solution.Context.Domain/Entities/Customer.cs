using System;
using System.Collections.Generic;
using System.Linq;

namespace Solution.Context.Domain.Entities 
{
    public class Customer 
    {
        public Guid Id { get; private set;}
        public string Prefix { get; private set;}
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public int Age { get; private set; }
        public DateTime RegistrationDate { get; private set; }
        
        public IEnumerable<Address> Addresses => _addresses;

        private readonly List<Address> _addresses = new List<Address>();

        public static Customer Register(
            string prefix,
            string firstName,
            string lastName,
            int age) 
        {
            var customer = new Customer {
                Id = Guid.NewGuid(),
                RegistrationDate = DateTime.UtcNow
            };

            customer.SetContact(prefix, firstName, lastName);
            customer.SetAge(age);

            return customer;
        }

        public void SetContact(string prefix, string firstName, string lastName) 
        {
            Prefix = prefix;
            FirstName = firstName;
            LastName = lastName;
        }

        public void SetAge(int age)
        {
            Age = age;
        }

        public Address GetPrimaryAddress()
        {
            return _addresses.FirstOrDefault(a => a.IsPrimaryAddress);
        }

        public void AssignAddress(Address address)
        {
            if (_addresses.Any(a => a.Id == address.Id))
            {
                throw new InvalidOperationException(
                    "Address already assigned to customer.");
            }

            if (address.IsPrimaryAddress) 
            {
                var currPrimary = _addresses.FirstOrDefault(a => a.IsPrimaryAddress);
                currPrimary?.SetPrimaryIndicator(false);
            }

            _addresses.Add(address);
        }

        public void SetPrimaryAddressTo(Guid addressId)
        {
            Address current = GetPrimaryAddress();
            current?.SetPrimaryIndicator(false);

            Address address = _addresses.FirstOrDefault(a => a.Id == addressId);
            address?.SetPrimaryIndicator(true);
        }
    }
}
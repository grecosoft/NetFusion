using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NetFusion.Base.Validation;
using System.Linq;

namespace Demo.App.Entities
{
    public class Customer : IValidatableType
    {
        [Required, MaxLength(20)]
        public string FirstName { get; set; }

        [Required, MaxLength(20)]
        public string LastName {get; set; }
        public IList<Address> Addresses { get; set; }

        public Customer()
        {
             Addresses = new List<Address>();
        }

        private void AddAddress(Address address)
        {
           Addresses.Add(address);
        }
        public void Validate(IObjectValidator validator)
        {
            validator.Verify(FirstName != LastName, "First Name cannot Equal Last Name!");
            validator.Verify(Addresses.Count() > 0, "Must have at least one address.");

            foreach(Address address in Addresses)
            {
                validator.AddChild(address);
            }
        }
    }
}

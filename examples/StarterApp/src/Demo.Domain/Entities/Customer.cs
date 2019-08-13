using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NetFusion.Base.Validation;

namespace Demo.Domain.Entities
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
            validator.Verify(Addresses.Any(), "Must have at least one address.");
            
            validator.AddChildren(Addresses);
        }
    }
}

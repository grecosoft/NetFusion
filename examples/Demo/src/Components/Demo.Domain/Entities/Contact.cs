using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NetFusion.Base.Validation;

namespace Demo.Domain.Entities
{
    public class Contact : IValidatableType
    {
        [Required, MaxLength(20)]
        public string FirstName { get; set; }

        [Required, MaxLength(20)]
        public string LastName {get; set; }
        public IList<ContactAddress> Addresses { get; set; }

        public Contact()
        {
             Addresses = new List<ContactAddress>();
        }

        private void AddAddress(ContactAddress address)
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

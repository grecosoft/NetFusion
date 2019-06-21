using System.ComponentModel.DataAnnotations;
using NetFusion.Base.Validation;

namespace Demo.App.Entities
{
    public class Address : IValidatableType
    {
        [Required, MaxLength(30)]
        public string Street { get; set; }
        [Required, MaxLength(40)]
        public string City { get; set; }
        [Required, StringLength(2, MinimumLength = 2)]
        public string State { get; set; }
        public string ZipCode { get; set; }

        public void Validate(IObjectValidator validator)
        {
            validator.Verify(State != "NC", "Can't move here.  State if Full!!!");
        }
    }
}

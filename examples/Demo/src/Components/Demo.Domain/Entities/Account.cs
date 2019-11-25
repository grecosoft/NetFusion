using System.Collections.Generic;
using System.Runtime.Serialization;
using NetFusion.Base.Entity;

namespace Demo.Domain.Entities
{
    public class Account : IAttributedEntity
    {
        public Account()
        {
            Attributes = new EntityAttributes();
        }

        // Typical static defined properties:
        public string FirstName { get; }
        public string LastName { get; }
        
        public Account(string firstName, string lastName): this()
        {
            FirstName = firstName;
            LastName = lastName;
        }
       
        /// <summary>
        /// Dynamic message attributes that can be associated with the command.
        /// </summary>
        [IgnoreDataMember]
        public IEntityAttributes Attributes { get; }

        /// <summary>
        /// Dynamic message attribute values.
        /// </summary>
        public IDictionary<string, object> AttributeValues
        {
            get => Attributes.GetValues();
            set => Attributes.SetValues(value);
        }
    }
}

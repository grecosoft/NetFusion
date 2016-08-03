using NetFusion.Domain.Entity;
using System.Collections.Generic;

namespace RefArch.Api.Models
{
    public class DynamicDomainEntity : IAttributedEntity
    {
        private IEntityAttributes _attributes;

        public DynamicDomainEntity()
        {
            _attributes = new EntityAttributes();
        }

        public IEntityAttributes Attributes => _attributes;

        public IDictionary<string, object> AttributeValues
        {
            get { return _attributes.GetValues(); }
            set { _attributes.SetValues(value); }
        }


        public bool IsActive { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public string ItemName { get; set; }
    }
}

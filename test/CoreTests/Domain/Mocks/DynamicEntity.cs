using NetFusion.Domain.Entities;
using System;
using System.Collections.Generic;

namespace CoreTests.Domain.Mocks
{
    public class DynamicEntity : IAttributedEntity
    {
        private IEntityAttributes _attributes;

        public DynamicEntity()
        {
            _attributes = new EntityAttributes();
        }

        public bool IsActive { get; set; }
        public int MaxValue { get; set; }
        public int MinValue { get; set; }

        public IEntityAttributes Attributes => _attributes;

        public IDictionary<string, object> AttributeValues
        {
            get { return _attributes.GetValues(); }
            set { _attributes.SetValues(value); }
        }
    }
}

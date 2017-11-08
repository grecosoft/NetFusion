using NetFusion.Base.Entity;
using System.Collections.Generic;

namespace CommonTest.Base.AttributedEntity
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

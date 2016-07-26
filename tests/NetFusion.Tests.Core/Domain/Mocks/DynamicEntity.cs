using NetFusion.Domain.Entity;
using System.Collections.Generic;

namespace NetFusion.Tests.Core.Domain.Mocks
{
    public class DynamicEntity : IAttributedEntity
    {
        private AttributedEntity _attributes;

        public DynamicEntity()
        {
            _attributes = new AttributedEntity();
        }

        public bool IsActive { get; set; }

        public IEnumerable<EntityAttributeValue> AttributeValues
        {
            get { return _attributes.GetPropertyValues(); }
            set { _attributes = new AttributedEntity(value); }
        }

        public dynamic Attributes { get { return _attributes.Attributes; } }

        public void SetAttributeValue(string name, object value)
        {
            _attributes.SetAttributeValue(name, value);
        }

        public bool DeleteAttribute(string name)
        {
            return _attributes.DeleteAttribute(name);
        }

        public bool ContainsAttribute(string name)
        {
            return _attributes.ContainsAttribute(name);
        }

        public T GetAttributeValue<T>(string name)
        {
            return _attributes.GetAttributeValue<T>(name);
        }

        public T GetAttributeValueOrDefault<T>(string name, T defaultValue = default(T))
        {
            return _attributes.GetAttributeValueOrDefault<T>(name, defaultValue);
        }
    }
}

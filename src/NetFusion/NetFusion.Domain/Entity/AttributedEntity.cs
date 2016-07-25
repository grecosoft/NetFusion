using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace NetFusion.Domain.Entity
{
    /// <summary>
    /// Default implementation of a set of properties that
    /// can be dynamically associated with a given application's
    /// domain entity.
    /// </summary>
    public class AttributedEntity
    {
        // This is C#'s implementation of dynamic object properties
        // that can be populated from a key/value dictionary. 
        private ExpandoObject _expando;

        // Access to the ExpandoObject as key/value dictionary.
        private IDictionary<string, object> _state;

        // Allows access to the dynamic properties using
        // the typical C# syntax.
        public dynamic Attributes { get; }

        // Creates new instance to hold an entity's dynamic data.
        public AttributedEntity() :
            this(new EntityAttributeValue[] { })
        {

        }

        // Creates a new instance based a an existing set of key/value pairs.
        public AttributedEntity(IEnumerable<EntityAttributeValue> propertyValues)
        {
            _expando = new ExpandoObject();
            _state = _expando;

            foreach (var prop in propertyValues)
            {
                _state.Add(prop.Name, prop.Value);
            }

            this.Attributes = _state;
        }

        public IEnumerable<EntityAttributeValue> GetPropertyValues()
        {
            var props = (IDictionary<string, object>)this.Attributes;
            return props.Select(p => new EntityAttributeValue { Name = p.Key, Value = p.Value });
        }

        public void SetAttributeValue(string name, object value)
        {
            _state[name] = value;
        }

        public T GetAttributeValue<T>(string name)
        {
            return (T)_state[name];
        }

        public T GetAttributeValueOrDefault<T>(string name, T defaultValue = default(T))
        {
            if (_state.ContainsKey(name))
            {
                return GetAttributeValue<T>(name);
            }

            return defaultValue;
        }

        public bool DeleteAttribute(string name)
        {
            return _state.Remove(name);
        }

        public bool ContainsAttribute(string name)
        {
            return _state.ContainsKey(name);
        }
    }
}

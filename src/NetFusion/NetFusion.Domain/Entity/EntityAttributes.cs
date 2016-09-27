using NetFusion.Common;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace NetFusion.Domain.Entity
{
    /// <summary>
    /// Default implementation of a set of properties that
    /// can be dynamically associated with a given domain entity.
    /// </summary>
    [Serializable]
    public class EntityAttributes : DynamicObject,
        IEntityAttributes
    {
        // Backing variable to store name/value pairs.
        private IDictionary<string, object> _attributes;

        public EntityAttributes()
        {
            _attributes = new Dictionary<string, object>();
        }

        public dynamic Values => this;

        // DynamicObject Overrides:
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _attributes.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _attributes[binder.Name] = value;
            return true;
        }

        // ---

        public void SetValues(IDictionary<string, object> values)
        {
            Check.NotNull(values, nameof(values));
            this._attributes = values;
        }

        public IDictionary<string, object> GetValues()
        {
            return _attributes;
        }

        public void SetValue(string name, object value)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));
            Check.NotNull(value, nameof(value));

            _attributes[name] = value;
        }

        public T GetValue<T>(string name)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));

            AssertValidKey(name);
            return (T)_attributes[name];
        }

        private void AssertValidKey(string name)
        {
            if (!_attributes.ContainsKey(name))
            {
                throw new InvalidOperationException(
                    $"The attribute with the name: {name} does not exist");
            }
        }

        public object GetValue(string name)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));

            AssertValidKey(name);
            return _attributes[name];
        }

        public T GetValueOrDefault<T>(string name, T defaultValue = default(T))
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));

            if (_attributes.ContainsKey(name))
            {
                return GetValue<T>(name);
            }

            return defaultValue;
        }

        public bool Delete(string name)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));

            return _attributes.Remove(name);
        }

        public bool Contains(string name)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));

            return _attributes.ContainsKey(name);
        }

        private string GetBasePropertyName(string methodName)
        {
            Check.NotNullOrWhiteSpace(methodName, nameof(methodName));

            return methodName.Replace("Get", "").Replace("Set", "");
        }

        public void SetValue<T>(T value, Type context = null,
            [CallerMemberName] string methodName = null)
        {
            string prefix = context != null ? context.FullName + "-" : "";
            methodName = GetBasePropertyName(methodName);
            this.SetValue(prefix + methodName, value);
        }

        public T GetValue<T>(T defaultValue, Type context = null,
            [CallerMemberName] string name = null)
        {
            string prefix = context != null ? context.FullName + "-" : "";

            name = GetBasePropertyName(name);
            return this.GetValueOrDefault<T>(prefix + name, defaultValue);
        }

        public T GetValue<T>(Type context = null, [CallerMemberName] string name = null)
        {
            string prefix = context != null ? context.FullName + "-" : "";

            name = GetBasePropertyName(name);
            return this.GetValue<T>(prefix + name);
        }
    }
}

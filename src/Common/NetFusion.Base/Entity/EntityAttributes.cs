using NetFusion.Common;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;

#if NET461 
using System.Runtime.Serialization;
#endif

namespace NetFusion.Base.Entity
{
    /// <summary>
    /// Default implementation of a set of properties that can be dynamically
    /// associated with a given domain entity.  An domain entity can provide
    /// dynamic behaviors by implementing the IAttributedEntity interface and
    /// delegating to an instance of this class.
    /// </summary>
#if NET461
    [Serializable]
#endif
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

        //------------------------------------------ DYNAMIC OBJECT OVERRIDES ------------------------------------------//
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _attributes.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _attributes[binder.Name] = value;
            return true;
        }

        //------------------------------------------ VALUE MAINTENANCE ------------------------------------------------//
        public void SetValues(IDictionary<string, object> values)
        {
            Check.NotNull(values, nameof(values));
            _attributes = values;
        }

        public IDictionary<string, object> GetValues()
        {
            return _attributes;
        }

        public void SetValue(string name, object value, 
            Type context = null,
            bool overrideIfPresent = false)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));

            string prefixedNamed = GetPropertyPrefixedName(context, name);

            if (_attributes.ContainsKey(name) && !overrideIfPresent) return;
            _attributes[prefixedNamed] = value;
        }

        public object GetValue(string name, Type context = null)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));

            string prefixedNamed = GetPropertyPrefixedName(context, name);

            AssertValidKey(prefixedNamed);
            return _attributes[prefixedNamed];
        }

        public T GetValue<T>(string name, Type context = null)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));

            return (T)GetValue(name, context);
        }

        public T GetValueOrDefault<T>(string name, T defaultValue = default(T), Type context = null)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));

            string prefixedNamed = GetPropertyPrefixedName(context, name);
            if (_attributes.ContainsKey(prefixedNamed))
            {
                return (T)GetValue(name, context);
            }

            return defaultValue;
        }

        public bool Contains(string name, Type context = null)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));

            string prefixedNamed = GetPropertyPrefixedName(context, name);
            return _attributes.ContainsKey(prefixedNamed);
        }

        public bool Delete(string name, Type context = null)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));

            string prefixedNamed = GetPropertyPrefixedName(context, name);
            return _attributes.Remove(prefixedNamed);
        }

        private string GetPropertyPrefixedName(Type context, string name)
        {
            return context != null ? context.Namespace + "-" + name : name;
        }

        private void AssertValidKey(string name)
        {
            if (!_attributes.ContainsKey(name))
            {
                throw new InvalidOperationException(
                    $"The attribute with the name: {name} does not exist");
            }
        }

        //------------------------------------------ CALLER MEMBER STORED VALUES -------------------------------------//
        // Returns the corresponding property for a given method name.
        private string GetBasePropertyName(string methodName)
        {
            Check.NotNullOrWhiteSpace(methodName, nameof(methodName));

            return methodName.Replace("Get", "").Replace("Set", "");
        }

        public void SetMemberValue<T>(T value, Type context = null, bool overrideIfPresent = false,
            [CallerMemberName] string callerName = null)
        {
            string propName = GetBasePropertyName(callerName);
            SetValue(propName, value, context, overrideIfPresent);
        }

        public T GetMemberValueOrDefault<T>(T defaultValue = default(T), Type context = null,
            [CallerMemberName] string callerName = null)
        {
            string propName = GetBasePropertyName(callerName);
            return GetValueOrDefault(propName, defaultValue, context);
        }
    }
}

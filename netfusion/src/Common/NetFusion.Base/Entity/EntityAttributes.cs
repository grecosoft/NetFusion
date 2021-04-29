using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace NetFusion.Base.Entity
{
    /// <summary>
    /// Default implementation of a set of properties that can be dynamically associated with 
    /// a given domain entity.  An domain entity can provide dynamic behaviors by implementing 
    /// the IAttributedEntity interface and delegating to an instance of this class.
    /// </summary>
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

        //-------------------------- [Dynamic Object Overrides] -------------------------//
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _attributes.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _attributes[binder.Name] = value;
            return true;
        }

        //----------------------------- [Value Maintenance] ----------------------------//
        public void SetValues(IDictionary<string, object> values)
        {
            _attributes = values ?? throw new ArgumentNullException(nameof(values), "Values cannot be null.");
        }

        public IDictionary<string, object> GetValues() => _attributes;

        public void SetValue(string name, object value, bool overrideIfPresent = true)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(
                "Attribute name cannot be null or empty string.", nameof(name));
            
            if (value == null) throw new ArgumentNullException(nameof(value));

            if (_attributes.ContainsKey(name) && !overrideIfPresent)
            {
                return;
            }
            _attributes[name] = value;
        }

        public object GetValue(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(
                "Attribute name cannot be null or empty string.", nameof(name));

            AssertValidKey(name);
            return _attributes[name];
        }
        
        public object GetValueOrDefault(string name, object defaultValue = default)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(
                "Attribute name cannot be null or empty string.", nameof(name));
            
            return _attributes.ContainsKey(name) ? _attributes[name] : defaultValue;
        }

        public bool TryGetValue(string name, out object value)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(
                "Attribute name cannot be null or empty string.", nameof(name));

            return _attributes.TryGetValue(name, out value);
        }

        public T GetValue<T>(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(
                "Attribute name cannot be null or empty string.", nameof(name));

            AssertValidKey(name);
            return (T)_attributes[name];
        }

        public bool TryGetValue<T>(string name, out T value)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(
                "Attribute name cannot be null or empty string.", nameof(name));

            if (_attributes.TryGetValue(name, out object attribValue))
            {
                value = (T)attribValue;
                return true;
            }

            value = default;
            return false;
        }

        public T GetValueOrDefault<T>(string name, T defaultValue = default)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(
                "Attribute name cannot be null or empty string.", nameof(name));

            return _attributes.ContainsKey(name) ? (T)_attributes[name] : defaultValue;
        }

        public bool Contains(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(
                "Attribute name cannot be null or empty string.", nameof(name));

            return _attributes.ContainsKey(name);
        }

        public bool Delete(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(
                "Attribute name cannot be null or empty string.", nameof(name));
            
            return _attributes.Remove(name);
        }

        private void AssertValidKey(string name)
        {
            if (! _attributes.ContainsKey(name))
            {
                throw new InvalidOperationException(
                    $"The attribute with the name: {name} does not exist");
            }
        }

        //-------------------------- [ Caller Member Stored Values ] -------------------------//

        // Returns the corresponding property for a given method name.
        private static string GetBasePropertyName(string methodName)
        {
            if (string.IsNullOrWhiteSpace(methodName)) throw new ArgumentException(
                "Method name cannot be null or empty string.", nameof(methodName));

            return methodName.Replace("Get", "").Replace("Set", "");
        }

        public void SetMemberValue<T>(T value, bool overrideIfPresent = true,
            [CallerMemberName] string callerName = null)
        {
            string propName = GetBasePropertyName(callerName);
            SetValue(propName, value, overrideIfPresent);
        }

        public T GetMemberValueOrDefault<T>(T defaultValue = default, [CallerMemberName] string callerName = null)
        {
            string propName = GetBasePropertyName(callerName);
            return GetValueOrDefault(propName, defaultValue);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NetFusion.Messaging.Types.Attributes
{
    /// <summary>
    /// Provides extension methods for reading basic types and array of basic types
    /// stored within a dictionary where the key is the name and the value is the
    /// string representation.  This is being used so deserialization will not be
    /// dependent on the specific serializer being used.  (JSON serializers and
    /// MessagePack serializers will not know the exact type if object was used
    /// for the dictionary's type.  These serializers will deserialize the value
    /// into an object representing the value that is dependent on their library).  
    /// </summary>
    public static class AttributeSetExtensions
    {
        public const string ArraySeparator = "^|";
        
        //--  Integer
        
        public static IDictionary<string, string> SetIntValue(this IDictionary<string, string> attributes, 
            string name, int value, 
            bool overrideIfPresent = true)
        {
            return SetAttribute(attributes, name, value.ToString(), overrideIfPresent);
        }

        public static IDictionary<string, string> SetIntValue(this IDictionary<string, string> attributes, 
            string name, IEnumerable<int> values, 
            bool overrideIfPresent = true)
        {
            var value = values.Select(v => v.ToString()).ToArray();
            return SetAttribute(attributes, name, string.Join(ArraySeparator, value), overrideIfPresent);
        }
     
        //--  Byte
        
        public static IDictionary<string, string> SetByteValue(this IDictionary<string, string> attributes, 
            string name, byte value, 
            bool overrideIfPresent = true)
        {
            return SetAttribute(attributes, name, value.ToString(), overrideIfPresent);
        }
        
        //--  Decimal
        
        public static IDictionary<string, string> SetDecimalValue(this IDictionary<string, string> attributes, 
            string name, decimal value, 
            bool overrideIfPresent = true)
        {
            return SetAttribute(attributes, name, value.ToString(CultureInfo.InvariantCulture), overrideIfPresent);
        }
        
        public static IDictionary<string, string> SetDecimalValue(this IDictionary<string, string> attributes,
            string name, IEnumerable<decimal> values, 
            bool overrideIfPresent = true)
        {
            var value = values.Select(v => v.ToString(CultureInfo.InvariantCulture)).ToArray();
            return SetAttribute(attributes, name, string.Join(ArraySeparator, value), overrideIfPresent);
        }
        
        //--  String
        
        public static IDictionary<string, string> SetStringValue(this IDictionary<string, string> attributes, 
            string name, string value, 
            bool overrideIfPresent = true)
        {
            return SetAttribute(attributes, name, value, overrideIfPresent);
        }

        public static IDictionary<string, string> SetStringValue(this IDictionary<string, string> attributes, 
            string name, IEnumerable<string> values, 
            bool overrideIfPresent = true)
        {
            return SetAttribute(attributes, name, string.Join(ArraySeparator, values), overrideIfPresent);
        }
        
        //--  Boolean
        
        public static IDictionary<string, string> SetBoolValue(this IDictionary<string, string> attributes, 
            string name, bool value, 
            bool overrideIfPresent = true)
        {
            return SetAttribute(attributes, name, value.ToString(), overrideIfPresent);
        }
        
        //--  Guid
        
        public static IDictionary<string, string> SetGuidValue(this IDictionary<string, string> attributes,
            string name, Guid value, 
            bool overrideIfPresent = true)
        {
            return SetAttribute(attributes, name, value.ToString(), overrideIfPresent);
        }
        
        //--  DateTime

        public static IDictionary<string, string> SetDateValue(this IDictionary<string, string> attributes, 
            string name, DateTime value, 
            bool overrideIfPresent = true)
        {
            return SetAttribute(attributes, name, value.ToString("o"), overrideIfPresent);
        }
        
        public static IDictionary<string, string> SetUtcDateValue(this IDictionary<string, string> attributes, 
            string name, DateTime value, 
            bool overrideIfPresent = true)
        {
            value = value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : value;
            return SetAttribute(attributes, name, value.ToString("o"), overrideIfPresent);
        }
        
        //-- TimeSpan

        public static IDictionary<string, string> SetTimeSpan(this IDictionary<string, string> attributes,
            string name, TimeSpan value,
            bool overrideIfPresent = true)
        {
            return SetAttribute(attributes, name, value.ToString(), overrideIfPresent);
        }
        
        //--  UInt
        
        public static IDictionary<string, string> SetUIntValue(this IDictionary<string, string> attributes, 
            string name, uint value, 
            bool overrideIfPresent = true)
        {
            return SetAttribute(attributes, name, value.ToString(CultureInfo.InvariantCulture), overrideIfPresent);
        }
        
        private static IDictionary<string, string> SetAttribute(IDictionary<string, string> attributes,
            string name, string value, 
            bool overrideIfPresent)
        {
            if (attributes == null) throw new ArgumentNullException(nameof(attributes));
            
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Value must be specified.", nameof(value));


            if (attributes.ContainsKey(name) && !overrideIfPresent)
            {
                return attributes;
            }

            attributes[name] = value;
            return attributes;
        }
    }
}
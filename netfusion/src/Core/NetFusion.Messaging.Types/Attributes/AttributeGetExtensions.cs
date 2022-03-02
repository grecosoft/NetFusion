using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Messaging.Types.Attributes
{
    /// <summary>
    /// Provides extension methods for reading basic types and arrays of basic types
    /// stored within a dictionary where the key is the name and the value is the
    /// string representation.  This is being used so deserialization will not be
    /// dependent on the specific serializer being used.  (JSON serializers and
    /// MessagePack serializers will not know the exact type if object was used
    /// for the dictionary's type.  These serializers will deserialize the value
    /// into an object representing the value that is dependent on their library).  
    /// </summary>
    public static class AttributeGetExtensions
    {

        public static bool HasValue(this IDictionary<string, string> attributes, string name)
        {
            if (attributes == null) throw new ArgumentNullException(nameof(attributes));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name not specified.", nameof(name));

            return attributes.ContainsKey(name);
        }
        
        //--  Integer
        
        public static int GetIntValue(this IDictionary<string, string> attributes, string name)
        {
            string value = GetValue(attributes, name);
            if (value == null)
            {
                throw new InvalidOperationException($"Attribute named: {name} not found.");
            }

            if (! int.TryParse(value, out int parsedValue))
            {
                throw new InvalidOperationException(
                    $"Attribute named: {name} with value: {value} could not be parsed as an Integer.");
            }

            return parsedValue;
        }
        
        public static int GetIntValue(this IDictionary<string, string> attributes, string name, int defaultValue)
        {
            string value = GetValue(attributes, name);
            if (value == null)
            {
                return defaultValue;
            }

            if (! int.TryParse(value, out int parsedValue))
            {
                throw new InvalidOperationException(
                    $"Attribute named: {name} with value: {value} could not be parsed as an Integer.");
            }

            return parsedValue;
        }
        
        
        public static int[] GetIntArrayValue(this IDictionary<string, string> attributes, string name)
        {
            string values = GetValue(attributes, name);
            if (values == null)
            {
                throw new InvalidOperationException($"Attribute named: {name} not found.");
            }

            return ParseValues().ToArray();

            IEnumerable<int> ParseValues()
            {
                string[] items = values.Split(AttributeSetExtensions.ArraySeparator);
                foreach (string itemValue in items)
                {
                    if (! int.TryParse(itemValue, out int parsedValue))
                    {
                        throw new InvalidOperationException(
                            $"Attribute named: {name} with value: {parsedValue} could not be parsed as an Integer.");
                    }
                    yield return parsedValue;
                }
            }
        }
        
        //--  UInt
        
        public static uint GetUIntValue(this IDictionary<string, string> attributes, string name)
        {
            string value = GetValue(attributes, name);
            if (value == null)
            {
                throw new InvalidOperationException($"Attribute named: {name} not found.");
            }

            if (! uint.TryParse(value, out uint parsedValue))
            {
                throw new InvalidOperationException(
                    $"Attribute named: {name} with value: {value} could not be parsed as an UInit.");
            }

            return parsedValue;
        }
        
        //--  Byte
        
        public static byte GetByteValue(this IDictionary<string, string> attributes, string name)
        {
            string value = GetValue(attributes, name);
            if (value == null)
            {
                throw new InvalidOperationException($"Attribute named: {name} not found.");
            }

            if (! byte.TryParse(value, out byte parsedValue))
            {
                throw new InvalidOperationException(
                    $"Attribute named: {name} with value: {value} could not be parsed as Byte.");
            }

            return parsedValue;
        }
        
        //--  Decimal
        
        public static decimal GetDecimalValue(this IDictionary<string, string> attributes, string name)
        {
            string value = GetValue(attributes, name);
            if (value == null)
            {
                throw new InvalidOperationException($"Attribute named: {name} not found.");
            }

            if (! decimal.TryParse(value, out decimal parsedValue))
            {
                throw new InvalidOperationException(
                    $"Attribute named: {name} with value: {value} could not be parsed as a Decimal.");
            }

            return parsedValue;
        }
        
        public static decimal GetDecimalValue(this IDictionary<string, string> attributes, string name, decimal defaultValue)
        {
            string value = GetValue(attributes, name);
            if (value == null)
            {
                return defaultValue;
            }

            if (! decimal.TryParse(value, out decimal parsedValue))
            {
                throw new InvalidOperationException(
                    $"Attribute named: {name} with value: {value} could not be parsed as a Decimal.");
            }

            return parsedValue;
        }
        
        public static decimal[] GetDecimalArrayValue(this IDictionary<string, string> attributes, string name)
        {
            string values = GetValue(attributes, name);
            if (values == null)
            {
                throw new InvalidOperationException($"Attribute named: {name} not found.");
            }

            return ParseValues().ToArray();

            IEnumerable<decimal> ParseValues()
            {
                string[] items = values.Split(AttributeSetExtensions.ArraySeparator);
                foreach (string itemValue in items)
                {
                    if (! decimal.TryParse(itemValue, out decimal parsedValue))
                    {
                        throw new InvalidOperationException(
                            $"Attribute named: {name} with value: {parsedValue} could not be parsed as a Decimal.");
                    }
                    
                    yield return parsedValue;
                }
            }
        }
        
        //--  String
        
        public static string GetStringValue(this IDictionary<string, string> attributes, string name)
        {
            string value = GetValue(attributes, name);
            if (value == null)
            {
                throw new InvalidOperationException($"Attribute named: {name} not found.");
            }

            return value;
        }
        
        public static string GetStringValue(this IDictionary<string, string> attributes, string name, string defaultValue)
        {
            return GetValue(attributes, name) ?? defaultValue;
        }
        
        public static string[] GetStringArrayValue(this IDictionary<string, string> attributes, string name)
        {
            string values = GetValue(attributes, name);
            if (values == null)
            {
                throw new InvalidOperationException($"Attribute named: {name} not found.");
            }

            return values.Split(AttributeSetExtensions.ArraySeparator);
        }
        
        //--  Guid
        
        public static Guid GetGuidValue(this IDictionary<string, string> attributes, string name)
        {
            string value = GetValue(attributes, name);
            if (value == null)
            {
                throw new InvalidOperationException($"Attribute named: {name} not found.");
            }

            if (! Guid.TryParse(value, out Guid parsedValue))
            {
                throw new InvalidOperationException(
                    $"Attribute named: {name} with value: {value} could not be parsed as a Guid.");
            }

            return parsedValue;
        }
        
        public static Guid GetGuidValue(this IDictionary<string, string> attributes, string name, Guid defaultValue)
        {
            string value = GetValue(attributes, name);
            if (value == null)
            {
                return defaultValue;
            }

            if (! Guid.TryParse(value, out Guid parsedValue))
            {
                throw new InvalidOperationException(
                    $"Attribute named: {name} with value: {value} could not be parsed as a Guid.");
            }

            return parsedValue;
        }
        
        //--  DateTime
            
        public static DateTime GetDateTimeValue(this IDictionary<string, string> attributes, string name)
        {
            string value = GetValue(attributes, name);
            if (value == null)
            {
                throw new InvalidOperationException($"Attribute named: {name} not found.");
            }

            if (! DateTime.TryParse(value, out DateTime parsedValue))
            {
                throw new InvalidOperationException(
                    $"Attribute named: {name} with value: {value} could not be parsed as a DateTime.");
            }

            return parsedValue;
        }        
        
        public static DateTime GetDateTimeValue(this IDictionary<string, string> attributes, string name, DateTime defaultValue)
        {
            string value = GetValue(attributes, name);
            if (value == null)
            {
                return defaultValue;
            }

            if (! DateTime.TryParse(value, out DateTime parsedValue))
            {
                throw new InvalidOperationException(
                    $"Attribute named: {name} with value: {value} could not be parsed as a DateTime.");
            }

            return parsedValue;
        }
        
        public static DateTime GetUtcDateTimeValue(this IDictionary<string, string> attributes, string name)
        {
            string value = GetValue(attributes, name);
            if (value == null)
            {
                throw new InvalidOperationException($"Attribute named: {name} not found.");
            }

            if (! DateTime.TryParse(value, out DateTime parsedValue))
            {
                throw new InvalidOperationException(
                    $"Attribute named: {name} with value: {value} could not be parsed as a DateTime.");
            }

            return parsedValue.ToUniversalTime();
        }        
        
        public static DateTime GetUtcDateTimeValue(this IDictionary<string, string> attributes, string name, DateTime defaultValue)
        {
            string value = GetValue(attributes, name);
            if (value == null)
            {
                return defaultValue.Kind == DateTimeKind.Local ? defaultValue.ToUniversalTime() : defaultValue;
            }

            if (! DateTime.TryParse(value, out DateTime parsedValue))
            {
                throw new InvalidOperationException(
                    $"Attribute named: {name} with value: {value} could not be parsed as a DateTime.");
            }

            return parsedValue.ToUniversalTime();
        }
        
        //-- TimeSpan
        
        public static TimeSpan GetTimeSpanValue(this IDictionary<string, string> attributes, string name)
        {
            string value = GetValue(attributes, name);
            if (value == null)
            {
                throw new InvalidOperationException($"Attribute named: {name} not found.");
            }

            if (! TimeSpan.TryParse(value, out TimeSpan parsedValue))
            {
                throw new InvalidOperationException(
                    $"Attribute named: {name} with value: {value} could not be parsed as a Guid.");
            }

            return parsedValue;
        }
        
        //--  Boolean

        public static bool GetBoolValue(this IDictionary<string, string> attributes, string name)
        {
            string value = GetValue(attributes, name);
            if (value == null)
            {
                throw new InvalidOperationException($"Attribute named: {name} not found.");
            }

            if (! bool.TryParse(value, out bool parsedValue))
            {
                throw new InvalidOperationException(
                    $"Attribute named: {name} with value: {value} could not be parsed as a Bool.");
            }

            return parsedValue;
        }
        
        public static bool GetBoolValue(this IDictionary<string, string> attributes, string name, bool defaultValue)
        {
            string value = GetValue(attributes, name);
            if (value == null)
            {
                return defaultValue;
            }

            if (! bool.TryParse(value, out bool parsedValue))
            {
                throw new InvalidOperationException(
                    $"Attribute named: {name} with value: {value} could not be parsed as a Bool.");
            }

            return parsedValue;
        }

        private static string GetValue(IDictionary<string, string> attributes, string name)
        {
            if (attributes == null) throw new ArgumentNullException(nameof(attributes));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Attribute Name not specified.", nameof(name));

            attributes.TryGetValue(name, out string value);
            return value;
        }
    }
}
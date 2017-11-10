using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NetFusion.Common.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Given an object returns a dictionary of name/value pairs for each property.
        /// </summary>
        /// <param name="value">The value to be converted to a dictionary.</param>
        /// <returns>Dictionary.</returns>
        public static IDictionary<string, object> ToDictionary(this object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var dictionary = new Dictionary<string, object>();
            var valueTypeInfo = value.GetType().GetTypeInfo();

            foreach (PropertyInfo property in valueTypeInfo.GetProperties()) 
            {
                var obj = property.GetValue(value);
                dictionary.Add(property.Name, obj);
            }
            return dictionary;
        }

        /// <summary>
        /// Returns an object serialized as indented JSON.
        /// </summary>
        /// <param name="value">The value to be serialized.</param>
        /// <returns>JSON encoded object.</returns>
        public static string ToIndentedJson(this object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return JsonConvert.SerializeObject(value, Formatting.Indented);
        }

        /// <summary>
        /// Returns an object serialized as JSON.
        /// </summary>
        /// <param name="value">The value to be serialized.</param>
        /// <returns>JSON encoded object.</returns>
        public static string ToJson(this object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return JsonConvert.SerializeObject(value, Formatting.None);
        }
    }
}

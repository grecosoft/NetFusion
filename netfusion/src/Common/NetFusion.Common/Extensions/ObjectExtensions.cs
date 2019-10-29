using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
        /// <param name="obj">The value to be converted to a dictionary.</param>
        /// <returns>Dictionary.</returns>
        public static IDictionary<string, object> ToDictionary(this object obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var dictionary = new Dictionary<string, object>();
  
            foreach (PropertyInfo property in obj.GetType().GetProperties()) 
            {
                var value = property.GetValue(obj);
                dictionary.Add(property.Name, value);
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
            return JsonConvert.SerializeObject(value, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        /// <summary>
        /// Returns an object serialized as JSON.
        /// </summary>
        /// <param name="value">The value to be serialized.</param>
        /// <returns>JSON encoded object.</returns>
        public static string ToJson(this object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return JsonConvert.SerializeObject(value, Formatting.None, new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }
    }
}

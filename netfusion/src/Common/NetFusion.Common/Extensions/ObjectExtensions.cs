using System;
using System.Text.Json;

namespace NetFusion.Common.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Returns an object serialized as indented JSON.
        /// </summary>
        /// <param name="value">The value to be serialized.</param>
        /// <returns>JSON encoded object.</returns>
        public static string ToIndentedJson(this object value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            return JsonSerializer.Serialize(value, new JsonSerializerOptions
            {
                WriteIndented = true,
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
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
            
            return JsonSerializer.Serialize(value, new JsonSerializerOptions
            {
                IgnoreNullValues = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
    }
}

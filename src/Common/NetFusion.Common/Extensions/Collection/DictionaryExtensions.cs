using System.Collections.Generic;

namespace NetFusion.Common.Extensions.Collection
{
    /// <summary>
    /// IDictionary extension methods.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Returns the value with the specified key or an optional default value.
        /// </summary>
        /// <typeparam name="TKey">The type of the dictionary key.</typeparam>
        /// <typeparam name="TValue">The type of the dictionary value.</typeparam>
        /// <param name="source">The dictionary to be searched.</param>
        /// <param name="key">The key to find the associated value.</param>
        /// <param name="defaultValue">The optional default value.</param>
        /// <returns>The associated key's value.  Otherwise, the default value.</returns>
        public static TValue GetOptionalValue<TKey, TValue>(this IDictionary<TKey, TValue> source,
            TKey key,
            TValue defaultValue = null)
            where TValue : class
        {
            Check.NotNull(source, nameof(source));

            TValue value;
            if (source.TryGetValue(key, out value))
            {
                return value;
            }
            return defaultValue;
        }
    }
}

using System;
using System.Linq;
using System.Reflection;

namespace NetFusion.Common.Extensions.Reflection
{
    public static class AttributeExtensions
    {
        /// <summary>
        /// Determines if source is decorated when an attribute.
        /// </summary>
        /// <typeparam name="T">The type of attribute.</typeparam>
        /// <param name="source">The source to check.</param>
        /// <returns>True if attribute exits.  Otherwise, False.</returns>
        public static bool HasAttribute<T>(this ICustomAttributeProvider source)
            where T : Attribute
        {
            if (source == null)throw new ArgumentNullException(nameof(source));

            return source.GetCustomAttributes(typeof(T), true).Length > 0;
        }

        /// <summary> 
        /// Returns an attribute if present on the specified source.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to find.</typeparam>
        /// <param name="source">The reflected type to check for attribute.</param>
        /// <returns>The attribute if present, otherwise, null is returned.  If more
        /// than one matching attribute, and exception is thrown.</returns>
        public static T GetAttribute<T>(this ICustomAttributeProvider source)
            where T : Attribute
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var matchingAttrs = source.GetCustomAttributes(typeof(T), true);
            if (matchingAttrs.Length > 1)
            {
                throw new InvalidOperationException($"More than one attribute of the type: {typeof(T)} exists.");
            }

            return matchingAttrs.Length == 1 ? (T)matchingAttrs[0] : null;
        }

        /// <summary>
        /// Returns an attribute if present on a specified type.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to fine.</typeparam>
        /// <param name="source">The type to search for attribute.</param>
        /// <returns>True if attribute is found.  Otherwise false.</returns>
        public static bool HasAttribute<T>(this Type source)
            where T : Attribute
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return source.GetTypeInfo().GetCustomAttributes(typeof(T), true).Count() > 0;
        }

        /// <summary>
        /// Returns an attribute if present on the specified source.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to find.</typeparam>
        /// <param name="source">The object to check for attribute.</param>
        /// <returns>The attribute if present, otherwise, null is returned.  
        /// If more than attribute is found, an exception is thrown.</returns>
        public static T GetAttribute<T>(this object source)
            where T : Attribute
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            return GetAttribute<T>(source.GetType());
        }

        /// <summary>
        /// Returns an attribute if present on the specified source.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to fined.</typeparam>
        /// <param name="source">The type to check for attribute.</param>
        /// <returns>The attribute if present, otherwise null is returned.  
        /// If more than one attribute is found, and exception is thrown.</returns>
        public static T GetAttribute<T>(this Type source)
            where T : Attribute
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var matchingAttrs = source.GetTypeInfo().GetCustomAttributes<T>(true);
            if (matchingAttrs.Count() > 1)
            {
                throw new InvalidOperationException($"More than one attribute of the type: {typeof(T)} exists.");
            }

            return matchingAttrs.FirstOrDefault();
        }
    }
}

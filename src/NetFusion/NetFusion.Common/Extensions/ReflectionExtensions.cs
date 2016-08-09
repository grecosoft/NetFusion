using System;
using System.Reflection;

namespace NetFusion.Common.Extensions
{
    /// <summary>
    /// Extension methods for checking properties of a type.
    /// </summary>
    public static class ReflectionExtensions
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
            Check.NotNull(source, nameof(source));
            return source.GetCustomAttributes(typeof(T), true).Length > 0;
        }

        /// <summary>
        /// Returns an attribute if present on the specified source.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to find.</typeparam>
        /// <param name="source">The reflected type to check for attribute.</param>
        /// <returns>The attribute if present, otherwise, null is returned.</returns>
        public static T GetAttribute<T>(this ICustomAttributeProvider source)
            where T : Attribute
        {
            Check.NotNull(source, nameof(source));

            var matchingAttrs = source.GetCustomAttributes(typeof(T), true);
            if (matchingAttrs.Length > 1)
            {
                throw new InvalidOperationException($"More than one attribute of the type: {typeof(T)} exists.");
            }

            return matchingAttrs.Length == 1 ? (T)matchingAttrs[0] : null;
        }

        /// <summary>
        /// Returns an attribute if present on the specified source.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to find.</typeparam>
        /// <param name="source">The object to check for attribute.</param>
        /// <returns>The attribute if present, otherwise, null is returned.</returns>
        public static T GetAttribute<T>(this object source)
            where T : Attribute
        {
            Check.NotNull(source, nameof(source));

            return GetAttribute<T>(source.GetType());
        }
    }
}

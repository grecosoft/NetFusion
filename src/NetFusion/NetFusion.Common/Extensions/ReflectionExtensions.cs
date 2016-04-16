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
    }
}

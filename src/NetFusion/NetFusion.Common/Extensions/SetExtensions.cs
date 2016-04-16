﻿using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Common.Extensions
{
    /// <summary>
    /// Extension methods for checking set membership.
    /// </summary>
    public static class SetExtensions
    {
        /// <summary>
        /// Determines if the specified source value matches any of the listed values.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="source">The value to check if membership.</param>
        /// <param name="values">The values to test if source is a subset.</param>
        /// <returns>True values contains source.  Otherwise, False.</returns>
        public static bool InSet<T>(this T source, params T[] values)
        {
            return values.Contains(source);
        }

        /// <summary>
        /// Determines if the source enumeration contains any of the listed values.
        /// </summary>
        /// <typeparam name="T">The type to check.</typeparam>
        /// <param name="source">The list of values.</param>
        /// <param name="values">The values to check if any given one is a member of source</param>
        /// <returns>True if a value is a member of source.  Otherwise, False.</returns>
        public static bool ContainsAny<T>(this IEnumerable<T> source, params T[] values)
        {
            Check.NotNull(source, nameof(source));

            return values.Any(source.Contains);
        }
    }
}


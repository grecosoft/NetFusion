using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NetFusion.Common.Extensions.Collections
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Checks to see of the enumerable has been materialized and if not converts it to a
        /// list and enumerates the items calling a delegate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="valueAction"></param>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static IList<T> ForEach<T>(this IEnumerable<T> source, Action<T> valueAction)
        {
            Check.NotNull(source, "source");
            Check.NotNull(valueAction, "valueAction");

            var list = (source as IList<T>) ?? source.ToList();
            foreach (var value in list)
            {
                valueAction(value);
            }

            return list;
        }

        /// <summary>
        /// Returns all the values contained within the specified lookup.
        /// </summary>
        /// <typeparam name="TKey">The type of the key on which the lookup is based.</typeparam>
        /// <typeparam name="TElement">The type of element contained in each lookup key list.</typeparam>
        /// <param name="source">Reference to a lookup.</param>
        /// <returns>All values contained for every key in the lookup.</returns>
        [DebuggerStepThrough]
        public static IEnumerable<TElement> Values<TKey, TElement>(this ILookup<TKey, TElement> source)
        {
            Check.NotNull(source, "source");
            return source.SelectMany(v => v);
        }

        /// <summary>
        /// Determines if enumerable is empty.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="source">The source enumeration.</param>
        /// <returns>True if the enumeration contains zero items.</returns>
        [DebuggerStepThrough]
        public static bool Empty<T>(this IEnumerable<T> source)
        {
            Check.NotNull(source, "source");
            return !source.Any();
        }

        /// <summary>
        /// Determines if the enumerable contains one and only one item.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="source">The source enumerable.</param>
        /// <returns>True if and only if one item.  Otherwise, false.</returns>
        [DebuggerStepThrough]
        public static bool IsSingletonSet<T>(this IEnumerable<T> source)
        {
            Check.NotNull(source, "source");
            return source.Count() == 1;
        } 

        /// <summary>
        /// Checks if a given property of an enumerable element contains duplicate values.
        /// </summary>
        /// <typeparam name="TSource">The source enumerable.</typeparam>
        /// <typeparam name="TKey">The element type.</typeparam>
        /// <param name="source">The source enumeration with potential duplicate property values.</param>
        /// <param name="propertySelector">Specifies the element property that should be tested for duplicate value.</param>
        /// <returns>Enumerable of all the duplicated property values.</returns>
        public static IEnumerable<TKey> WhereDuplicated<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> propertySelector)
        {
            Check.NotNull(source, "source");
            Check.NotNull(propertySelector, "propertySelector");

            return source.GroupBy(propertySelector)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);
        }

        /// <summary>
        /// Orders a list of object instances by their types.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="types"></param>
        /// <returns></returns>
        public static IEnumerable<T> OrderByMatchingType<T>(this IEnumerable<T> source, IEnumerable<Type> types)
            where T : class
        {
            foreach (var itemType in types)
            {
                var itemsOfType = source.Where(d => d.GetType() == itemType);
                foreach(var item in itemsOfType)
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Determines if an enumeration of class instances contains
        /// exactly one item.  
        /// </summary>
        /// <typeparam name="T">The type of the class instances.</typeparam>
        /// <param name="souce">The enumeration to check.</param>
        /// <returns>The value contained in the enumeration if there is
        /// exactly one instance.  Otherwise, null is returned.</returns>
        public static T OneAndOnlyOne<T>(this IEnumerable<T> source)
            where T : class
        {
            Check.NotNull(source, "source");

            if (source.Count() == 1)
            {
                return source.ElementAt(0);
            }
            return null;
        }
    }
}

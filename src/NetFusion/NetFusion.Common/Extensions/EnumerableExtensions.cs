using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NetFusion.Common.Extensions
{
    public static class EnumerableExtensions
    {
        [DebuggerStepThrough]
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> set)
        {
            return set ?? Enumerable.Empty<T>();
        }

        [DebuggerStepThrough]
        public static IList<T> ForEach<T>(this IEnumerable<T> source, Action<T> valueAction)
        {
            Check.NotNull(source, "source");

            var list = (source as IList<T>) ?? source.ToList();
            foreach (var value in list)
            {
                valueAction(value);
            }

            return list;
        }

        [DebuggerStepThrough]
        public static T SingletonOrDefault<T>(this IEnumerable<T> source,
            T defaultValue = default(T))
        {
            return source.Count() == 1 ? source.ElementAt(0) : defaultValue;
        }

        [DebuggerStepThrough]
        public static void ForEachValue<TKey, TElement>(this ILookup<TKey, TElement> source,
            Action<TElement> action)
        {
            foreach (var value in source.Values())
            {
                action(value);
            }
        }

        [DebuggerStepThrough]
        public static void ForEachValue<TKey, TElement>(this IDictionary<TKey, TElement> source,
            Action<TElement> action)
        {
            foreach (var value in source.Values)
            {
                action(value);
            }
        }

        [DebuggerStepThrough]
        public static IEnumerable<TElement> Values<TKey, TElement>(this ILookup<TKey, TElement> source)
        {
            return source.SelectMany(v => v);
        }

        [DebuggerStepThrough]
        public static bool Empty<T>(this IEnumerable<T> source)
        {
            Check.NotNull(source, "source");
            return !source.Any();
        }

        [DebuggerStepThrough]
        public static bool IsSingletonSet<T>(this IEnumerable<T> source)
        {
            Check.NotNull(source, "source");
            return source.Count() == 1;
        } 
    }
}

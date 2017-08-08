using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NetFusion.Common.Extensions.Collection
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Converts a list to a read-only collection.
        /// </summary>
        /// <typeparam name="T">The type contained within the collection.</typeparam>
        /// <param name="source">The list to convert.</param>
        /// <returns>Read-only collection.</returns>
        public static IReadOnlyCollection<T> ToReadOnly<T>(this IList<T> source)
        {
            Check.NotNull(source, nameof(source));

            return new ReadOnlyCollection<T>(source);
        }
    }
}

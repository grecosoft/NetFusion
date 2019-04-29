using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetFusion.Bootstrap.Extensions
{
    /// <summary>
    /// Extension methods for filtering a container's plug-in types and creating instances.
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Provided a list of types, finds the unique set of assemblies containing the types.
        /// </summary>
        /// <param name="types">The types to find the containing assemblies.</param>
        /// <returns>Distinct list of assemblies.</returns>
        public static IEnumerable<Assembly> ContainingAssemblies(this IEnumerable<Type> types)
        {
            if (types == null) throw new ArgumentNullException(nameof(types), 
                "List of types cannot be null.");

            return types.Select(t => t.GetTypeInfo().Assembly)
                .Distinct();
        }
    } 
}

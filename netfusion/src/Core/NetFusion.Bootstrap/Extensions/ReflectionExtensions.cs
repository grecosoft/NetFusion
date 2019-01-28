using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;
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

        /// <summary>
        /// Filters the source list of types to those deriving from a provided base type and 
        /// creates an instance of each type.
        /// </summary>
        /// <param name="pluginTypes">The list of plug-in types to filter.</param>
        /// <param name="baseType">The base used to filter the list of plug-in types.</param>
        /// <returns>Object instances of all plug-in types deriving from the specified base type.</returns>
        public static IEnumerable<object> CreateInstancesDerivingFrom(this IEnumerable<PluginType> pluginTypes, Type baseType)
        {
            if (pluginTypes == null) throw new ArgumentNullException(nameof(pluginTypes), 
                "List of plug-in types cannot be null.");

            if (baseType == null) throw new ArgumentNullException(nameof(baseType), 
                "Base type cannot be null.");

            return pluginTypes.Select(pt => pt.Type).CreateInstancesDerivingFrom(baseType);
        }

        /// <summary>
        /// Filters the source list of types to those deriving from a provided base type and 
        /// creates an instance of each type.
        /// </summary>
        /// <typeparam name="T">The base used to filter the list of plug-in types.</typeparam>
        /// <returns>Object instances of all plug-in types deriving from the specified base type.</returns>
        public static IEnumerable<T> CreateInstancesDerivingFrom<T>(this IEnumerable<PluginType> pluginTypes)
        {
            if (pluginTypes == null) throw new ArgumentNullException(nameof(pluginTypes),
                "List of plug-in types cannot be null.");

            return pluginTypes.Select(pt => pt.Type).CreateInstancesDerivingFrom<T>();
        }
    } 
}

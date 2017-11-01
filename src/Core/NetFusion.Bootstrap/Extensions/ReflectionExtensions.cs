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
            if (types == null) throw new ArgumentNullException(nameof(types), "List of types cannot be null.");

            return types.Select(t => t.GetTypeInfo().Assembly)
                .Distinct();
        }

        /// <summary>
        /// Provided a list of object instances, reduces the list to only instances created 
        /// from a list of specific types.
        /// </summary>
        /// <typeparam name="T">The type of the object instances.</typeparam>
        /// <param name="instances">The list of object instances to filter.</param>
        /// <param name="types">The list of types used to filter the list of
        /// object instances.</param>
        /// <returns>Filter list of instances based on a set of provided types.</returns>
        public static IEnumerable<T> CreatedFrom<T>(this IEnumerable<T> instances,
            IEnumerable<Type> types)
        {
            if (instances == null) throw new ArgumentNullException(nameof(instances), "List of instance object cannot be null.");
            if (types == null) throw new ArgumentNullException(nameof(types), "List of types cannot be null.");
            
            return instances.Where(i => types.Contains(i.GetType())).OfType<T>();
        }

        /// <summary>
        /// Provided a list of object instances, reduces the list to only instances
        /// created from a list of specific plug-in types.
        /// </summary>
        /// <typeparam name="T">The type of the object instances.</typeparam>
        /// <param name="instances">The list of object instances to filter.</param>
        /// <param name="pluginTypes">The list of plug-in types used to filter
        /// the list of object instances.</param>
        /// <returns>Filter list of instances based on a set of provided plug-ins.</returns>
        public static IEnumerable<T> CreatedFrom<T>(this IEnumerable<T> instances,
            IEnumerable<PluginType> pluginTypes)
        {
            if (instances == null) throw new ArgumentNullException(nameof(instances), "List of instance object cannot be null.");
            if (pluginTypes == null) throw new ArgumentNullException(nameof(pluginTypes), "List of plug-in types cannot be null.");

            IEnumerable<Type> types = pluginTypes.Select(pt => pt.Type);
            return instances.CreatedFrom(types);
        }

        /// <summary>
        /// Filters the source list of types to those that are assignable to the 
        /// provided base type and then creates an instance of each type.
        /// </summary>
        /// <param name="pluginTypes">The list of plug-in types to filter.</param>
        /// <param name="baseType">The type or base used to filter the list of plug-in types.</param>
        /// <returns>Object instances of all plug-in types that are assignable to the specified matching type.</returns>
        public static IEnumerable<object> CreateInstancesDerivingFrom(this IEnumerable<PluginType> pluginTypes, Type baseType)
        {
            if (pluginTypes == null) throw new ArgumentNullException(nameof(pluginTypes), "List of plug-in types cannot be null.");
            if (baseType == null) throw new ArgumentNullException(nameof(baseType), "Base type cannot be null.");

            return pluginTypes.Select(pt => pt.Type).CreateInstancesDerivingFrom(baseType);
        }

        /// <summary>
        /// Filters the source list of plug-in types to those that are assignable 
        /// to the provided base type and then creates an instance of each type.
        /// </summary>
        /// <typeparam name="T">The type or base used to filter the list of plug-in types.</typeparam>
        /// <returns>Object instances of all types that are assignable to the
        /// provided filter type.</returns>
        public static IEnumerable<T> CreateInstancesDerivingFrom<T>(this IEnumerable<PluginType> pluginTypes)
        {
            if (pluginTypes == null) throw new ArgumentNullException(nameof(pluginTypes), "List of plug-in types cannot be null.");

            return pluginTypes.Select(pt => pt.Type).CreateInstancesDerivingFrom<T>();
        }
    } 
}

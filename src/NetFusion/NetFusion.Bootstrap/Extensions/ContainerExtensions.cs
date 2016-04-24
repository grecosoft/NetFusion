﻿using NetFusion.Bootstrap.Plugins;
using NetFusion.Common;
using NetFusion.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetFusion.Bootstrap.Extensions
{
    /// <summary>
    /// Extension methods for filtering a container's plug-in types.
    /// </summary>
    public static class ContainerExtensions
    {
        /// <summary>
        /// Provided a list of types, finds the unique set of assemblies 
        /// containing the types.
        /// </summary>
        /// <param name="types">The types to find the containing assemblies.</param>
        /// <returns>Distinct list of assemblies.</returns>
        public static Assembly[] ContainingAssemblies(this IEnumerable<Type> types)
        {
            Check.NotNull(types, nameof(types));

            return types.Select(t => t.Assembly)
                .Distinct().ToArray();
        }

        /// <summary>
        /// Provided a list of object instances, reduces the list to only instances
        /// created from a list of specific types.
        /// </summary>
        /// <typeparam name="T">The type of the object instances.</typeparam>
        /// <param name="instances">The list of object instances to filter.</param>
        /// <param name="types">The list of types used to filter the list of
        /// object instances.</param>
        /// <returns>Filter list of instances based on a set of provided types.</returns>
        public static IEnumerable<T> CreatedFrom<T>(this IEnumerable<T> instances,
            IEnumerable<Type> types)
        {
            Check.NotNull(instances, nameof(instances));
            Check.NotNull(types, nameof(types));

            return instances.Where(i => types.Contains(i.GetType()));
        }

        /// <summary>
        /// Provided a list of object instances, reduces the list to only instances
        /// created from a list of types.
        /// </summary>
        /// <typeparam name="T">The type of the object instances.</typeparam>
        /// <param name="instances">The list of object instances to filter.</param>
        /// <param name="pluginTypes">The list of plug-in types used to filter
        /// the list of object instances.</param>
        /// <returns>Filter list of instances based on a set of provided types.</returns>
        public static IEnumerable<T> CreatedFrom<T>(this IEnumerable<T> instances,
            IEnumerable<PluginType> pluginTypes)
        {
            Check.NotNull(instances, nameof(instances));
            Check.NotNull(pluginTypes, nameof(pluginTypes));

            var types = pluginTypes.Select(pt => pt.Type);
            return instances.CreatedFrom(types);
        }

        /// <summary>
        /// Filters the source list of types to those that are assignable to the 
        /// provided base type and then creates an instance of each type.
        /// </summary>
        /// <param name="pluginTypes">The list of plug-in types to filter.</param>
        /// <param name="matchingType">The type or base used to filter the list of plug-in types.</param>
        /// <returns>Object instances of all plug-in types that are assignable to the specified matching type.</returns>
        public static IEnumerable<object> CreateMatchingInstances(this IEnumerable<PluginType> pluginTypes, Type matchingType)
        {
            var types = pluginTypes.Select(pt => pt.Type);
            foreach (var type in types.Where(t => t.IsCreatableType() && t.IsDerivedFrom(matchingType)))
            {
                yield return type.CreateInstance();
            }
        }

        /// <summary>
        /// Filters the source list of types to those that are assignable to the 
        /// provided base type and then creates an instance of each type.
        /// </summary>
        /// <typeparam name="T">The type or base used to filter the list of plug-in types.</typeparam>
        /// <param name="pluginTypes">The list of plug-in types to filter.</param>
        /// <returns>Object instances of all types that are assignable to the
        /// <returns>Object instances of all plug-in types that are assignable to the specified matching type.</returns>
        public static IEnumerable<T> CreateMatchingInstances<T>(this IEnumerable<Type> types)
        {
            foreach(var type in types.Where(t => t.IsCreatableType() && t.IsDerivedFrom<T>()))
            {
                yield return (T)type.CreateInstance();
            }
        }

        /// <summary>
        /// Filters the source list of plug-in types to those that are assignable 
        /// to the provided base type and then creates an instance of each type.
        /// </summary>
        /// <typeparam name="T">The type or base used to filter the list of plug-in types.</typeparam>
        /// <returns>Object instances of all types that are assignable to the
        /// provided filter type.</returns>
        public static IEnumerable<T> CreateMatchingInstances<T>(this IEnumerable<PluginType> pluginTypes)
        {
            return pluginTypes.Select(pt => pt.Type).CreateMatchingInstances<T>();
        }
    } 
}
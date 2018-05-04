﻿using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using System;
using System.Collections.Generic;

namespace NetFusion.Bootstrap.Dependencies
{
    /// <summary>
    /// Extension methods for IServiceContainer.
    /// </summary>
    public static class ServiceBuilderExtensions
    {
        /// <summary>
        /// Registers a service instance for a sub list of supported interface types.
        /// </summary>
        /// <param name="services">Reference to service collection.</param>
        /// <param name="interfaceTypes">The service interface types to register implementation.</param>
        /// <param name="implementation">The service's implementation.</param>
        /// <returns>Service collection.</returns>
        public static IServiceCollection AddSingleton(this IServiceCollection services, 
            IEnumerable<Type> interfaceTypes, 
            object implementation)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (interfaceTypes == null) throw new ArgumentNullException(nameof(interfaceTypes));
            if (implementation == null) throw new ArgumentNullException(nameof(implementation));

            foreach (Type interfaceType in interfaceTypes)
            {
                services.Add(new ServiceDescriptor(interfaceType, implementation));
            }

            return services;
        }

        /// <summary>
        /// Creates a new type catalog for a given set of plugin types.
        /// </summary>
        /// <param name="services">The service collection delegated to by the created catalog.</param>
        /// <param name="pluginTypes">The plugin-in types added to the catalog to be filtered.</param>
        /// <returns>Type catalog instance.</returns>
        public static ITypeCatalog CreateCatalog(this IServiceCollection services, 
            IEnumerable<PluginType> pluginTypes)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (pluginTypes == null) throw new ArgumentNullException(nameof(pluginTypes));

            return new TypeCatalog(services, pluginTypes);
        }

        /// <summary>
        /// Creates a new type catalog for a given set of plugin types.
        /// </summary>
        /// <param name="services">The service collection delegated to by the created catalog.</param>
        /// <param name="pluginTypes">The plugin-in types added to the catalog to be filtered.</param>
        /// <returns>Type catalog instance.</returns>
        public static ITypeCatalog CreateCatalog(this IServiceCollection services,
            IEnumerable<Type> pluginTypes)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (pluginTypes == null) throw new ArgumentNullException(nameof(pluginTypes));

            return new TypeCatalog(services, pluginTypes);
        }
    }
}

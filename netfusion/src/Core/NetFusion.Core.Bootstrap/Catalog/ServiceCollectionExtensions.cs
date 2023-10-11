using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace NetFusion.Core.Bootstrap.Catalog;

/// <summary>
/// Extension methods for IServiceContainer.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Creates a new type catalog for a given set of types.
    /// </summary>
    /// <param name="services">The service collection delegated to by the created catalog.</param>
    /// <param name="pluginTypes">The types added to the catalog to be filtered.</param>
    /// <returns>Type catalog instance.</returns>
    public static ITypeCatalog CreateCatalog(this IServiceCollection services,
        IEnumerable<Type> pluginTypes)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (pluginTypes == null) throw new ArgumentNullException(nameof(pluginTypes));

        return new TypeCatalog(services, pluginTypes);
    }
    
    /// <summary>
    /// Registers a service instance for a list of supported contract types.
    /// </summary>
    /// <param name="services">Reference to service collection.</param>
    /// <param name="contractTypes">The service interface types to register implementation.</param>
    /// <param name="implementation">The service's implementation.</param>
    /// <returns>Service collection.</returns>
    public static IServiceCollection AddSingleton(this IServiceCollection services, 
        IEnumerable<Type> contractTypes, 
        object implementation)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (contractTypes == null) throw new ArgumentNullException(nameof(contractTypes));
        if (implementation == null) throw new ArgumentNullException(nameof(implementation));

        foreach (Type interfaceType in contractTypes)
        {
            services.Add(new ServiceDescriptor(interfaceType, implementation));
        }

        return services;
    }
}
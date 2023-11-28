using System;
using Microsoft.Extensions.DependencyInjection;

namespace NetFusion.Core.Bootstrap.Catalog;

/// <summary>
/// Allows types matching a provided filter to be registered with the Microsoft's service collection.
/// A reference to this interface is passed to plug-in modules and used to scan for types to be added
/// to the service-collection during the bootstrap process.
/// </summary>
public interface ITypeCatalog
{
    /// <summary>
    /// Registers the types matching the provided filter as the specified service type.
    /// When the service type is resolved, the collection of matching service implementations
    /// are returned. 
    /// </summary>
    /// <typeparam name="TService">The service registration type.  All concrete classes
    /// matching the supplied filter will be registered as TService.</typeparam>
    /// <param name="filter">The predicate used to find matching types.</param>
    /// <param name="lifetime">The lifetime of the registered service.</param>
    /// <returns>Reference to type catalog.</returns>
    ITypeCatalog AsService<TService>(Func<Type, bool> filter, ServiceLifetime lifetime);

    /// <summary>
    /// Registers the types matching the provided filter as both the service and implementation types.
    /// </summary>
    /// <param name="filter">The predicate used to find matching types.</param>
    /// <param name="lifetime">The lifetime of the registered service.</param>
    /// <returns>Reference to the type catalog.</returns>
    ITypeCatalog AsSelf(Func<Type, bool> filter, ServiceLifetime lifetime);

    /// <summary>
    /// Registers the types matching the provided filter as a provided service descriptor.
    /// </summary>
    /// <param name="filter">The predicate used to find matching types.</param>
    /// <param name="describedBy">Delegate passed the matching type and returns
    /// the corresponding descriptor to be added to the service collection.</param>
    /// <returns>Reference to the type catalog.</returns>
    ITypeCatalog AsDescriptor(Func<Type, bool> filter, Func<Type, ServiceDescriptor> describedBy);

    /// <summary>
    /// Registers the service implementation types matching the provided filter as an interface
    /// matching a convention.  An exception is raised if the service implementation type does
    /// not implement one and only one service interface having the name of the implementation
    /// type prefixed with the character "I".
    /// <example>
    ///     class ContactRepository : IContactRepository
    ///     {
    ///     }
    /// </example>
    /// </summary>
    /// <param name="filter">The predicate used to find matching types.</param>
    /// <param name="lifetime">The lifetime of the registered service.</param>
    /// <returns>Reference to the type catalog.</returns>
    ITypeCatalog AsImplementedInterface(Func<Type, bool> filter, ServiceLifetime lifetime);

    /// <summary>
    /// Registers the service implementation types having a name ending in a suffix as an interface
    /// matching a convention.  An exception is raised if the service implementation type does
    /// not implement one and only one service interface having the name of the implementation
    /// type prefixed with the character "I".
    /// </summary>
    /// <param name="typeSuffix">The suffix type name used to find matching types.</param>
    /// <param name="lifetime">The lifetime of the registered service.</param>
    /// <returns>Reference to the type catalog.</returns>
    ITypeCatalog AsImplementedInterface(string typeSuffix, ServiceLifetime lifetime);
}
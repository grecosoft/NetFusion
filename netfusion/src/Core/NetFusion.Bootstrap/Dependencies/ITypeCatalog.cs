using Microsoft.Extensions.DependencyInjection;
using System;

namespace NetFusion.Bootstrap.Dependencies
{
    /// <summary>
    /// Allows plug-in types matching a provided filter to be registered with the Microsoft's 
    /// service collection.  A reference to this interface is passed to plug-in modules and 
    /// used to scan for types to be added to the service collection during the application
    /// container's bootstrap process.
    /// </summary>
    public interface ITypeCatalog
    {
        /// <summary>
        /// Registers the types matching the provided filter as the specified service type.
        /// </summary>
        /// <typeparam name="TService">The service registration type.</typeparam>
        /// <param name="filter">The predicate used find matching types.</param>
        /// <param name="lifetime">The lifetime of the registered service.</param>
        /// <returns>Reference to type catalog.</returns>
        ITypeCatalog AsService<TService>(Func<Type, bool> filter, ServiceLifetime lifetime);

        /// <summary>
        /// Registers the types matching the provided filter as the service and implementation types.
        /// </summary>
        /// <param name="filter">The predicated used to find matching types.</param>
        /// <param name="lifetime">The lifetime of the registered service.</param>
        /// <returns>Reference to the type catalog.</returns>
        ITypeCatalog AsSelf(Func<Type, bool> filter, ServiceLifetime lifetime);

        /// <summary>
        /// Registers the types matching the provided filters as a provided service descriptor.
        /// </summary>
        /// <param name="filter">The predicate used to find matching types.</param>
        /// <param name="describedBy">Delegate passed the matching type and returned
        /// the corresponding descriptor to be added to the service collection.</param>
        /// <returns>Reference to the type catalog.</returns>
        ITypeCatalog AsDescriptor(Func<Type, bool> filter, Func<Type, ServiceDescriptor> describedBy);

        /// <summary>
        /// Registers the service implementation types matching the provided filter as an interface
        /// matching a convention.  An exception is raised if the service implementation type supports
        /// zero interfaces.  If multiple interfaces are supported, one is selected starting with the 
        /// name of the service implementation type prefixed with "I".  If a single service interface
        /// type cannot be determined, an exception is thrown. 
        /// </summary>
        /// <param name="filter">The predicate used to find matching types.</param>
        /// <param name="lifetime">The lifetime of the registered service.</param>
        /// <returns>Reference to the type catalog.</returns>
        ITypeCatalog AsImplementedInterface(Func<Type, bool> filter, ServiceLifetime lifetime);

        /// <summary>
        /// Registers the service implementation types having a name ending in a suffix as an interface
        /// matching a convention.  An exception is raised if the service implementation type supports
        /// zero interfaces.  If multiple interfaces are supported, one is selected starting with the 
        /// name of the service implementation type prefixed with "I".  If a single service interface
        /// type cannot be determined, an exception is thrown. 
        /// </summary>
        /// <param name="typeSuffix">The suffix type name used to find matching types.</param>
        /// <param name="lifetime">The lifetime of the registered service.</param>
        /// <returns>Reference to the type catalog.</returns>
        ITypeCatalog AsImplementedInterface(string typeSuffix, ServiceLifetime lifetime);
    }
}

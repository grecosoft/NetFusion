using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetFusion.Common.Extensions.Reflection;

namespace NetFusion.Bootstrap.Catalog
{
    /// <summary>
    /// Constructed with a list of types and provides methods for filtering the list
    /// for types to be added to the service collection.
    /// </summary>
    public class TypeCatalog : ITypeCatalog
    {
        public IServiceCollection Services { get; }
        private readonly Type[] _types;

        public TypeCatalog(IServiceCollection serviceCollection, IEnumerable<Type> types)
        {
            Services = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
            _types = GetNonAbstractTypes(types ?? throw new ArgumentNullException(nameof(types)));
        }
        
        public TypeCatalog(IServiceCollection serviceCollection, params Type[] types)
        {
            Services = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
            _types = GetNonAbstractTypes(types ?? throw new ArgumentNullException(nameof(types)));
        }

        private static Type[] GetNonAbstractTypes(IEnumerable<Type> types) => types.Where(t => !t.IsAbstract).ToArray();
       
        public ITypeCatalog AsService<TService>(Func<Type, bool> filter, ServiceLifetime lifetime)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            foreach (Type matchingType in _types.Where(filter))
            {
                AddDescriptor(new ServiceDescriptor(typeof(TService), matchingType, lifetime));
            }
            return this;
        }

        public ITypeCatalog AsSelf(Func<Type, bool> filter, ServiceLifetime lifetime)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            foreach (Type matchingType in _types.Where(filter))
            {
                AddDescriptor(new ServiceDescriptor(matchingType, matchingType, lifetime));
            }
            return this;
        }

        public ITypeCatalog AsDescriptor(Func<Type, bool> filter,
            Func<Type, ServiceDescriptor> describedBy)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            if (describedBy == null) throw new ArgumentNullException(nameof(describedBy));
       
            foreach (Type matchingType in _types.Where(filter))
            {
                AddDescriptor(describedBy(matchingType));
            }
            return this;
        }

        public ITypeCatalog AsImplementedInterface(Func<Type, bool> filter, ServiceLifetime lifetime)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            foreach (Type matchingType in _types.Where(filter))
            {
                Type serviceType = GetServiceInterface(matchingType);
                AddDescriptor(new ServiceDescriptor(serviceType, matchingType, lifetime));
            }

            return this;
        }

        public ITypeCatalog AsImplementedInterface(string typeSuffix, ServiceLifetime lifetime)
        {
            if (string.IsNullOrWhiteSpace(typeSuffix))
                throw new ArgumentException("Type suffix not specified.");

            return AsImplementedInterface(
                t => t.Name.EndsWith(typeSuffix, StringComparison.Ordinal),
                lifetime);
        }

        // Validates that the implementation type or instance is assignable to the service type
        // before adding the service to the collection.
        private void AddDescriptor(ServiceDescriptor descriptor)
        {
            if (! descriptor.ImplementationInstance?.GetType().CanAssignTo(descriptor.ServiceType) ?? false)
            {
                throw new InvalidCastException(
                    $"Implementation Instance: {descriptor.ImplementationInstance?.GetType()} not assignable to " + 
                    $"Service Type: {descriptor.ServiceType}");
            }
            
            if (! descriptor.ImplementationType?.CanAssignTo(descriptor.ServiceType) ?? false)
            {
                throw new InvalidCastException(
                    $"Implementation Type: {descriptor.ImplementationType} not assignable to " + 
                    $"Service Type: {descriptor.ServiceType}");
            }

            Services.Add(descriptor);
        }
        
        private static Type GetServiceInterface(Type implementationType)
        {
            string serviceName = $"I{implementationType.Name}";
            
            Type[] serviceInterfaces = implementationType.GetInterfaces()
                .Where(t => t.Name.Equals(serviceName, StringComparison.Ordinal))
                .ToArray();
            
            if (serviceInterfaces.Length != 1)
            {
                throw new InvalidOperationException(
                    $"The implementation type: {implementationType} does not implement one and " + 
                    $"only one interface named: {serviceName}");
            }

            return serviceInterfaces.First();
        }
    }
}

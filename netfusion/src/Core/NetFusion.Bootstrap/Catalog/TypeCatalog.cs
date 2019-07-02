using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace NetFusion.Bootstrap.Catalog
{
    /// <summary>
    /// Constructed with a list of types and provides methods for filtering the list
    /// for types to be added to the service collection.
    /// </summary>
    public class TypeCatalog : ITypeCatalog
    {
        public IServiceCollection Services { get; }
        private readonly IEnumerable<Type> _types;

        public TypeCatalog(IServiceCollection serviceCollection, IEnumerable<Type> types)
        {
            Services = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
            _types = types ?? throw new ArgumentNullException(nameof(types));
        }
        
        public TypeCatalog(IServiceCollection serviceCollection, params Type[] types)
        {
            Services = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
            _types = types ?? throw new ArgumentNullException(nameof(types));
        }
       
        public ITypeCatalog AsService<TService>(Func<Type, bool> filter, ServiceLifetime lifetime)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            foreach (Type matchingType in _types.Where(filter))
            {
                Services.Add(new ServiceDescriptor(typeof(TService), matchingType, lifetime));
            }
            return this;
        }

        public ITypeCatalog AsSelf(Func<Type, bool> filter, ServiceLifetime lifetime)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            foreach (Type matchingType in _types.Where(filter))
            {
                Services.Add(new ServiceDescriptor(matchingType, matchingType, lifetime));
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
                Services.Add(describedBy(matchingType));
            }
            return this;
        }

        public ITypeCatalog AsImplementedInterface(Func<Type, bool> filter, ServiceLifetime lifetime)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            foreach (Type matchingType in _types.Where(filter).Where(t => !t.IsAbstract))
            {
                Type serviceType = GetServiceInterface(matchingType);
                Services.Add(new ServiceDescriptor(serviceType, matchingType, lifetime));
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

        private static Type GetServiceInterface(Type serviceType)
        {
            Type[] serviceInterfaces = serviceType.GetInterfaces();
            if (! serviceInterfaces.Any())
            {
                throw new InvalidOperationException(
                    $"Interface for service type {serviceType} could not be determined." + 
                    "Service does not implement any interfaces.");
            }

            // If there is only one supported interface, use this as the service interface.
            if (serviceInterfaces.Length == 1)
            {
                return serviceInterfaces.First();
            }

            // If there are more than on supported interface, select one based on conventions.
            if (serviceInterfaces.Length >= 1)
            {
                Type[] serviceInterfacesByConvention = serviceInterfaces.Where(
                    t => t.Name.StartsWith("I" + serviceType.Name, StringComparison.Ordinal)).ToArray();

                if (serviceInterfacesByConvention.Length == 1)
                {
                    return serviceInterfacesByConvention.First();
                }
            }

            throw new InvalidOperationException(
                $"Interface for service type {serviceType} could not be determined." +
                "Service implements more than one possible interface.");
        }
    }
}

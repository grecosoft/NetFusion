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

            foreach (Type matchingType in _types.Where(filter))
            {
                Type serviceType = GetServiceInterface(matchingType);
                if (serviceType != null)
                {
                    Services.Add(new ServiceDescriptor(serviceType, matchingType, lifetime));
                }
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
                return null;
            }

            Type[] serviceInterfacesByConvention = serviceInterfaces.Where(
                t => t.Name.Equals("I" + serviceType.Name, StringComparison.Ordinal)).ToArray();

            if (serviceInterfacesByConvention.Length == 1)
            {
                return serviceInterfacesByConvention.First();
            }
        
            return null;
        }
    }
}

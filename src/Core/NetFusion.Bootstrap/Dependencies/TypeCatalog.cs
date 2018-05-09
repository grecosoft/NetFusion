using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Bootstrap.Dependencies
{
    /// <summary>
    /// Constructed with a list of types and provides methods for filtering the list
    /// for types to be added to the service collection.
    /// </summary>
    public class TypeCatalog : ITypeCatalog
    {
        private readonly IServiceCollection _serviceCollection;
        private readonly IEnumerable<Type> _types;

        public TypeCatalog(IServiceCollection serviceCollection, IEnumerable<Type> types)
        {
            _serviceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
            _types = types ?? throw new ArgumentNullException(nameof(types));
        }

        public TypeCatalog(IServiceCollection serviceCollection, IEnumerable<PluginType> types) :
            this(serviceCollection, types?.Select(pt => pt.Type))
        {

        }

        public ITypeCatalog AsService<TService>(Func<Type, bool> filter, ServiceLifetime lifetime)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            foreach (Type matchingType in _types.Where(filter))
            {
                _serviceCollection.Add(new ServiceDescriptor(typeof(TService), matchingType, lifetime));
            }
            return this;
        }

        public ITypeCatalog AsSelf(Func<Type, bool> filter, ServiceLifetime lifetime)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            foreach (Type matchingType in _types.Where(filter))
            {
                _serviceCollection.Add(new ServiceDescriptor(matchingType, matchingType, lifetime));
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
                _serviceCollection.Add(describedBy(matchingType));
            }
            return this;
        }

        public ITypeCatalog AsImplementedInterfaces(Func<Type, bool> filter, ServiceLifetime lifetime)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));

            foreach (Type matchingType in _types.Where(filter))
            {
                foreach (Type serviceType in matchingType.GetInterfaces())
                {
                    _serviceCollection.Add(new ServiceDescriptor(serviceType, matchingType, lifetime));
                }
            }

            return this;
        }

        public ITypeCatalog AsImplementedInterfaces(string typeSuffix, ServiceLifetime lifetime)
        {
            if (string.IsNullOrWhiteSpace(typeSuffix))
                throw new ArgumentException("Type suffix not specified.");

            return AsImplementedInterfaces(
                t => t.Name.EndsWith(typeSuffix, StringComparison.Ordinal),
                lifetime);
        }
    }
}

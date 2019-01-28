using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.EntityFramework.Internal;
using NetFusion.EntityFramework.Settings;

namespace NetFusion.EntityFramework.Modules
{
    /// <summary>
    /// Plugin module that discovers all EntityFramework derived DbContext classes
    /// and automatically configure them in the service-container for injection into
    /// application specific components such as repositories.
    /// </summary>
    public class EntityContextModule : PluginModule
    {
        // Discovered Properties:
        private IEnumerable<IEntityTypeMapping> EntityMappings { get; set; }
        
        private EntityDbRegistration[] _registrations;
        private Dictionary<Type, IEntityTypeMapping[]> _contextMappings;
        
        public override void Initialize()
        {
            // Finds all context derived types:
            _registrations = Context.AllPluginTypes
                .Where(EntityDbRegistration.IsEntityDbContextType)
                .Select(ct => new EntityDbRegistration(ct, GetContextMappings(ct)))
                .ToArray();

            _contextMappings = _registrations.ToDictionary(r => r.ImplementationType, r => r.Mappings);
        }

        // Adds each found derived EntityDbContext to the service-collection using
        // the determined service type under which it should be registered.
        public override void RegisterServices(IServiceCollection services)
        {
            foreach (EntityDbRegistration registration in _registrations)
            {
                // Register the context's service type and a factory
                // method that will create and configure the context
                // instance when injected into a dependent component.
                services.AddScoped(registration.ServiceType, sp =>
                {
                    var connSettings = sp.GetService<ConnectionSettings>();
                    if (connSettings == null)
                    {
                        throw new ContainerException(
                            "Application settings for: netfusion:entityFramework not found.");
                    }

                    DbContextSettings contextSettings = GetSettingsForContext(connSettings, registration);
                    IEntityTypeMapping[] contextMappings = _contextMappings[registration.ImplementationType];
                    
                    // Return instance of a configured context:
                    return registration.ImplementationType.CreateInstance(
                        contextSettings,
                        contextMappings);
                });
            }
        }

        private static DbContextSettings GetSettingsForContext(ConnectionSettings connSettings, EntityDbRegistration registration)
        {
            string contextName = registration.ImplementationType.Name;
                    
            var contextSettings = connSettings.Contexts.FirstOrDefault(
                cs => cs.ContextName == contextName);

            if (contextSettings == null)
            {
                throw new ContainerException(
                    $"Connection settings not found for database context type: {contextName}");
            }

            return contextSettings;
        }
        
        // Returns only the entity type mappings within the same or child namespace as the context.
        private IEntityTypeMapping[] GetContextMappings(Type contextType)
        {
            if (contextType.Namespace == null)
            {
                return new IEntityTypeMapping[] {};   
            }

            return EntityMappings.Select(map => new
                {
                    mappingNs = map.GetType().Namespace,
                    mapping = map
                })
                .Where(entityMap =>
                    entityMap.mappingNs != null && entityMap.mappingNs.StartsWith(contextType.Namespace, StringComparison.Ordinal))
                .Select(entityMap => entityMap.mapping)
                .ToArray();
        }
    }
}
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
    /// Plugin module that will discover all EntityFramework derived DbContext classes
    /// and automatically configure them in the service-container for injection into
    /// application specific components such as repositories.
    /// </summary>
    public class EntityContextModule : PluginModule
    {
        // Discovered Properties:
        private IEnumerable<IEntityTypeMapping> EntityMappings { get; set; }
        
        private EntityDbRegistration[] _registrations;
        
        public override void Configure()
        {
            // Finds all types meeting the criteria of an EntityFramework 
            // derived context type that can be automatically configured.
            _registrations = Context.AllPluginTypes
                .Where(EntityDbRegistration.IsEntityDbType)
                .Select(pt => new EntityDbRegistration(pt)).ToArray();
        }

        // Adds each found derived DbContext to the service-collection using
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
                    // Find the connections settings for the context:
                    var connSettings = sp.GetService<ConnectionSettings>();
                    if (connSettings == null)
                    {
                        throw new ContainerException(
                            "Application settings for: netfusion:entityFramework not found.");
                    }
                    
                    string contextName = registration.ImplementationType.Name;
                    
                    var contextSettings = connSettings.Contexts.FirstOrDefault(
                        cs => cs.ContextName == contextName);

                    if (contextSettings == null)
                    {
                        throw new ContainerException(
                            $"Connection settings not found for database context type: {contextName}");
                    }

                    // Return instance of a configured context:
                    IEntityTypeMapping[] contextMappings = GetContextMappings(registration.ImplementationType);
                    
                    return registration.ImplementationType.CreateInstance(
                        contextSettings.ConnectionString,
                        contextMappings);
                });
            }
        }
        
        // Returns only the entity type mappings within the same or child namespace as the context.
        private IEntityTypeMapping[] GetContextMappings(Type contextType)
        {
            return EntityMappings.Where(map => 
                    map.GetType().Namespace.StartsWith(contextType?.Namespace ?? "", StringComparison.Ordinal))
                .ToArray();
        }
    }
}
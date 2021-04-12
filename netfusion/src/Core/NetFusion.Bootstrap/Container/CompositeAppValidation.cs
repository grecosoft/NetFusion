using NetFusion.Bootstrap.Exceptions;
using NetFusion.Common.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Validates the state of the  registered plugins before being
    /// used to build the composite-application.
    /// </summary>
    internal class CompositeAppValidation
    {
        private readonly IPlugin[] _plugins;

        public CompositeAppValidation(IPlugin[] plugins)
        {
            _plugins = plugins ?? throw new ArgumentNullException(nameof(plugins));
        }

        public void Validate()
        {
            AssertPluginIdentity();
            AssertPluginMetadata();
            AssertPluginTypes();
            AssertPluginConfigs();
        }

        private void AssertPluginIdentity()
        {
            IEnumerable<Type> invalidPluginTypes = _plugins.Where(p => string.IsNullOrWhiteSpace(p.PluginId))
                .Select(p => p.GetType())
                .ToArray();

            if (invalidPluginTypes.Any())
            {
                throw new ContainerException(
                    "All plugins must have a PluginId specified.  See details for invalid plugin types.",
                    "MissingPluginIds", invalidPluginTypes);
            }

            IEnumerable<Type> duplicatePluginIds = _plugins.WhereDuplicated(p => p.PluginId)
                .Select(p => p.GetType())
                .ToArray();

            if (duplicatePluginIds.Any())
            {
                throw new ContainerException(
                    "Plug-in identity values must be unique.  See details for invalid plugin types.",
                    "DuplicatePluginIds", duplicatePluginIds);
            }
        }

        private void AssertPluginMetadata()
        {
            IEnumerable<Type> invalidPluginTypes = _plugins.Where(p => 
                    string.IsNullOrWhiteSpace(p.AssemblyName) || string.IsNullOrWhiteSpace(p.Name))
                .Select(p => p.GetType())
                .ToArray();

            if (invalidPluginTypes.Any())
            {
                throw new ContainerException(
                    "All plugins must have AssemblyName and Name values.  See details for invalid plugin types.", 
                    "InvalidPluginTypes", invalidPluginTypes);
            }
        }

        private void AssertPluginTypes()
        {
            var hostPluginTypes = _plugins.Where(p => p.PluginType == PluginTypes.HostPlugin)
                .Select(p => p.GetType()).ToArray();
            
            if (hostPluginTypes.Empty())
            {
                throw new ContainerException(
                    "The composite application must have one host plugin type.");
            }

            if (hostPluginTypes.Length > 1)
            {
                throw new ContainerException(
                    "There can only be one host plugin.  See details for invalid plugin types.", 
                    "HostPluginTypes", hostPluginTypes);
            }
        }

        private void AssertPluginConfigs()
        {
            var duplicateConfigTypes = _plugins.SelectMany(p => p.Configs)
                .WhereDuplicated(c => c.GetType())
                .ToArray();

            if (duplicateConfigTypes.Any())
            {
                throw new ContainerException(
                    "A plugin configuration can only be registered once. See details for invalid configuration types", 
                    "DuplicateConfigTypes", duplicateConfigTypes);
            }
        }
    }
}

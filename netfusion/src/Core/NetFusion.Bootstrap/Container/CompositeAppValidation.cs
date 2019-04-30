using NetFusion.Bootstrap.Exceptions;
using NetFusion.Common.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Validates that the manifest registry was correctly constructed from
    /// the discovered assemblies representing plug-ins. 
    /// </summary>
    public class CompositeAppValidation
    {
        private readonly CompositeApp _compositeApp;

        public CompositeAppValidation(CompositeApp compositeApp)
        {
            _compositeApp = compositeApp ?? throw new ArgumentNullException(nameof(compositeApp));
        }

        public void Validate()
        {
            AssertPluginIdentity();
            AssertPluginMetadata();
            AssertPluginTypes();
        }

        private void AssertPluginIdentity()
        {
            IEnumerable<Type> invalidPluginTypes = _compositeApp.AllPlugins
                .Where(p => string.IsNullOrWhiteSpace(p.PluginId))
                .Select(p => p.GetType())
                .ToArray();

            if (invalidPluginTypes.Any())
            {
                throw new ContainerException(
                    "All plugins must have a PluginId specified.  See details for invalid plugin types.",
                    "MissingPluginIds", invalidPluginTypes);
            }

            IEnumerable<string> duplicatePluginIds = _compositeApp.AllPlugins
                .WhereDuplicated(p => p.PluginId)
                .ToArray();

            if (duplicatePluginIds.Any())
            {
                throw new ContainerException(
                    "Plug-in identity values must be unique.  See details for duplicated Plug-in Ids.",
                     "DuplicatePluginIds", duplicatePluginIds);
            }
        }

        private void AssertPluginMetadata()
        {
            IEnumerable<Type> invalidPluginTypes = _compositeApp.AllPlugins
                .Where(p => string.IsNullOrWhiteSpace(p.AssemblyName) || string.IsNullOrWhiteSpace(p.Name))
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
            var hostPluginTypes = _compositeApp.AllPlugins
                .Where(p => p.PluginType == PluginTypes.HostPlugin)
                .Select(p => p.GetType()).ToArray();
            
            if (hostPluginTypes.Empty())
            {
                throw new ContainerException(
                    "The composite application must have one host plugin type.");
            }

            if (hostPluginTypes.Length > 1)
            {
                throw new ContainerException("There can only be one host plugin.", 
                    "HostPluginTypes", hostPluginTypes);
            }
        }
    }
}

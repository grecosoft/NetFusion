using NetFusion.Bootstrap.Exceptions;
using NetFusion.Common.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Validates that the manifest registry was correctly constructed from
    /// the discovered assemblies representing plug-ins. 
    /// </summary>
    public class CompositeAppValidation
    {
        private readonly CompositeApp _registry;

        public CompositeAppValidation(CompositeApp registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry),
                "Manifest Registry cannot be null.");
        }

        public void Validate()
        {
            AssertManifestIds();
            AssertManifestNames();
            AssertLoadedManifests();
        }

        private void AssertManifestIds()
        {
            IEnumerable<Type> invalidManifestTypes = _registry.AllPlugins
                .Where(m => string.IsNullOrWhiteSpace(m.PluginId))
                .Select(m => m.GetType())
                .ToArray();

            if (invalidManifestTypes.Any())
            {
                throw new ContainerException(
                    "All manifest instances must have a PluginId specified.  See details for invalid manifest types.",
                    "MissingPluginIds", invalidManifestTypes);
            }

            IEnumerable<string> duplicateManifestIds = _registry.AllPlugins
                .WhereDuplicated(m => m.PluginId)
                .ToArray();

            if (duplicateManifestIds.Any())
            {
                throw new ContainerException(
                    "Plug-in identity values must be unique.  See details for duplicated Plug-in Ids.",
                     "DuplicateManifestIds", duplicateManifestIds);
            }
        }

        private void AssertManifestNames()
        {
            IEnumerable<Type> invalidManifestTypes = _registry.AllPlugins
                .Where(m => string.IsNullOrWhiteSpace(m.AssemblyName) || string.IsNullOrWhiteSpace(m.Name))
                .Select(m => m.GetType())
                .ToArray();

            if (invalidManifestTypes.Any())
            {
                throw new ContainerException(
                    "All manifest instances must have AssemblyName and Name values.  See details for invalid manifest types.", 
                    "InvalidManifestTypes", invalidManifestTypes);
            }

            IEnumerable<string> duplicateNames = _registry.AllPlugins
                .WhereDuplicated(m => m.Name)
                .ToArray();

            if (duplicateNames.Any())
            {
                throw new ContainerException(
                    "Plug-in names must be unique.  See details for duplicated Plug-in names.",
                    "DuplicateNames", duplicateNames);
            }
        }

        private void AssertLoadedManifests()
        {
            if (_registry.HostPlugin == null)
            {
                throw new ContainerException(
                    $"A Host Application Plug-In manifest could not be found");
            }
        }
    }
}

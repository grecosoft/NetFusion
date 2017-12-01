using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Manifests;
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
    public class ManifestValidation
    {
        private readonly ManifestRegistry _registry;

        public ManifestValidation(ManifestRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry),
                "Manifest Registry cannot be null.");
        }

        public void Validate()
        {
            if (_registry.AllManifests == null)
            {
                throw new ContainerException("Plug-in manifests not set by type resolver.");
            }

            AssertManifestIds();
            AssertManifestNames();
            AssertLoadedManifests();
        }

        private void AssertManifestIds()
        {
            IEnumerable<Type> invalidManifestTypes = _registry.AllManifests
                .Where(m => string.IsNullOrWhiteSpace(m.PluginId))
                .Select(m => m.GetType());

            if (invalidManifestTypes.Any())
            {
                throw new ContainerException(
                    "All manifest instances must have a PluginId specified.  " +
                    "See details for invalid manifest types.", invalidManifestTypes);
            }

            IEnumerable<string> duplicateManifestIds = _registry.AllManifests
                .WhereDuplicated(m => m.PluginId);

            if (duplicateManifestIds.Any())
            {
                throw new ContainerException(
                    "Plug-in identity values must be unique.  See details for duplicated Plug-in Ids.",
                    duplicateManifestIds);
            }
        }

        private void AssertManifestNames()
        {
            IEnumerable<Type> invalidManifestTypes = _registry.AllManifests
                .Where(m => string.IsNullOrWhiteSpace(m.AssemblyName) || string.IsNullOrWhiteSpace(m.Name))
                .Select(m => m.GetType());

            if (invalidManifestTypes.Any())
            {
                throw new ContainerException(
                    "All manifest instances must have AssemblyName and Name values.  " +
                    "See details for invalid manifest types.", invalidManifestTypes);
            }

            IEnumerable<string> duplicateNames = _registry.AllManifests.WhereDuplicated(m => m.Name);

            if (duplicateNames.Any())
            {
                throw new ContainerException(
                    "Plug-in names must be unique.  See details for duplicated Plug-in names.",
                    duplicateNames);
            }
        }

        private void AssertLoadedManifests()
        {
            if (_registry.AppHostPluginManifests.Empty())
            {
                throw new ContainerException(
                    $"A Host Application Plug-In manifest could not be found " +
                    $"derived from: {typeof(IAppHostPluginManifest)}");
            }

            if (!_registry.AppHostPluginManifests.IsSingletonSet())
            {
                throw new ContainerException(
                    "More than one Host Application Plug-In manifest was found.",
                    _registry.AppHostPluginManifests.Select(am => new
                    {
                        ManifestType = am.GetType().FullName,
                        am.AssemblyName,
                        PluginName = am.Name,
                        am.PluginId
                    }));
            }
        }
    }
}

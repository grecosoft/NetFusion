using System;
using System.Collections.Generic;
using System.Linq;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Core.Bootstrap.Exceptions;
using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Core.Bootstrap.Container;

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
    }

    private void AssertPluginIdentity()
    {
        IEnumerable<Type> invalidPluginTypes = _plugins.Where(p => string.IsNullOrWhiteSpace(p.PluginId))
            .Select(p => p.GetType())
            .ToArray();

        if (invalidPluginTypes.Any())
        {
            throw new BootstrapException(
                "All plugins must have a PluginId specified.  See details for invalid plugin types.",
                "MissingPluginIds", invalidPluginTypes, "bootstrap-missing-plugin-id");
        }

        IEnumerable<Type> duplicatePluginIds = _plugins.WhereDuplicated(p => p.PluginId)
            .Select(p => p.GetType())
            .ToArray();

        if (duplicatePluginIds.Any())
        {
            throw new BootstrapException(
                "Plug-in identity values must be unique.  See details for invalid plugin types.",
                "DuplicatePluginIds", duplicatePluginIds, "bootstrap-duplicate-plugin-id");
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
            throw new BootstrapException(
                "All plugins must have AssemblyName and Name values.  See details for invalid plugin types.", 
                "InvalidPluginTypes", invalidPluginTypes, "bootstrap-missing-metadata");
        }
    }

    private void AssertPluginTypes()
    {
        var hostPluginTypes = _plugins.Where(p => p.PluginType == PluginTypes.HostPlugin)
            .Select(p => p.GetType()).ToArray();
            
        if (hostPluginTypes.Empty())
        {
            throw new BootstrapException("The composite application must have one host plugin type.",
                "bootstrap-missing-host-plugin");
        }

        if (hostPluginTypes.Length > 1)
        {
            throw new BootstrapException(
                "There can only be one host plugin.  See details for invalid plugin types.", 
                "HostPluginTypes", hostPluginTypes, "bootstrap-multiple-host-plugins");
        }
    }
}
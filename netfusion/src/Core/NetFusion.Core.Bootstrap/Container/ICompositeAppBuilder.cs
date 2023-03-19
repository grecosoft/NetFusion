using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.Bootstrap.Logging;
using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Core.Bootstrap.Container;

/// <summary>
/// Interface containing the information from which the composite-application is built.
/// </summary>
public interface ICompositeAppBuilder
{
    /// <summary>
    /// The plugin representing the host application.
    /// </summary>
    IPlugin HostPlugin { get; }

    /// <summary>
    /// List of all the application specific plugins pertaining to the application.
    /// </summary>
    IPlugin[] AppPlugins { get; }

    /// <summary>
    /// List of all non-application specific plugins containing core reusable implementations.
    /// </summary>
    IPlugin[] CorePlugins { get; }
        
    /// <summary>
    /// List of all plugins.
    /// </summary>
    IPlugin[] AllPlugins { get; }

    /// <summary>
    /// List of modules associated with all the plugins.
    /// </summary>
    IPluginModule[] AllModules { get; }

    /// <summary>
    /// Returns types associated with a specific category of plugin.
    /// </summary>
    /// <param name="pluginTypes">The category of plugins to limit the return types.</param>
    /// <returns>List of limited plugin types or all plugin types if no category is specified.</returns>
    IEnumerable<Type> GetPluginTypes(params PluginTypes[] pluginTypes);

    /// <summary>
    /// The .net configuration service for reading application configurations.
    /// </summary>
    IConfiguration Configuration { get; }

    /// <summary>
    /// The .net service collection populated by plugin modules.
    /// </summary>
    IServiceCollection ServiceCollection { get; }

    /// <summary>
    /// Reference to a log showing how the application was composed from plugins.
    /// </summary>
    CompositeAppLogger CompositeLog { get; }
}
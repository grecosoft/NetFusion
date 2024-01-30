using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Core.Bootstrap.Container;

namespace NetFusion.Core.Bootstrap.Plugins;

/// <summary>
/// Class containing information that can be used by a given module
/// when the application container is bootstrapped.
/// </summary>
public class ModuleContext
{
    /// <summary>
    /// The plug-in representing the application host.
    /// </summary>
    public IPlugin AppHost { get; }

    /// <summary>
    /// The plugin where the module is defined.
    /// </summary>
    public IPlugin Plugin { get; }
        
    /// <summary>
    /// The plugin types that can be accessed by the module limited to the set based on its type of plugin.  
    /// This list will contain all types from all plug-ins if the context is associated with a core plugin.
    /// However, for application centric plugins, the list is limited to types found in application plugins.
    /// </summary>
    public IEnumerable<Type> AllPluginTypes { get; }
        
    /// <summary>
    /// The plugin types limited to just those associated with application centric plugins.  
    /// If the module is within an application centric plugin, then this list will be the
    /// same as AllPluginTypes.
    /// </summary>
    public IEnumerable<Type> AllAppPluginTypes { get; }

    /// <summary>
    /// The application configuration configured for the application container.
    /// </summary>
    public IConfiguration Configuration { get; }
         
    public ModuleContext(ICompositeAppBuilder builder, IPlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(builder);

        LoggerFactory = builder.BootstrapLoggerFactory;
        Configuration = builder.Configuration;

        Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        AppHost = builder.HostPlugin;
            
        AllPluginTypes = FilteredTypesByPluginType(builder, plugin);
        AllAppPluginTypes = GetAppPluginTypes(builder);
    }
    
    /// <summary>
    /// Logger factory used to create loggers.  Depending on the stage of
    /// the bootstrap process, the logger factory will reference the
    /// bootstrap logger-factory or the host configured logger-factory.
    /// </summary>
    public ILoggerFactory LoggerFactory { get; private set; }

    internal void InitLogging(IServiceProvider services)
    {
        // Override bootstrap logger-factory with host built logger-factory.
        LoggerFactory = services.GetRequiredService<ILoggerFactory>();
    }

    private static IEnumerable<Type> FilteredTypesByPluginType(ICompositeAppBuilder builder, IPlugin plugin) => 
        plugin.PluginType == PluginTypes.CorePlugin ? builder.GetPluginTypes() : GetAppPluginTypes(builder);
        
    private static IEnumerable<Type> GetAppPluginTypes(ICompositeAppBuilder builder) =>
        builder.GetPluginTypes(PluginTypes.AppPlugin, PluginTypes.HostPlugin);
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Container;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Base;

namespace NetFusion.Bootstrap.Plugins
{
    /// <summary>
    /// Class containing information that can be used by a given module
    /// when the application container is bootstrapped.
    /// </summary>
    public class ModuleContext
    {
        private readonly ICompositeAppBuilder _builder;
        private ILoggerFactory _loggerFactory;
        private ILogger _logger;

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
         
        public ModuleContext(ICompositeAppBuilder builder, IPlugin plugin)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));

            Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
            AppHost = builder.HostPlugin;
            
            AllPluginTypes = FilteredTypesByPluginType(builder, plugin);
            AllAppPluginTypes = GetAppPluginTypes(builder);
        }

        /// <summary>
        /// The application configuration configured for the application container.
        /// </summary>
        public IConfiguration Configuration => _builder.Configuration;

        /// <summary>
        /// Logging factory.  Only available after the service-provider has been created.
        /// Use the ExtendedLogger in code executing before the container is created.
        /// </summary>
        public ILoggerFactory LoggerFactory
        {
            get
            {
                if (_loggerFactory == null)
                {
                    throw new InvalidOperationException(
                        $"LoggerFactory not available until service-provider created. Use {nameof(NfExtensions.Logger)}");
                }

                return _loggerFactory;
            }
        }

        /// <summary>
        /// Logger with the name of the plug-in used to identify the log messages.
        /// Can only be used after the service-provider has been created.
        /// </summary>
        public ILogger Logger
        {
            get
            {
                if (_logger == null)
                {
                    throw new InvalidOperationException(
                        "Logger can't be accessed until service-provider created. Use {nameof(NfExtensions.Logger)}");
                }

                return _logger;
            }
        }
        
        internal void InitLogging(IServiceProvider services)
        {
            _loggerFactory = services.GetService<ILoggerFactory>();
            _logger = _loggerFactory.CreateLogger(Plugin.GetType());
        }

        private static IEnumerable<Type> FilteredTypesByPluginType(ICompositeAppBuilder builder, IPlugin plugin)
        {
            // Core plug-in can access types from all other plug-in types.
            if (plugin.PluginType == PluginTypes.CorePlugin)
            {
                return builder.GetPluginTypes();
            }

            // Application centric plug-in can only access types contained in
            // other application plugs.
            return GetAppPluginTypes(builder);
        }

        private static IEnumerable<Type> GetAppPluginTypes(ICompositeAppBuilder builder)
        {
            return builder.GetPluginTypes(PluginTypes.AppPlugin, PluginTypes.HostPlugin);
        }
    }
}

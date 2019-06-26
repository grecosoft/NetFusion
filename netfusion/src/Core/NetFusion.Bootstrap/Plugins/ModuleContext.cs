using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace NetFusion.Bootstrap.Plugins
{
    /// <summary>
    /// Class containing information that can be used by a given module
    /// when the application container is bootstrapped.
    /// </summary>
    public class ModuleContext
    {
        private readonly ICompositeAppBuilder _builder;

        /// <summary>
        /// The plug-in representing the application host.
        /// </summary>
        public IPlugin AppHost { get; }

        /// <summary>
        /// The plug-in where the module is defined.
        /// </summary>
        public IPlugin Plugin { get; }
        
        /// <summary>
        /// The plug-in types that can be accessed by the module limited to the set based on its type of plug-in.  
        /// This list will contain all types from all plug-ins if the context is associated with a core plug-in.
        /// However, for application centric plug-ins, the list is limited to types found in application plug-ins.
        /// </summary>
        public IEnumerable<Type> AllPluginTypes { get; }
        
        /// <summary>
        /// The plug-in types limited to just those associated with application centric plug-ins.  
        /// If the module is within an application centric plug-in, then this list will be the
        /// same as AllPluginTypes.
        /// </summary>
        public IEnumerable<Type> AllAppPluginTypes { get; }
        
        public ModuleContext(ICompositeAppBuilder builder, IPlugin plugin)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));

            Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
            AppHost = builder.HostPlugin;
            
            AllPluginTypes = FilteredTypesByPluginType();
            AllAppPluginTypes = GetAppPluginTypes();
        }

        /// <summary>
        /// The application configuration configured for the application container.
        /// </summary>
        public IConfiguration Configuration => _builder.Configuration;

        public ILoggerFactory LoggerFactory { get; private set; }
        
        /// <summary>
        /// Logger with the name of the plug-in used to identify the log messages.
        /// </summary>
        public ILogger Logger { get; private set; }
        
        public void Initialize(IServiceProvider services)
        {
            var scopedLoggerType = typeof(ILogger<>).MakeGenericType(Plugin.GetType());

            LoggerFactory = services.GetService<LoggerFactory>();
            Logger = (ILogger)services.GetService(scopedLoggerType);
        }

        public void Log(LogLevel logLevel, string message, params object[] args)
        {
            
        }

        private IEnumerable<Type> FilteredTypesByPluginType()
        {
            // Core plug-in can access types from all other plug-in types.
            if (Plugin.PluginType == PluginTypes.CorePlugin)
            {
                return _builder.GetPluginTypes();
            }

            // Application centric plug-in can only access types contained in
            // other application plugs.
            return GetAppPluginTypes();
        }

        private IEnumerable<Type> GetAppPluginTypes()
        {
            return _builder.GetPluginTypes(PluginTypes.ApplicationPlugin,
                PluginTypes.HostPlugin);
        }

        // TODO:  Delete this after code-review for all plugins.  A plugin module now automatically
        // has any dependent plugin-module services set.
        public T GetPluginModule<T>() where T : IPluginModuleService
        {
            var foundModules = _builder.AllModules.OfType<T>().ToArray();
            if (! foundModules.Any())
            {
                throw new ContainerException($"Plug-in module of type: {typeof(T)} not found.");
            }

            if (foundModules.Length > 1)
            {
                throw new ContainerException($"Multiple plug-in modules implementing: {typeof(T)} found.");
            }

            return foundModules.First();
        }
    }
}

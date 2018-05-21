using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Bootstrap.Plugins
{
    /// <summary>
    /// Class containing information that can be used by a given module
    /// when the application container is bootstrapped.
    /// </summary>
    public class ModuleContext
    {
        private readonly CompositeApplication _compositeApp;

        /// <summary>
        /// The plug-in representing the application host.
        /// </summary>
        public Plugin AppHost { get; }

        /// <summary>
        /// The plug-in where the module is defined.
        /// </summary>
        public Plugin Plugin { get; }

        /// <summary>
        /// The logger factory configured for the application container.
        /// </summary>
        public ILoggerFactory LoggerFactory => _compositeApp.LoggerFactory;

        /// <summary>
        /// The application configuration configured for the application container.
        /// </summary>
        public IConfiguration Configuration => _compositeApp.Configuration;

        /// <summary>
        /// Logger with the name of the plug-in used to identify the log messages.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// The plug-in types that can be accessed by the module limited to the set based on its type of plug-in.  
        /// This list will contain all types from all plug-ins if the context is associated with a core plug-in.
        /// However, for application centric plug-ins, the list is limited to types found in application plug-ins.
        /// </summary>
        public IEnumerable<Type> AllPluginTypes { get; }

        /// <summary>
        /// The plug-in types limited to just those associated with application centric plug-ins.  
        /// If the module  is within an application centric plug-in, then this list will be the
        /// same as AllPluginTypes.
        /// </summary>
        public IEnumerable<Type> AllAppPluginTypes { get; }

        public ModuleContext(
            CompositeApplication compositeApp, 
            Plugin plugin)
        {
            _compositeApp = compositeApp ?? throw new ArgumentNullException(nameof(compositeApp));

            Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
            Logger = _compositeApp.LoggerFactory.CreateLogger(plugin.GetType().FullName);

            AppHost = compositeApp.AppHostPlugin;
            AllPluginTypes = FilteredTypesByPluginType();
            AllAppPluginTypes = GetAppPluginTypes();
        }

        private IEnumerable<Type> FilteredTypesByPluginType()
        {
            // Core plug-in can access types from all other plug-in types.
            if (Plugin.PluginType == PluginTypes.CorePlugin)
            {
                return _compositeApp.GetPluginTypes()
                    .Select(pt => pt.Type)
                    .ToList();
            }

            // Application centric plug-in can only access types contained in
            // other application plugs.
            return GetAppPluginTypes();
        }

        private IEnumerable<Type> GetAppPluginTypes()
        {
            return _compositeApp.GetPluginTypes(PluginTypes.AppComponentPlugin, PluginTypes.AppHostPlugin)
                .Select(pt => pt.Type)
                .ToList();
        }

        /// <summary>
        /// Returns plug-in type for corresponding .NET type.
        /// </summary>
        /// <param name="type">The .NET type.</param>
        /// <returns>Wrapped .NET type as a plug-in type.</returns>
        public PluginType GetPluginType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var allPluginTypes = _compositeApp.GetPluginTypes();
            var pluginType = allPluginTypes.FirstOrDefault(mt => mt.Type == type);

            if (pluginType == null)
            {
                throw new ContainerException(
                    $"A plug-in type could not be found for the corresponding .NET type: {type}.");
            }
            return pluginType;
        }

        /// <summary>
        /// Returns a plug-in module that implements a specific interface.
        /// </summary>
        /// <typeparam name="T">The interface of the module to locate.</typeparam>
        /// <returns>The module implementing the specified interface.  If one and
        /// only one module is not found, an exception is thrown.</returns>
        public T GetPluginModule<T>() where T : IPluginModuleService
        {
            var foundModules = _compositeApp.AllPluginModules.OfType<T>().ToArray();
            if (! foundModules.Any())
            {
                throw new ContainerException($"Plug-in module of type: {typeof(T)} not found.");
            }

            if (foundModules.Count() > 1)
            {
                throw new ContainerException($"Multiple plug-in modules implementing: {typeof(T)} found.");
            }

            return foundModules.First();
        }
    }
}

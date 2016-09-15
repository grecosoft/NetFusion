using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Logging;
using NetFusion.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Bootstrap.Plugins
{
    /// <summary>
    /// Class containing information that can be used by a given module.
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
        /// Logger with plug-in context.
        /// </summary>
        public IContainerLogger Logger { get; }

        /// <summary>
        /// All plug-in modules.
        /// </summary>
        public IEnumerable<IPluginModule> AllPluginModules { get; }

        /// <summary>
        /// The plug-in types that can be accessed by the module limited to
        /// the set based on its type of plug-in.
        /// </summary>
        public IEnumerable<Type> AllPluginTypes { get; }

        /// <summary>
        /// The plug-in types limited to just those associated with application
        /// centric plug-ins.  The module is within an application centric plug-in,
        /// then this list will be the same as AllPluginTypes.
        /// </summary>
        public IEnumerable<Type> AllAppPluginTypes { get; }

        /// <summary>
        /// The plug-in types limited to just those associated with a core plug-in.
        /// If this property is called by a module contained within an application
        /// centric plug-in an empty list will be returned.
        /// </summary>
        public IEnumerable<Type> AllCorePluginTypes { get; }

        internal ModuleContext(
            IContainerLogger logger,
            CompositeApplication compositeApp, 
            Plugin plugin)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(compositeApp, nameof(compositeApp));
            Check.NotNull(plugin, nameof(plugin));

            _compositeApp = compositeApp;

            this.AppHost = compositeApp.AppHostPlugin;
            this.AllPluginModules = compositeApp.AllPluginModules;
            this.Plugin = plugin;
            this.Logger = logger.ForContext(plugin.GetType());

            this.AllPluginTypes = FilteredTypesByPluginType();
            this.AllAppPluginTypes = GetAppPluginTypes();
            this.AllCorePluginTypes = GetCorePluginTypes();
        }

        private IList<Type> FilteredTypesByPluginType()
        {
            // Core plug-in can access types from all other plug-in types.
            if (this.Plugin.PluginType == PluginTypes.CorePlugin)
            {
                return _compositeApp.GetPluginTypesFrom()
                    .Select(pt => pt.Type)
                    .ToList();
            }

            // Application centric plug-in can only access types contained in
            // other application plugs.
            return GetAppPluginTypes();
        }

        private IList<Type> GetAppPluginTypes()
        {
            return _compositeApp.GetPluginTypesFrom(PluginTypes.AppComponentPlugin, PluginTypes.AppHostPlugin)
                .Select(pt => pt.Type)
                .ToList();
        }

        private IList<Type> GetCorePluginTypes()
        {
            if (this.Plugin.PluginType == PluginTypes.CorePlugin)
            {
                return _compositeApp.GetPluginTypesFrom(PluginTypes.CorePlugin)
                .Select(pt => pt.Type)
                .ToList();
            }

            return new List<Type>();
        }

        /// <summary>
        /// Returns plug-in type for corresponding .NET type.
        /// </summary>
        /// <param name="type">The .NET type.</param>
        /// <returns>Wrapped .NET type as a plug-in type.</returns>
        public PluginType GetPluginType(Type type)
        {
            Check.NotNull(type, nameof(type));

            var allPluginTypes = _compositeApp.GetPluginTypesFrom();
            var pluginType = allPluginTypes.FirstOrDefault(mt => mt.Type == type);

            if (pluginType == null)
            {
                throw new ContainerException(
                    $"A plug-in type could not be found for the corresponding .NET type: {type}.");
            }
            return pluginType;

        }

        // Returns plug-in type for corresponding object instance.
        private PluginType GetPluginType(object instance)
        {
            Check.NotNull(instance, nameof(instance));
            return GetPluginType(instance.GetType());
        }

        /// <summary>
        ///  Returns a plug-in module that implements a specific interface.
        /// </summary>
        /// <typeparam name="T">The interface of the module to locate.</typeparam>
        /// <returns>The module implementing the specified interface.  If one and
        /// only one module is not found, an exception is thrown.</returns>
        public T GetPluginModule<T>() where T : IPluginModuleService
        {
            var foundModules = this.AllPluginModules.OfType<T>();
            if (!foundModules.Any())
            {
                throw new InvalidOperationException(
                    $"Plug-in module of type: {typeof(T)} not found.");
            }

            if (foundModules.Count() > 1)
            {
                throw new InvalidOperationException(
                    $"Multiple plug-in modules implementing: {typeof(T)} found.");
            }

            return foundModules.First();
        }
    }
}

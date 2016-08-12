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

        internal ModuleContext(
            IContainerLogger logger,
            CompositeApplication compositeApp, 
            Plugin plugin)
        {
            Check.NotNull(compositeApp, nameof(compositeApp));
            Check.NotNull(plugin, nameof(plugin));

            _compositeApp = compositeApp;

            this.AppHost = compositeApp.AppHostPlugin;
            this.AllPluginModules = compositeApp.AllPluginModules;
            this.Plugin = plugin;
            this.Logger = logger.ForContext(plugin.GetType());
        }

        /// <summary>
        /// Returns types associated with a specific category of plug-in.
        /// </summary>
        /// <param name="pluginTypes">The category of plug-ins to limit the return types.</param>
        /// <returns>List of limited plug-in types or all plug-in types if no category is specified.</returns>
        public IEnumerable<Type> GetPluginTypesFrom(params PluginTypes[] pluginTypes)
        {
            Check.NotNull(pluginTypes, nameof(pluginTypes));
            return _compositeApp.GetPluginTypesFrom(pluginTypes).Select(pt => pt.Type);
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
        /// Returns all the object instances created from types belonging
        /// to a specified plug-in.
        /// </summary>
        /// <typeparam name="T">The type of the source.</typeparam>
        /// <param name="instances">List of object references.</param>
        /// <param name="plugin">The plug in to check for ownership.</param>
        /// <returns>Filters list of object instances created from types 
        /// belonging to a specified plug in.</returns>
        public IEnumerable<T> CreatedFromPlugin<T>(IEnumerable<T> instances, Plugin plugin)
        {
            Check.NotNull(instances, nameof(instances));
            Check.NotNull(plugin, nameof(plugin));

            return instances.Where(i => GetPluginType(i).Plugin == plugin);
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
                throw new InvalidOperationException($"Plug-in module of type: {typeof(T)} not found.");
            }

            if (foundModules.Count() > 1)
            {
                throw new InvalidOperationException($"Multiple plug-in modules implementing: {typeof(T)} found.");
            }

            return foundModules.First();
        }
    }
}

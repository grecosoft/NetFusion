using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Manifests;
using NetFusion.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Bootstrap.Plugins
{
    /// <summary>
    /// Used to store information about a plug-in that is part of the composite application.
    /// </summary>
    public class Plugin
    {
        /// <summary>
        /// The type of the plug-in based on the containing assembly manifest type.
        /// </summary>
        public PluginTypes PluginType { get; internal set; }

        /// <summary>
        /// The manifest associated with the plug-in.
        /// </summary>
        /// <returns>Plug-in manifest.</returns>
        public IPluginManifest Manifest { get; }

        /// <summary>
        /// The name of the assembly the plug-in manifest was found.
        /// </summary>
        /// <returns>Assembly name.</returns>
        public string AssemblyName { get; }

        public Plugin(IPluginManifest manifest)
        {          
            Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
            AssemblyName = manifest.AssemblyName;

            SetPluginType(manifest);
        }

        /// <summary>
        /// The types contained within the plug-in.
        /// </summary>
        /// <returns>List of plug-in types containing additional information for 
        /// the associated .NET type.</returns>
        public PluginType[] PluginTypes { get; private set; }

        /// <summary>
        /// Modules found within the plug-in used to bootstrap the plug-in.
        /// </summary>
        /// <returns>List of plug-in modules.</returns>
        public IPluginModule[] Modules { get; private set; }

        
        /// <summary>
        /// Sets the resolved types from which a plugin-in is built.  This method is invoked 
        /// by the ITypeResolver implementation.
        /// </summary>
        /// <param name="pluginTypes">Type types contained with-in the plug-in.</param>
        /// <param name="pluginModules">Modules used to bootstrap the plugin.</param>
        public void SetPluginResolvedTypes(PluginType[] pluginTypes, IPluginModule[] pluginModules)
        {
            PluginTypes = pluginTypes ?? throw new ArgumentNullException(nameof(pluginTypes));
            Modules = pluginModules ?? throw new ArgumentNullException(nameof(pluginModules));
        }

        /// <summary>
        /// Returns modules that are not marked as being executed.  This is for use during development 
        /// to disable a given plug-in module that is currently being developed and not complete.
        /// </summary>
        public IPluginModule[] IncludedModules => Modules.Where(m => !m.IsExcluded).ToArray();

        /// <summary>
        /// The configurations associated with the plug-in.
        /// </summary>
        public IList<IContainerConfig> PluginConfigs { get; internal set; }

        /// <summary>
        /// The known types that were discovered by all of the modules contained within the plug-in.
        /// </summary>
        public Type[] DiscoveredTypes { get; internal set; }

        private IEnumerable<Type> Types => PluginTypes.Select(pt => pt.Type);

        /// <summary>
        /// Filters the list of instances to only those created from types belonging to the plug-in.
        /// </summary>
        /// <typeparam name="T">The type of object instances.</typeparam>
        /// <param name="instances">List of object instances to filter.</param>
        /// <returns>Filtered list of object instances to those created
        /// from types belonging to the plug-in.</returns>
        public IEnumerable<T> CreatedFrom<T>(IEnumerable<T> instances)
        {
            if (instances == null) throw new ArgumentNullException(nameof(instances));

            return instances
                .Where(i => HasType(i.GetType()))
                .OfType<T>();
        }

        /// <summary>
        /// Returns a configuration associated with the plug-in for a given
        /// configuration type. 
        /// </summary>
        /// <typeparam name="T">The configuration type.</typeparam>
        /// <returns>The registered configuration if found. Otherwise,
        /// a default instance if returned.</returns>
        public T GetConfig<T>() where T : IContainerConfig, new()
        {
            var config = PluginConfigs.FirstOrDefault(
                pc => pc.GetType() == typeof(T));

            if (config == null)
            {
                config = new T();
                PluginConfigs.Add(config);
            }

            return (T)config;
        }

        /// <summary>
        /// Determines if a configuration was specified by the host application.
        /// </summary>
        /// <typeparam name="T">The type of the configuration.</typeparam>
        /// <returns>True if configuration is registered.  Otherwise, false is returned.</returns>
        public bool IsConfigSet<T>() where T : IContainerConfig
        {
            return PluginConfigs.Any(pc => pc.GetType() == typeof(T));
        }

        /// <summary>
        /// Returns a required configuration associated with the plug-in for a 
        /// given configuration type. 
        /// </summary>
        /// <typeparam name="T">The configuration type.</typeparam>
        /// <returns>The registered configuration if found. Otherwise,
        /// and exception is thrown.</returns>
        public T GetRequiredConfig<T>() where T : IContainerConfig, new()
        {
            if (!IsConfigSet<T>())
            {
                throw new ContainerException(
                    $"The container configuration of type: {typeof(T).FullName} has not been " +
                    $"registered by the host application.");
            }

            return GetConfig<T>();
        }

        /// <summary>
        /// Determines if the plug-in contains a specific type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if found, otherwise false.</returns>
        internal bool HasType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return Types.Contains(type);
        }

        private void SetPluginType(IPluginManifest manifest)
        {
            if (manifest is IAppHostPluginManifest)
                PluginType = Plugins.PluginTypes.AppHostPlugin;
            else if (manifest is IAppComponentPluginManifest)
                PluginType = Plugins.PluginTypes.AppComponentPlugin;
            else if (manifest is ICorePluginManifest)
                PluginType = Plugins.PluginTypes.CorePlugin;
            else throw new InvalidOperationException($"Invalided manifest type of: {manifest.GetType()}");
        }
    }
}

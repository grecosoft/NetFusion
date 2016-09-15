using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Manifests;
using NetFusion.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Bootstrap.Plugins
{
    /// <summary>
    /// Used to store information about a plug-in that is used 
    /// to compose the application's functionality.
    /// </summary>
    public class Plugin
    {
        /// <summary>
        /// Enumerated plug-in type category.
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
            Check.NotNull(manifest, nameof(manifest));

            this.Manifest = manifest;
            this.AssemblyName = manifest.AssemblyName;
            SetPluginType(manifest);
        }

        /// <summary>
        /// The configurations associated with the plug-in set
        /// when the application container is created by the host.
        /// </summary>
        public IList<IContainerConfig> PluginConfigs { get; internal set; }

        /// <summary>
        /// The types contained within the plug-in.
        /// </summary>
        /// <returns>List of plug-in types containing additional information
        /// for the associated .NET type.</returns>
        public PluginType[] PluginTypes { get; set; }

        /// <summary>
        /// Modules found within the plug-in used to bootstrap the plug-in.
        /// </summary>
        /// <returns>List of plug-in modules.</returns>
        public IPluginModule[] PluginModules { get; set; }

        /// <summary>
        /// The known types that were discovered by all of the plug-in modules
        /// contained within the plug-in.
        /// </summary>
        public Type[] DiscoveredTypes { get; internal set; }

        /// <summary>
        /// Filters the list of instances to only those created from types 
        /// belonging to the plug-in.
        /// </summary>
        /// <typeparam name="T">The type of object instances.</typeparam>
        /// <param name="instances">List of object instances to filter.</param>
        /// <returns>Filtered list of object instances to those created
        /// from types belonging to the plug-in.</returns>
        public IEnumerable<T> CreatedFrom<T>(IEnumerable<T> instances)
        {
            Check.NotNull(instances, nameof(instances));
            return instances.Where(i => HasType(i.GetType()));
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
            var config = this.PluginConfigs.FirstOrDefault(
                pc => pc.GetType() == typeof(T));

            if (config == null)
            {
                config = new T();
                this.PluginConfigs.Add(config);
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
            return this.PluginConfigs.Any(
                pc => pc.GetType() == typeof(T));
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
                throw new InvalidOperationException(
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
            Check.NotNull(type, nameof(type));

            return this.Types.Contains(type);
        }

        private IEnumerable<Type> Types => this.PluginTypes.Select(pt => pt.Type); 

        private void SetPluginType(IPluginManifest manifest)
        {
            if (manifest is IAppHostPluginManifest)
                this.PluginType = Plugins.PluginTypes.AppHostPlugin;
            else if (manifest is IAppComponentPluginManifest)
                this.PluginType = Plugins.PluginTypes.AppComponentPlugin;
            else if (manifest is ICorePluginManifest)
                this.PluginType = Plugins.PluginTypes.CorePlugin;
            else throw new InvalidOperationException($"Invalided manifest type of: {manifest.GetType()}");
        }
    }
}

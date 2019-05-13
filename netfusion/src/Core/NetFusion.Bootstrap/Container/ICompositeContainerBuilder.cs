using System;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Provides methods for building a composite application container from
    /// a set of registered plugins.
    /// </summary>
    public interface ICompositeContainerBuilder
    {
        /// <summary>
        /// Adds a plugin type for which modules should be loaded during the
        /// initialization of the composite-container application.
        /// </summary>
        /// <typeparam name="TPlugin">The plugin to be added.</typeparam>
        /// <returns>Reference to builder.</returns>
        ICompositeContainerBuilder AddPlugin<TPlugin>() where TPlugin : IPlugin, new();
        
        /// <summary>
        /// Can be called by the host when bootstrapping the application to configure
        /// container level configurations.
        /// </summary>
        /// <param name="configure">Delegate passed an instance of the configuration.</param>
        /// <typeparam name="T">The type of the container to initialize.</typeparam>
        /// <returns>Reference to the builder.  If the specified configuration is not
        /// a registered at the container level, an exception is raised.</returns>
        ICompositeContainerBuilder InitConfig<T>(Action<T> configure) where T : IPluginConfig;
        
        /// <summary>
        /// Returns reference to a plugin specific configuration.
        /// </summary>
        /// <typeparam name="T">The type of the plugin configuration to return.</typeparam>
        /// <returns>Reference to the plugin configuration instance of the specified
        /// type or an exception if not found.</returns>
        T GetPluginConfig<T>() where T : IPluginConfig;
        
        /// <summary>
        /// Builds a composite container from the set of added plug-ins.  After this method
        /// is invoked, the IServiceCollection will have been populated with services from 
        /// the plugin modules and the instance of the IServiceProvider created.
        /// </summary>
        /// <returns>Reference to the built composite-container that can be started by the host.</returns>
        ICompositeContainer Build();
    }
}
using System;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Core.Bootstrap.Container;

/// <summary>
/// Provides methods for building a composite-application from a set of registered plugins.
/// A reference to this interface is used by the host application when started to register
/// plugins from which the composite-container is built.  A reference to this interface is
/// obtained by calling the CompositeContainer IServiceCollection extension method.
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
    /// Adds one or more plugin instances for which modules should be loaded during
    /// the initialization of the composite-container.  This method is used for testing.
    /// </summary>
    /// <param name="plugin">One ore more plugin instances.</param>
    /// <returns>Reference to builder.</returns>
    ICompositeContainerBuilder AddPlugin(params IPlugin[] plugin);

    /// <summary>
    /// Can be called by the host when bootstrapping the application to configure
    /// plugin level configurations.
    /// </summary>
    /// <param name="configure">Delegate passed an instance of the configuration.</param>
    /// <typeparam name="T">The type of the configuration to initialize.</typeparam>
    /// <returns>Reference to the builder.  If the specified configuration is not
    /// registered for one of the added container's plugins, an exception is raised.</returns>
    ICompositeContainerBuilder InitPluginConfig<T>(Action<T> configure) where T : IPluginConfig;

    /// <summary>
    /// Builds a composite-application from the set of registered plug-ins.  After this method
    /// is invoked, the ICompositeApp will have been added to the IServiceCollection. The host
    /// can obtain an instance of the ICompositeApp from the created IServiceProvider.
    /// </summary>
    /// <param name="services">Delegate passed the service-collection being populated.  This is
    /// the last place where the host can add services and therefore will override any existing
    /// registered service.</param>
    void Compose(Action<IServiceCollection>? services = null);
}
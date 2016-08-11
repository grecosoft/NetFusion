using Autofac;
using NetFusion.Bootstrap.Logging;
using System;
using System.Collections.Generic;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Container that bootstraps the application using plug-in types determined by the specified TypeResolver.  
    /// The plug-in types are scanned based on a set of conventions and used to create an Autofac container.
    /// </summary>
    public interface IAppContainer: IDisposable
    {
        /// <summary>
        /// Reference to the associated container logger.
        /// </summary>
        IContainerLogger Logger { get; }

        /// <summary>
        /// Adds a container configuration to the container.
        /// </summary>
        /// <param name="config">The configuration to add.</param>
        /// <returns>The application container.</returns>
        IAppContainer WithConfig(IContainerConfig config);

        /// <summary>
        /// Adds a container configuration to the container specified
        /// by the generic type.
        /// </summary>
        /// <typeparam name="T">The type of the configuration.</typeparam>
        /// <returns>The application container.</returns>
        IAppContainer WithConfig<T>()
            where T : IContainerConfig, new();

        /// <summary>
        /// Adds a container configuration to the container specified by 
        /// the generic type and then calls an initialize function.
        /// </summary>
        /// <typeparam name="T">The type of the configuration.</typeparam>
        /// <param name="configAction">Delegate called to initialize the
        /// created configuration.</param>
        /// <returns>The application container.</returns>
        IAppContainer WithConfig<T>(Action<T> configAction)
            where T : IContainerConfig, new();

        /// <summary>
        /// Registers one or more configuration sections from the host's
        /// configuration file as container configurations.  If a section
        /// with the name don't exist, it is ignored.
        /// </summary>
        /// <param name="sectionNames">The name of the sections.</param>
        /// <returns>The application container.</returns>
        IAppContainer WithConfigSection(params string[] sectionNames);

        /// <summary>
        /// Loads and initializes all of the plug-ins but does not start
        /// their execution.
        /// </summary>
        /// <returns>Reference to the loaded container.</returns>
        ILoadedContainer Build();

        /// <summary>
        /// Dependency injection container from which lifetime scopes can 
        /// be created to resolve plug-in services.
        /// </summary>
        /// <returns>Configured DI container that can be used to resolve services.
        /// </returns>
        IContainer Services { get; }

        /// <summary>
        /// Log of the composite application built by the application container.
        /// </summary>
        /// <returns>Dictionary of key value pairs that can be serialized to JSON.</returns>
        IDictionary<string, object> Log { get; }

        /// <summary>
        /// Allows each module to safely stop.
        /// </summary>
        void Stop();
    }
}

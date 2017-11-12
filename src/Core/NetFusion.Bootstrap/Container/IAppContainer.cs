using Autofac;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Validation;
using System;
using System.Collections.Generic;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Container that bootstraps the application using plug-in types determined by the specified TypeResolver.  
    /// The plug-in types are scanned based on a set of conventions and used to create an application container.
    /// The end product of the bootstrapped application container is a configured DI Container configured based
    /// on a set of conventions.
    /// </summary>
    public interface IAppContainer: IDisposable
    {
        /// <summary>
        /// Reference to the associated container logger factory.  This is a reference to the factory
        /// logger provided by MS Logging Extensions.
        /// </summary>
        ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Adds a container configuration to the container.
        /// </summary>
        /// <param name="config">The configuration to add.</param>
        /// <returns>The application container.</returns>
        IAppContainer WithConfig(IContainerConfig config);

        /// <summary>
        /// Adds a container configuration to the container specified by the generic type.
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
        /// <param name="configInit">Delegate called to initialize the created configuration.</param>
        /// <returns>The application container.</returns>
        IAppContainer WithConfig<T>(Action<T> configInit)
            where T : IContainerConfig, new();

        /// <summary>
        /// Loads and initializes all of the plug-ins but does not start their execution.
        /// </summary>
        /// <returns>Reference to the loaded container.</returns>
        IBuiltContainer Build();

        /// <summary>
        /// Dependency injection container from which lifetime scopes can be created to resolve plug-in services.
        /// </summary>
        /// <returns>Configured DI container that can be used to resolve services.
        /// </returns>
        IContainer Services { get; }

        /// <summary>
        /// Log of the composite application built by the application container.
        /// </summary>
        /// <returns>Dictionary of key/value pairs that can be serialized to JSON.</returns>
        IDictionary<string, object> Log { get; }

        /// <summary>
        /// Creates an IObjectValidator used to validate a specific object instance.
        /// </summary>
        /// <param name="obj">The object to be validated.</param>
        /// <returns>Instance of IObjectValidator.</returns>
        IObjectValidator CreateValidator(object obj);

        /// <summary>
        /// Allows each module to be safely stopped.
        /// </summary>
        void Stop();
    }
}

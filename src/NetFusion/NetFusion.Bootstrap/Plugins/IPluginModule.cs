﻿using System.Collections.Generic;

namespace NetFusion.Bootstrap.Plugins
{
    /// <summary>
    /// Implemented by one or more plug-in classes allowing the
    /// plug-in to discover types based on known conventions. Also,
    /// allows concrete module to scan and register for plug-in types.
    /// </summary>
    public interface IPluginModule 
    {
        /// <summary>
        /// Indicates that the module should not be loaded.  This can
        /// be used when developing plug-in modules that are not ready
        /// to be included.
        /// </summary>
        bool IsExcluded { get; set; }

        /// <summary>
        /// Contains plug-in type information that can be used by the plug-in
        /// module during bootstrapping.
        /// </summary>
        /// <returns>Contains information that can be used by the module
        /// when it is being configured.</returns>
        ModuleContext Context { get; set; }

        /// <summary>
        /// Called before type scanning and type registration.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Called after all plug-in modules have been initialized but 
        /// before type scanning and type registration.
        /// </summary>
        void Configure();

        /// <summary>
        /// Allows the plug-in to scan for types it defines that are 
        /// to be registered with the dependency injection container.
        /// </summary>
        /// <param name="registration">Reference to instance used to
        /// filter types to be registered.  This registration only
        /// contains types contained in the plug-in associated with
        /// the module.</param>
        void ScanPlugin(TypeRegistration registration);

        /// <summary>
        /// Allows the plug-in to register specific components
        /// as services within the dependency injection container.
        /// </summary>
        /// <param name="builder">The Autofac builder used to 
        /// register components that can be dependency injected.
        /// </param>
        void RegisterComponents(Autofac.ContainerBuilder builder);

        /// <summary>
        /// Allows the plug-in module to scan for types within all other plug-ins.
        /// This registration is limited to all other plug-in types when called on 
        /// a core plug in.  For an application plug-in, the types are limited to 
        /// just other application plug ins.
        /// </summary>
        /// <param name="registration">Reference to instance used to
        /// filter types to be registered.</param>
        void ScanAllOtherPlugins(TypeRegistration registration);

        /// <summary>
        /// Allows a core plug-in module to scan for type limited to only 
        /// application centric plug-in types.
        /// </summary>
        /// <param name="registration">Reference to instance used to
        /// filter types to be registered.</param>
        void ScanApplicationPluginTypes(TypeRegistration registration);

        /// <summary>
        /// The last method called on the module by the bootstrap process.  
        /// Called after all types have been registered and the container
        /// has been created.
        /// </summary>
        /// <param name="container">The created container.</param>
        void StartModule(Autofac.IContainer container);

        /// <summary>
        /// Called after the module is initialized and configured so
        /// that it can add module specific logs to the application
        /// composite log.
        /// </summary>
        /// <param name="moduleLog">Log dictionary to populate.</param>
        void Log(IDictionary<string, object> moduleLog);
    }
}
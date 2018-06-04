using Microsoft.Extensions.DependencyInjection;
using NetFusion.Bootstrap.Dependencies;
using System;
using System.Collections.Generic;

namespace NetFusion.Bootstrap.Plugins
{
    /// <summary>
    /// Implemented by one or more plug-in classes allowing the plug-in to be configured.  
    /// This includes the registering of types within the dependency-injection container, 
    /// the discovering of known-type implementations, and the execution of any needed
    /// logic upon startup and shutdown.
    /// </summary>
    public abstract class PluginModule : IPluginModule,
        IDisposable
    {
        private bool _disposed;

        protected PluginModule()
        {
            IsExcluded = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (! dispose || _disposed) return;

            _disposed = true;
        }

        /// <summary>
        /// Indicates that the module should not be loaded.  This can be used when developing
        /// plug-in modules that are not ready to be included.
        /// </summary>
        public bool IsExcluded { get; set; }

        /// <summary>
        /// Contains plug-in information that can be used by the module during bootstrapping.
        /// </summary>
        /// <returns>Contains information that can be used by the module when it is being
        /// configured.</returns>
        public ModuleContext Context { get; set; }

        /// <summary>
        /// The first method called on the module.  This method is called on all modules 
        /// before the Configure method is called.
        /// </summary>
        public virtual void Initialize()
        {

        }

        /// <summary>
        /// Called after all plug-in modules have been initialized.
        /// </summary>
        public virtual void Configure()
        {

        }

        /// <summary>
        /// Called first for all plug-in modules to allow default service implementations
        /// to be registered for use if not overridden by another plug-in module.
        /// </summary>
        /// <param name="services">Service collection used to register services.
        /// </param>
        public virtual void RegisterDefaultServices(IServiceCollection services)
        {

        }

        /// <summary>
        /// Allows the plug-in to scan for types it defines that are to be registered
        /// with the service collection as services.
        /// </summary>
        /// <param name="catalog">Reference to instance used to filter types to be 
        /// registered.  This registration only contains types contained in the plug-in 
        /// associated with the module.</param>
        public virtual void ScanPlugin(ITypeCatalog catalog)
        {
        }

        /// <summary>
        /// Allows the plug-in to register specific types as services within the 
        /// service collection.
        /// </summary>
        /// <param name="services">Service collection used to register types 
        /// that can be dependency injected as services.
        /// </param>
        public virtual void RegisterServices(IServiceCollection services)
        {
        }

        /// <summary>
        /// Allows the plug-in module to scan for types within all other plug-ins.
        /// This registration contains all other plug-in types when called on a
        /// core plug in.  For an application plug-in, the types are limited to 
        /// only other application plug ins.
        /// </summary>
        /// <param name="catalog">Reference to instance used to filter types to be 
        /// registered.</param>
        public virtual void ScanAllOtherPlugins(ITypeCatalog catalog)
        {
        }

        /// <summary>
        /// Allows a core plug-in module to scan for types limited to only application
        /// centric plug-in types.
        /// </summary>
        /// <param name="catalog">Reference to instance used to filter types to 
        /// be registered.</param>
        public virtual void ScanApplicationPlugins(ITypeCatalog catalog)
        {
        }

        /// <summary>
        /// Method called on the module by the bootstrap process.  Called after
        /// all types have been registered and the container has been created.
        /// </summary>
        /// <param name="services">Scoped service provider.</param>
        public virtual void StartModule(IServiceProvider services)
        {
            
        }

        /// <summary>
        /// Called after all modules have been started.  This method can contain
        /// logic that requires calling other module provides services that must
        /// have already been started.
        /// </summary>
        /// <param name="services">Scoped service provider.</param>
        public virtual void RunModule(IServiceProvider services)
        {
            
        }

        /// <summary>
        /// Called when the container is stopped.  Allows the module to complete
        /// any processing before the container is stopped.
        /// </summary>
        /// <param name="services">Scoped service provider.</param>
        public virtual void StopModule(IServiceProvider services)
        {
            
        }

        /// <summary>
        /// Called after the module is initialized and configured so that it can 
        /// add module specific logs to the application
        /// composite log.
        /// </summary>
        /// <param name="moduleLog">Log dictionary to populate.</param>
        public virtual void Log(IDictionary<string, object> moduleLog)
        {

        }
    }
}

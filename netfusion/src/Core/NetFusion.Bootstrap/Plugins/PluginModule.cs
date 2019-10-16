using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using NetFusion.Bootstrap.Catalog;

namespace NetFusion.Bootstrap.Plugins
{
    /// <summary>
    /// One or more modules are defined by a plugin to organize their implementation. 
    /// This includes the registering of types within the dependency-injection container, 
    /// the discovering of known-type implementations, and the execution of any needed
    /// logic upon startup and shutdown.
    /// </summary>
    public abstract class PluginModule : IPluginModule
    {
        /// <summary>
        /// Contains plug-in context information that can be used by the module during
        /// bootstrapping.
        /// </summary>
        /// <returns>Provides context information associated with plugin.</returns>
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
        /// to be registered for use if not overridden by another plug-in module (usually
        /// at the application level by an application plugin).
        /// </summary>
        /// <param name="services">Service collection used to register services.
        /// </param>
        public virtual void RegisterDefaultServices(IServiceCollection services)
        {

        }

        /// <summary>
        /// Allows the plug-in to scan for types to be registered with the service collection.
        /// </summary>
        /// <param name="catalog">Reference to a catalog used to filter types to be registered.
        /// The types contained within the catalog are based on the plugin's type.  Core plugins,
        /// can scan types contained within all plugins.  Whereas application centric plugins are
        /// limited to scanning types contained within application plugins. </param>
        public virtual void ScanPlugins(ITypeCatalog catalog)
        {
        }

        /// <summary>
        /// Allows the plug-in to register specific types as services within the service collection.
        /// </summary>
        /// <param name="services">Service collection used to register types 
        /// that can be dependency injected as services.
        /// </param>
        public virtual void RegisterServices(IServiceCollection services)
        {
        }

        public Task StartModuleAsync(IServiceProvider services)
        {
            return OnStartModuleAsync(services);
        }

        public Task RunModuleAsync(IServiceProvider services)
        {
            return OnRunModuleAsync(services);
        }

        public Task StopModuleAsync(IServiceProvider services)
        {
            return OnStopModuleAsync(services);
        }
        
        /// <summary>
        /// Method called on the module by the bootstrap process.  Called after
        /// all types have been registered and the container has been created.
        /// </summary>
        /// <param name="services">Scoped service provider.</param>
        protected virtual Task OnStartModuleAsync(IServiceProvider services)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called after all modules have been started.  This method can contain
        /// logic that requires calling other module provides services that must
        /// have already been started.
        /// </summary>
        /// <param name="services">Scoped service provider.</param>
        protected virtual Task OnRunModuleAsync(IServiceProvider services)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when the container is stopped.  Allows the module to complete
        /// any processing before the container is stopped.
        /// </summary>
        /// <param name="services">Scoped service provider.</param>
        protected virtual Task OnStopModuleAsync(IServiceProvider services)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called after the module is initialized and configured so that it can 
        /// add module specific logs to the application composite log.
        /// </summary>
        /// <param name="moduleLog">Log dictionary to populate.</param>
        public virtual void Log(IDictionary<string, object> moduleLog)
        {

        }
    }
}

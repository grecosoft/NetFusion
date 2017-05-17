using Autofac;
using System;
using System.Collections.Generic;

namespace NetFusion.Bootstrap.Plugins
{
    /// <summary>
    /// Derived from by one or more plug-in classes allowing the plug-in to be configured.  
    /// This includes the registering of types within the dependency-injection container, 
    /// the discovering of known-type implementations, and the execution of any needed
    /// logic upon startup.
    /// </summary>
    public abstract class PluginModule : IPluginModule,
        IDisposable
    {
        private bool _disposed = false;

        public PluginModule()
        {
            this.IsExcluded = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (!dispose || _disposed) return;

            _disposed = true;
        }

        /// <summary>
        /// Indicates that the module should not be loaded.  This can be used when
        /// developing plug-in modules that are not ready to be included.
        /// </summary>
        public bool IsExcluded { get; set; }

        /// <summary>
        /// Contains plug-in context information that can be used by the plug-in
        /// module during bootstrapping.
        /// </summary>
        /// <returns>Contains information that can be used by the module
        /// when it is being configured.</returns>
        public ModuleContext Context { get; set; }

        /// <summary>
        /// Called before container type registration.
        /// </summary>
        public virtual void Initialize()
        {

        }

        /// <summary>
        /// Called after all plug-in modules have been initialized but 
        /// before container type registration.
        /// </summary>
        public virtual void Configure()
        {

        }

        /// <summary>
        /// Called first for all plug-in modules to allow for default instances
        /// of services to be registered that will be used if no overridden by
        /// another plug-in module.
        /// </summary>
        /// <param name="builder">The Autofac builder used to 
        /// register components that can be dependency injected.
        /// </param>
        public virtual void RegisterDefaultComponents(Autofac.ContainerBuilder builder)
        {

        }

        /// <summary>
        /// Allows the plug-in to scan for types it defines that are 
        /// to be registered with the dependency injection container.
        /// </summary>
        /// <param name="registration">Reference to instance used to
        /// filter types to be registered.  This registration only
        /// contains types contained in the plug-in associated with
        /// the module.</param>
        public virtual void ScanPlugin(TypeRegistration registration)
        {

        }

        /// <summary>
        /// Allows the plug-in to register specific components
        /// as services within the dependency injection container.
        /// </summary>
        /// <param name="builder">The Autofac builder used to 
        /// register components that can be dependency injected.
        /// </param>
        public virtual void RegisterComponents(Autofac.ContainerBuilder builder)
        {

        }

        /// <summary>
        /// Allows the plug-in module to scan for types within all other 
        /// plug-ins.  This registration is limited to all other plug-in
        /// types when called on a core plug in.  For an application plug in, 
        /// the types are limited to just other application plug ins.
        /// </summary>
        /// <param name="registration">Reference to instance used to
        /// filter types to be registered.</param>
        public virtual void ScanAllOtherPlugins(TypeRegistration registration)
        {

        }

        /// <summary>
        /// Allows a core plug-in module to scan for type limited to only 
        /// application centric plug-in types.
        /// </summary>
        /// <param name="registration">Reference to instance used to
        /// filter types to be registered.</param>
        public virtual void ScanApplicationPlugins(TypeRegistration registration)
        {

        }

        /// <summary>
        /// The last method called on the module by the bootstrap process.  
        /// Called after all types have been registered and the container
        /// has been created.
        /// </summary>
        /// <param name="scope">Child scope of the created container.</param>
        public virtual void StartModule(ILifetimeScope scope)
        {
            
        }

        /// <summary>
        /// Called after all modules have been started.
        /// </summary>
        /// <param name="scope">Child scope of the created container.</param>
        public virtual void RunModule(Autofac.ILifetimeScope scope)
        {

        }

        /// <summary>
        /// Allows the module to complete any processing before the container
        /// is stopped.
        /// </summary>
        /// <param name="scope">Child scope of the created container. </param>
        public virtual void StopModule(ILifetimeScope scope)
        {

        }

        /// <summary>
        /// Called after the module is initialized and configured so
        /// that it can add module specific logs to the application
        /// composite log.
        /// </summary>
        /// <param name="moduleLog">Log dictionary to populate.</param>
        public virtual void Log(IDictionary<string, object> moduleLog)
        {

        }
    }
}

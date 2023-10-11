using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.Bootstrap.Catalog;
using NetFusion.Core.Bootstrap.Exceptions;

namespace NetFusion.Core.Bootstrap.Plugins;

/// <summary>
/// One or more modules are defined by a plugin to organize their implementation. 
/// This includes the registering of services within the dependency-injection container, 
/// the discovering of known-type implementations, and the execution of any needed
/// logic upon startup and shutdown.
/// </summary>
public abstract class PluginModule : IPluginModule
{
    private ModuleContext? _context;
        
    // The following properties are set to record plugin module related information.
    string IPluginModule.Name => GetType().Name;
        
    PropertyInfo[] IPluginModule.DependentServiceProperties { get; set; } = Array.Empty<PropertyInfo>();

    IDictionary<PropertyInfo, Tuple<Type, Type[]>> IPluginModule.KnownTypeProperties { get; } =
        new Dictionary<PropertyInfo, Tuple<Type, Type[]>>();

    void IPluginModule.SetContext(ModuleContext context) => _context = context;

    /// <summary>
    /// Contains plug-in context information that can be used by the module during
    /// bootstrapping.
    /// </summary>
    /// <returns>Provides context information associated with plugin.</returns>
    public ModuleContext Context => _context ?? 
        throw new BootstrapException("Module Context not set.");

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
    /// Allows the plug-in to register specific types as services within the service collection.
    /// </summary>
    /// <param name="services">Service collection used to register types 
    /// that can be dependency injected as services.
    /// </param>
    public virtual void RegisterServices(IServiceCollection services)
    {
    }

    /// <summary>
    /// Allows the plug-in to scan for types to be registered with the service collection.
    /// </summary>
    /// <param name="catalog">Reference to a catalog used to filter types to be registered.
    /// The types contained within the catalog are based on the plugin's type.  Core plugins,
    /// can scan types contained within all plugins.  Whereas application centric plugins are
    /// limited to scanning types contained within application plugins. </param>
    public virtual void ScanForServices(ITypeCatalog catalog)
    {
    }

    /// <summary>
    /// Method called on the module by the bootstrap process.  Called after all types have been
    /// registered and the composite application has been created.
    /// </summary>
    /// <param name="services">Scoped service provider.</param>
    Task IPluginModule.StartModuleAsync(IServiceProvider services) => OnStartModuleAsync(services);

    /// <summary>
    /// Called after all modules have been started.  This method can contain logic that requires
    /// calling other module provided services that must have already been started.
    /// </summary>
    /// <param name="services">Scoped service provider.</param>
    Task IPluginModule.RunModuleAsync(IServiceProvider services) => OnRunModuleAsync(services);

    /// <summary>
    /// Called when the composite application is stopped.  Allows the module to complete any processing
    /// before the composite application is stopped.
    /// </summary>
    /// <param name="services">Scoped service provider.</param>
    Task IPluginModule.StopModuleAsync(IServiceProvider services) => OnStopModuleAsync(services);

    /// <summary>
    /// Method called on the module by the bootstrap process.  Called after
    /// all types have been registered and the container has been created.
    /// </summary>
    /// <param name="services">Scoped service provider.</param>
    protected virtual Task OnStartModuleAsync(IServiceProvider services) => Task.CompletedTask;

    /// <summary>
    /// Called after all modules have been started.  This method can contain
    /// logic that requires calling other module provides services that must
    /// have already been started.
    /// </summary>
    /// <param name="services">Scoped service provider.</param>
    protected virtual Task OnRunModuleAsync(IServiceProvider services) => Task.CompletedTask;

    /// <summary>
    /// Called when the container is stopped.  Allows the module to complete
    /// any processing before the container is stopped.
    /// </summary>
    /// <param name="services">Scoped service provider.</param>
    protected virtual Task OnStopModuleAsync(IServiceProvider services) => Task.CompletedTask;

    /// <summary>
    /// Called after the module is initialized and configured so that it can 
    /// add module specific logs to the application composite log.
    /// </summary>
    /// <param name="moduleLog">Log dictionary to populate.</param>
    public virtual void Log(IDictionary<string, object> moduleLog)
    {

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Core.Bootstrap.Catalog;
using NetFusion.Core.Bootstrap.Exceptions;
using NetFusion.Core.Bootstrap.Logging;
using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Core.Bootstrap.Container;

/// <summary>
/// Responsible for registering the ICompositeApp instance built from
/// a set of plugins specified by the application host.
///
/// https://github.com/grecosoft/NetFusion/wiki/core-bootstrap-overview
/// </summary>
internal class CompositeAppBuilder : ICompositeAppBuilder
{
    // .NET Core Service Abstractions:
    public IServiceCollection ServiceCollection { get; }
    public IConfiguration Configuration { get; }
        
    public IPlugin[] AllPlugins { get; private set; } = Array.Empty<IPlugin>();
    public IPluginModule[] AllModules { get; private set; } = Array.Empty<IPluginModule>();
        
    // Plugins filtered by type:
    private IPlugin? _hostPlugin;
    public IPlugin[] AppPlugins { get; private set; } = Array.Empty<IPlugin>();
    public IPlugin[] CorePlugins { get; private set; } = Array.Empty<IPlugin>();
        
    // Logging Properties:
    public CompositeAppLogger CompositeLog { get; private set; }
        
    public CompositeAppBuilder(IServiceCollection serviceCollection, IConfiguration configuration)
    {
        ServiceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        CompositeLog = new CompositeAppLogger(this, ServiceCollection);
    }
        
    // =========================== [Plug Composition] ==========================

    /// <summary>
    /// Assembles all plugins using the type resolver by setting each plugin's dependent
    /// services and composing each plugin for a set of discovered type instances.
    /// </summary>
    /// <param name="plugins">List of all plugins used to build the composite application.</param>
    /// <param name="typeResolver">The type resolver.</param>
    internal void AssemblePlugins(IPlugin[] plugins, ITypeResolver typeResolver)
    {
        if (typeResolver == null) throw new ArgumentNullException(nameof(typeResolver));
        if (plugins == null) throw new ArgumentNullException(nameof(plugins));
            
        InitializePlugins(plugins, typeResolver);
        SetPluginServiceDependencies();
        ComposePlugins(typeResolver);
        ConfigurePlugins();
    }

    public IPlugin HostPlugin => _hostPlugin ?? throw new BootstrapException(
        "Host Plugin can't be accessed until builder is composed.");
      

    /// <summary>
    /// Returns types associated with a specific category of plugin.
    /// </summary>
    /// <param name="pluginTypes">The category of plugins to limit the return types.</param>
    /// <returns>List of limited plugin types or all plugin types if no category is specified.</returns>
    public IEnumerable<Type> GetPluginTypes(params PluginTypes[] pluginTypes)
    {
        if (pluginTypes == null) throw new ArgumentNullException(nameof(pluginTypes),
            "List of Plugin types cannot be null.");

        return pluginTypes.Length == 0 ? AllPlugins.SelectMany(p => p.Types) 
            : AllPlugins.Where(p => pluginTypes.Contains(p.PluginType)).SelectMany(p => p.Types);
    }
        
    // --------------------------- Initialization -------------------------------

    private void InitializePlugins(IPlugin[] plugins, ITypeResolver typeResolver)
    {
        SetPluginAssemblyInfo(plugins, typeResolver);
            
        // Before composing the plugin modules, verify the plugins from which 
        // the composite application is being build.
        var validator = new CompositeAppValidation(plugins);
        validator.Validate();

        CategorizePlugins(plugins);
    }
        
    // Delegates to the type resolver to populate information and the types associated with each plugin.
    // This decouples the container from runtime information and makes it easier to test.
    private static void SetPluginAssemblyInfo(IPlugin[] plugins, ITypeResolver typeResolver)
    {           
        foreach (IPlugin plugin in plugins)
        {
            typeResolver.SetPluginMeta(plugin);
        }
    }
        
    private void CategorizePlugins(IEnumerable<IPlugin> plugins)
    {
        AllPlugins = plugins.ToArray();
        AllModules = AllPlugins.SelectMany(p => p.Modules).ToArray();
            
        _hostPlugin = AllPlugins.First(p => p.PluginType == PluginTypes.HostPlugin);
        AppPlugins = AllPlugins.Where(p => p.PluginType == PluginTypes.AppPlugin).ToArray();
        CorePlugins = AllPlugins.Where(p => p.PluginType == PluginTypes.CorePlugin).ToArray();
    }
        
    // --------------------------- Service Dependencies -------------------------------
        
    // https://github.com/grecosoft/NetFusion/wiki/core-modules-services
    private void SetPluginServiceDependencies()
    {
        SetPluginServiceDependencies(CorePlugins, PluginTypes.CorePlugin);
        SetPluginServiceDependencies(AppPlugins, PluginTypes.AppPlugin, PluginTypes.CorePlugin);

        SetPluginServiceDependencies(new []{ HostPlugin }, 
            PluginTypes.HostPlugin, 
            PluginTypes.AppPlugin, 
            PluginTypes.CorePlugin);
    }
        
    // A plugin module can reference another module by having a property deriving from IPluginModuleService
    // Finds all IPluginModuleService derived properties of the referencing module and sets them to the
    // corresponding referenced module instance. 
    private void SetPluginServiceDependencies(IPlugin[] plugins, params PluginTypes[] pluginTypes)
    {
        foreach (IPlugin plugin in plugins)
        {
            foreach (IPluginModule module in plugin.Modules)
            {
                module.DependentServiceProperties = GetDependentServiceProperties(module);
                foreach (PropertyInfo serviceProp in module.DependentServiceProperties)
                {
                    IPluginModuleService dependentService = GetPluginModuleService(module, 
                        serviceProp.PropertyType, pluginTypes);
                        
                    serviceProp.SetValue(module, dependentService);
                }
            }
        }
    }
        
    private static PropertyInfo[] GetDependentServiceProperties(IPluginModule module)
    {
        const BindingFlags bindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        return module.GetType().GetProperties(bindings)
            .Where(p =>
                p.PropertyType.IsDerivedFrom(typeof(IPluginModuleService))
                && p.CanWrite)
            .ToArray();
    }

    /// <summary>
    /// Given a IPluginModuleService type referenced by a module's property, searches for the module 
    /// providing the implementation within all plugin types specified.  
    /// 
    /// For example, a core plugin should only be dependent on other core plugin modules.  It would
    /// not make sense for a lower level core plugin to be dependent on a higher-level application
    /// specific plugin module.
    /// 
    /// On the other hand, an application specific plugin module can be dependent on both core and
    /// other application plugin modules.  This can be the case since application plugin modules
    /// can build on top of services provided by core plugins.
    /// </summary>
    /// <param name="module">The plugin-module being on which the service type is to be set.</param>
    /// <param name="serviceType">The module having a dependency on another module.</param>
    /// <param name="pluginTypes">The types of plugins (Core, App, Host) to search for the dependent module.</param>
    /// <returns>Reference to the dependent module.</returns>
    private IPluginModuleService GetPluginModuleService(IPluginModule module, Type serviceType, PluginTypes[] pluginTypes)
    {
        var foundModules = AllPlugins.Where(p=> pluginTypes.Contains(p.PluginType))
            .SelectMany(p => p.Modules)
            .Where(m => m.GetType().IsDerivedFrom(serviceType)).ToArray();
            
        if (! foundModules.Any())
        {
            throw new BootstrapException(
                $"Plugin module implementing service type: {serviceType} not found for module: {module.GetType()}.");
        }
            
        if (foundModules.Length > 1)
        {
            throw new BootstrapException(
                $"Multiple plugin modules implementing service type: {serviceType} found for module: {module.GetType()}.");
        }

        return (IPluginModuleService)foundModules.First();
    }
        
    // --------------------------- Plugin Composition -------------------------------

    // Allow each plug-in module to compose itself from concrete types, defined by other plugins,
    // based on abstract types defined by the plugin being composed.  Think of this as a simplified
    // implementation of Microsoft's MEF.
    //
    // https://github.com/grecosoft/NetFusion/wiki/core-modules-knowntypes
    private void ComposePlugins(ITypeResolver typeResolver)
    {
        ComposeCorePlugins(typeResolver);
        ComposeAppPlugins(typeResolver);
        ComposeHostPlugin(typeResolver);
    }

    // Core plugins are composed from all other plugin types since they implement
    // reusable cross-cutting concerns.
    private void ComposeCorePlugins(ITypeResolver typeResolver)
    {
        var allPluginTypes = GetPluginTypes().ToArray();

        foreach (var plugin in CorePlugins)
        {
            typeResolver.ComposePlugin(plugin, allPluginTypes);
        }
    }

    // Application plugins contain a specific application's implementations
    // and are composed only from other application specific plugin types.
    private void ComposeAppPlugins(ITypeResolver typeResolver)
    {
        var allAppPluginTypes = GetPluginTypes(
            PluginTypes.AppPlugin, 
            PluginTypes.HostPlugin).ToArray();

        foreach (var plugin in AppPlugins)
        {
            typeResolver.ComposePlugin(plugin, allAppPluginTypes);
        }
    }

    private void ComposeHostPlugin(ITypeResolver typeResolver)
    {
        var hostPluginTypes = GetPluginTypes(PluginTypes.HostPlugin).ToArray();
        typeResolver.ComposePlugin(HostPlugin, hostPluginTypes);
    }
        
    // --------------------------- Module Initialization / Configuration -------------------------------
        
    // Before services are registered by modules, a two phase initialization is completed.
    // First all modules have the Initialize() method called.  The Initialize() method is
    // where plugins should cache information that can be accessed by other modules.
    // After all modules are initialized, each module has the Configure() method called.
    // Code contained within a module's Configure() method can reference information
    // initialized by a dependent module.
    //
    // https://github.com/grecosoft/NetFusion/wiki/core-bootstrap-modules#initialization-and-configuration
    private void ConfigurePlugins()
    {
        CorePlugins.ForEach(InitializeModules);
        AppPlugins.ForEach(InitializeModules);
        InitializeModules(HostPlugin);

        CorePlugins.ForEach(ConfigureModules);
        AppPlugins.ForEach(ConfigureModules);
        ConfigureModules(HostPlugin);
    }

    private void InitializeModules(IPlugin plugin)
    {
        foreach (IPluginModule module in plugin.Modules)
        {
            var context = new ModuleContext(this, plugin);
            module.SetContext(context);
            module.Initialize();
        }
    }

    private void ConfigureModules(IPlugin plugin)
    {
        foreach (IPluginModule module in plugin.Modules)
        {
            module.Configure();
        }
    }
        
    // =========================== [Module Service Registrations] =========================
   
    // Allows each module to register services that can be injected into
    // other components.  These services expose functionality implemented
    // by a plugin that can be injected into other components.
    //
    // https://github.com/grecosoft/NetFusion/wiki/core-modules-service-registration
    internal void RegisterPluginServices()
    {
        // Plugin Service Registrations:
        RegisterCorePluginServices();
        RegisterAppPluginServices();
        RegisterHostPluginServices();

        // Additional Registrations:
        RegisterPluginModulesAsService();
        RegisterCompositeApplication();
    }
        
    private void RegisterCorePluginServices()
    {
        var allPluginTypes = GetPluginTypes(
            PluginTypes.CorePlugin,
            PluginTypes.AppPlugin, 
            PluginTypes.HostPlugin).ToArray();
            
        RegisterPluginServices(CorePlugins);
        ScanForServices(CorePlugins, allPluginTypes);
    }
        
    private void RegisterAppPluginServices()
    {
        var allAppPluginTypes = GetPluginTypes(
            PluginTypes.AppPlugin, 
            PluginTypes.HostPlugin).ToArray();
            
        RegisterPluginServices(AppPlugins);
        ScanForServices(AppPlugins, allAppPluginTypes);
    }

    private void RegisterHostPluginServices()
    {
        var hostPluginTypes = GetPluginTypes(PluginTypes.HostPlugin).ToArray();
            
        RegisterPluginServices(new []{ HostPlugin });
        ScanForServices(new []{ HostPlugin }, hostPluginTypes);
    }

    private void ScanForServices(IPlugin[] plugins, Type[] pluginTypes)
    {
        var catalog = ServiceCollection.CreateCatalog(pluginTypes);

        foreach (var module in plugins.SelectMany(p => p.Modules))
        {
            module.ScanForServices(catalog);
        }
    } 
        
    private void RegisterPluginServices(IPlugin[] plugins)
    {
        foreach (IPluginModule module in plugins.SelectMany(p => p.Modules))
        {
            module.RegisterServices(ServiceCollection);
        }
    }
        
    // Registers all modules as a service that implements one or more
    // interfaces deriving from IPluginModuleService.
    private void RegisterPluginModulesAsService()
    {
        var modulesExposingServices = AllModules.OfType<IPluginModuleService>();

        foreach (IPluginModuleService module in modulesExposingServices)
        {
            var moduleType = module.GetType();
            var exposedServiceInterfaces = moduleType.GetInterfacesDerivedFrom<IPluginModuleService>();

            ServiceCollection.AddSingleton(exposedServiceInterfaces, module);
        }
    }

    // Registers the ICompositeApp component in the container representing the
    // application built from a set of plugins.
    private void RegisterCompositeApplication()
    {
        ServiceCollection.AddSingleton<ICompositeAppBuilder>(this);
        ServiceCollection.AddSingleton<ICompositeApp, CompositeApp>();
    }
}
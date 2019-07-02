using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Catalog;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Logging;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Responsible for registering the ICompositeApp instance built from
    /// a set of plugins registered by the application host.
    /// </summary>
    internal class CompositeAppBuilder : ICompositeAppBuilder
    {
        private readonly IList<IContainerConfig> _containerConfigs;
        
        public IConfiguration Configuration { get; }
        public IServiceCollection ServiceCollection { get; }
        
        // Plugins filtered by type:
        public IPlugin HostPlugin { get; private set; }
        public IPlugin[] AppPlugins { get; private set; }
        public IPlugin[] CorePlugins { get; private set; }

        public IPlugin[] AllPlugins { get; private set; } = Array.Empty<IPlugin>();
        public IPluginModule[] AllModules => AllPlugins.SelectMany(p => p.Modules).ToArray();
        
        public IBootstrapLogger BootstrapLogger { get; }
        public CompositeAppLog CompositeLog { get; private set; }
        
        public CompositeAppBuilder(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            _containerConfigs = new List<IContainerConfig>();
            
            ServiceCollection = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            BootstrapLogger = new BootstrapLogger();
        }
        
        //---------------------------------------------
        //--Initialization
        //---------------------------------------------
        
        public void ComposeModules(ITypeResolver typeResolver, IEnumerable<IPlugin> plugins)
        {
            if (typeResolver == null) throw new ArgumentNullException(nameof(typeResolver));
            if (plugins == null) throw new ArgumentNullException(nameof(plugins));

            CategorizePlugins(plugins);
            SetPluginAssemblyInfo(typeResolver);
            
            // Before composing the plugin modules, verify the plugins from which 
            // the composite application is being build.
            var validator = new CompositeAppValidation(AllPlugins);
            validator.Validate();

            ComposeModuleDependencies();
            
            // Allow each plug-in module to compose itself from concrete types, defined
            // by other plugins, based on abstract types it defines. 
            ComposeCorePlugins(typeResolver);
            ComposeApplicationPlugins(typeResolver);
            
            ConfigurePlugins();
        }

        public void AddContainerConfig(IContainerConfig containerConfig)
        {
            if (containerConfig == null) throw new ArgumentNullException(nameof(containerConfig));
            _containerConfigs.Add(containerConfig);
        }
        
        public T GetContainerConfig<T>() where T : IContainerConfig
        {
            var config = _containerConfigs.FirstOrDefault(c => c.GetType() == typeof(T));
            if (config == null)
            {
                throw new ContainerException(
                    $"Container configuration of type: {typeof(T)} is not registered.");
            }

            return (T)config;
        }

        private void CategorizePlugins(IEnumerable<IPlugin> plugins)
        {
            AllPlugins = plugins.ToArray();
            
            HostPlugin = AllPlugins.FirstOrDefault(p => p.PluginType == PluginTypes.HostPlugin);
            AppPlugins = AllPlugins.Where(p => p.PluginType == PluginTypes.ApplicationPlugin).ToArray();
            CorePlugins = AllPlugins.Where(p => p.PluginType == PluginTypes.CorePlugin).ToArray();
            
            LogPlugins(AllPlugins);
        }
        
        private void LogPlugins(IEnumerable<IPlugin> plugins)
        {
            foreach (var plugin in plugins.OrderBy(p => p.PluginType))
            {
                var details = new
                {
                    plugin.Name,
                    plugin.PluginId,
                    plugin.AssemblyName,
                    Configs = plugin.Configs.Select(c => c.GetType().FullName),
                    Modules = plugin.Modules.Select(m => m.GetType().FullName)
                };
                
                BootstrapLogger.Add(LogLevel.Debug, details.ToIndentedJson());
            }
        }
        
        // Delegates to the type resolver to populate information and the types associated with each plugin.
        // This decouples the container from runtime information and makes it easier to test.
        private void SetPluginAssemblyInfo(ITypeResolver typeResolver)
        {           
            foreach (IPlugin plugin in AllPlugins)
            {
                typeResolver.SetPluginMeta(plugin);
            }
        }
        
        //---------------------------------------------
        //--Module Dependencies
        //---------------------------------------------
        
        // A plugin module can reference another module by referencing any of its supported interfaces
        // deriving from IPluginModuleService.  Finds all IPluginModuleService derived properties of the
        // referencing module and set them corresponding referenced module instance.
        private void ComposeModuleDependencies()
        {
            BootstrapLogger.Add(LogLevel.Debug, 
                "Settings Plugin Module Properties Referencing other Plugin Modules.");
            
            foreach (IPluginModule module in AllModules)
            {
                var dependentServiceProps = GetDependentServiceProperties(module);
                foreach (PropertyInfo serviceProp in dependentServiceProps)
                {
                    IPluginModuleService dependentService = GetModuleSupportingService(serviceProp.PropertyType);
                    serviceProp.SetValue(module, dependentService);
                }
                
                LogModuleDependencies(module, dependentServiceProps);
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
        
        private IPluginModuleService GetModuleSupportingService(Type serviceType)
        {
            var foundModules = AllModules.Where(m => m.GetType().IsDerivedFrom(serviceType)).ToArray();
            if (! foundModules.Any())
            {
                throw new ContainerException($"Plug-in module of type: {serviceType} not found.");
            }
            
            if (foundModules.Length > 1)
            {
                throw new ContainerException($"Multiple plug-in modules implementing: {serviceType} found.");
            }

            return (IPluginModuleService)foundModules.First();
        }

        private void LogModuleDependencies(IPluginModule module, IEnumerable<PropertyInfo> moduleProps)
        {            
            foreach (PropertyInfo moduleProp in moduleProps)
            {
                BootstrapLogger.Add(LogLevel.Trace,
                    "Module: {moduleName} Property: {propName}:  References Module: {refModuleName}",
                    module.GetType().FullName,
                    moduleProp.Name,
                    moduleProp.PropertyType.FullName);
            }
        }
        
        
        //--------------------------------------------------
        //--Container Composition
        //--------------------------------------------------

        // Core plugins are composed from all other plugin types since they implement
        // reusable cross-cutting concerns.
        private void ComposeCorePlugins(ITypeResolver typeResolver)
        {
            BootstrapLogger.Add(LogLevel.Debug, 
                "Settings Plugin Module Properties Referencing Concrete Implementations.");
            
            var allPluginTypes = GetPluginTypes().ToArray();

            foreach (var plugin in CorePlugins)
            {
                typeResolver.ComposePlugin(plugin, allPluginTypes);
            }
        }

        // Application plugins contain a specific application's implementations
        // and are composed only from other application specific plugins.
        private void ComposeApplicationPlugins(ITypeResolver typeResolver)
        {
            var allAppPluginTypes = GetPluginTypes(
                PluginTypes.ApplicationPlugin, 
                PluginTypes.HostPlugin).ToArray();

            foreach (var plugin in AppPlugins)
            {
                typeResolver.ComposePlugin(plugin, allAppPluginTypes);
            }
            
            typeResolver.ComposePlugin(HostPlugin, allAppPluginTypes);
        }
        
        /// <summary>
        /// Returns types associated with a specific category of plug-in.
        /// </summary>
        /// <param name="pluginTypes">The category of plug-ins to limit the return types.</param>
        /// <returns>List of limited plug in types or all plug-in types if no category is specified.</returns>
        public IEnumerable<Type> GetPluginTypes(params PluginTypes[] pluginTypes)
        {
            if (pluginTypes == null) throw new ArgumentNullException(nameof(pluginTypes),
                "List of Plug-in types cannot be null.");

            return pluginTypes.Length == 0 ? AllPlugins.SelectMany(p => p.Types) 
                : AllPlugins.Where(p => pluginTypes.Contains(p.PluginType)).SelectMany(p => p.Types);
        }
        
        
        //--------------------------------------------------
        //--Module Initialization and Configuration
        //--------------------------------------------------
        
        // Before services are registered by modules, a two phase initialization is completed.
        // First all modules have the Initialize() method called.  The Initialize() method is
        // where plugins should cache information that can be accessed my other modules.
        // After all modules are initialized, each module has The Configure() method called.
        // Code contained within a module's Configure() method can reference information
        // initialized by a dependent module.
        private void ConfigurePlugins()
        {
            CorePlugins.ForEach(ConfigureModules);
            AppPlugins.ForEach(ConfigureModules);
            
            ConfigureModules(HostPlugin);
        }

        private void ConfigureModules(IPlugin plugin)
        {
            foreach (IPluginModule module in plugin.Modules)
            {
                module.Context = new ModuleContext(this, plugin);
                module.Initialize();
            }
            
            foreach (IPluginModule module in plugin.Modules)
            {
                module.Configure();
            }
        }
        
        //--------------------------------------------------
        //--Module Service Registrations
        //--------------------------------------------------
   
        // Allows each module to register services that can be injected into
        // other components.  These services expose functionality implemented
        // by a plugin.
        public void RegisterServices(IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
           
            RegisterDefaultPluginServices(services);
            ScanPluginsForServices(services);
            RegisterPluginServices(services);

            RegisterPluginModuleServices(services);
            RegisterCompositeApplication(services);
            
            CreateCompositeLogger(services);
        }
 
        // Next, allow all plugin modules to register any default services
        // implementations that can be optionally overridden by other plug-ins.  
        private void RegisterDefaultPluginServices(IServiceCollection services)
        {
            foreach (IPluginModule module in AllPlugins.SelectMany(p => p.Modules))
            {
                module.RegisterDefaultServices(services);
            }
        }
        
        // Allow each plugin module to scan for plugin types.  
        private void ScanPluginsForServices(IServiceCollection services)
        {
            foreach (IPlugin plugin in AllPlugins)
            {
                var sourceTypes = FilteredTypesByPluginType(plugin);
                var typeCatalog = services.CreateCatalog(sourceTypes);

                foreach (IPluginModule module in plugin.Modules)
                {
                    module.ScanPlugins(typeCatalog);
                }
            }
        }

        // Lastly, allow plugin modules to register services specific to their implementation.
        private void RegisterPluginServices(IServiceCollection services)
        {
            foreach (IPluginModule module in AllPlugins.SelectMany(p => p.Modules))
            {
                module.RegisterServices(services);
            }
        }
        
        private IEnumerable<Type> FilteredTypesByPluginType(IPlugin plugin)
        {
            // Core plug-in can access types from all other plug-in types.
            if (plugin.PluginType == PluginTypes.CorePlugin)
            {
                return AllPlugins.SelectMany(p => p.Types);
            }

            // Application centric plug-in can only access types contained in
            // other application plugs.
            return AppPlugins.SelectMany(p => p.Types);
        }
        
        private void RegisterPluginModuleServices(IServiceCollection services)
        {
            var modulesWithServices = AllModules.OfType<IPluginModuleService>();

            foreach (IPluginModuleService moduleService in modulesWithServices)
            {
                var moduleServiceType = moduleService.GetType();
                var moduleServiceInterfaces = moduleServiceType.GetInterfacesDerivedFrom<IPluginModuleService>();

                services.AddSingleton(moduleServiceInterfaces, moduleService);
            }
        }

        // Registers the ICompositeApp component in the container representing the
        // application build for a set of plugins.
        private void RegisterCompositeApplication(IServiceCollection services)
        {
            services.AddSingleton<ICompositeAppBuilder>(this);
            services.AddSingleton<ICompositeApp, CompositeApp>();
        }
        
        private void CreateCompositeLogger(IServiceCollection serviceCollection)
        {
            CompositeLog = new CompositeAppLog(this, serviceCollection);
        }
    }
}
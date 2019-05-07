using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Dependencies;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Application composed from a set of plugins.  The end result of the composite application
    /// is a configured .net core Service Collection from which a Service Provider can be built.
    /// </summary>
    public class CompositeApp
    {
        public bool IsStarted { get; private set; }
        
        public IPlugin[] AllPlugins { get; }
        public IEnumerable<IPluginModule> AllModules => AllPlugins.SelectMany(p => p.Modules);
        
        // Reference to the plugin associated with the host executable.
        public IPlugin HostPlugin { get; private set; }
        
        // Plugins from which the host application is built.
        public IPlugin[] AppPlugins { get; private set; }
        
        // Reusable plugins that can be used across multiple applications.
        public IPlugin[] CorePlugins { get; private set; }
        
        // Microsoft Abstractions:
        public ILoggerFactory LoggerFactory { get; }
        public IConfiguration Configuration { get; }
        
        public CompositeApp(
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IEnumerable<IPlugin> plugins)
        {
            LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            
            if (plugins == null) throw new ArgumentNullException(nameof(plugins));
            
            AllPlugins = plugins.ToArray();
        }

        public void Initialize()
        {
            HostPlugin = AllPlugins.FirstOrDefault(p => p.PluginType == PluginTypes.HostPlugin);
            AppPlugins = AllPlugins.Where(p => p.PluginType == PluginTypes.ApplicationPlugin).ToArray();
            CorePlugins = AllPlugins.Where(p => p.PluginType == PluginTypes.CorePlugin).ToArray();

            var validator = new CompositeAppValidation(this);
            validator.Validate();

            // If the composite structure is valid, compose each plugin module.
            // This is process each module and set any dependent plugin services.
            ComposePluginModules();
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
        
        //------------------------------------ Plugin Module Services ------------------------------//
        
        private void ComposePluginModules()
        {
            foreach (IPluginModule module in AllModules)
            {
                var dependentServiceProps = GetDependentServiceProperties(module);
                foreach (PropertyInfo serviceProp in dependentServiceProps)
                {
                    IPluginModuleService dependentService = GetModuleSupportingService(serviceProp.PropertyType);
                    serviceProp.SetValue(module, dependentService);
                }
            }
        }
        
        private static IEnumerable<PropertyInfo> GetDependentServiceProperties(IPluginModule module)
        {
            const BindingFlags bindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            return module.GetType().GetProperties(bindings)
                .Where(p =>
                    p.PropertyType.IsDerivedFrom(typeof(IPluginModuleService))
                    && p.CanWrite);
        }
        
        private IPluginModuleService GetModuleSupportingService(Type serviceType)
        {
            var foundModules = AllModules.Where(m => m.GetType().IsDerivedFrom(serviceType)).ToArray();
            if (!foundModules.Any())
            {
                throw new ContainerException($"Plug-in module of type: {serviceType} not found.");
            }
            
            if (foundModules.Length > 1)
            {
                throw new ContainerException($"Multiple plug-in modules implementing: {serviceType} found.");
            }

            return (IPluginModuleService)foundModules.First();
        }
        
        
        //------------------------------------ Plugin Service Population ------------------------------//

        public void PopulateServices(IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            
            ConfigurePlugins();

            RegisterDefaultPluginServices(services);
            ScanPluginsForServices(services);
            RegisterPluginServices(services);
        }

        private void ConfigurePlugins()
        {
            CorePlugins.ForEach(ConfigureModules);
            AppPlugins.ForEach(ConfigureModules);
            
            ConfigureModules(HostPlugin);
        }

        // First Initializes all plugin modules then invokes their Configure method.
        private void ConfigureModules(IPlugin plugin)
        {
            foreach (IPluginModule module in plugin.Modules)
            {
                module.Context = new ModuleContext(this, plugin, module);
                module.Initialize();
            }
            
            foreach (IPluginModule module in plugin.Modules)
            {
                module.Configure();
            }
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
        
        //------------------------------------ Plugin Service Execution ------------------------------//
        
        /// <summary>
        /// This is the last step of the bootstrap process.   
        /// </summary>
        /// <param name="services">Scoped service provider.</param>
        public void StartPluginModules(IServiceProvider services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services),
                "Services cannot be null.");

            // Start the plug-in modules in dependent order starting with core modules 
            // and ending with the application host modules.
            IsStarted = true;
     
            StartPluginModules(services, CorePlugins);
            StartPluginModules(services, AppPlugins);
            StartPluginModules(services, new[] { HostPlugin });

            // Last phase to allow any modules to execute any processing that
            // might be dependent on another module being started.
            foreach (IPluginModule module in AllModules)
            {
                module.RunModule(services);
            }
        }

        private static void StartPluginModules(IServiceProvider services, IEnumerable<IPlugin> plugins)
        {
            foreach (IPluginModule module in plugins.SelectMany(p => p.Modules))
            {
                module.StartModule(services);
            }
        }
        
        /// <summary>
        /// Stops all plug-in modules in the reverse order from which they were started.
        /// </summary>
        /// <param name="services">Scoped service provider.</param>
        public void StopPluginModules(IServiceProvider services)
        {
            StopPluginModules(services, new[] { HostPlugin });
            StopPluginModules(services, AppPlugins);
            StopPluginModules(services, CorePlugins);
    
            IsStarted = false;
        }

        private static void StopPluginModules(IServiceProvider services, IEnumerable<IPlugin> plugins)
        {
            foreach (IPluginModule module in plugins.SelectMany(p => p.Modules))
            {
                module.StopModule(services);
            }
        }
    }
}
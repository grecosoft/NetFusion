using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Dependencies;
using NetFusion.Bootstrap.Manifests;
using NetFusion.Bootstrap.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Represents an application that is composed of plug-ins that are used
    /// to populate Microsoft's abstract service collection with services that
    /// can be dependency injected into dependent components.
    /// </summary>
    public class CompositeApplication
    {
        public bool IsStarted { get; private set; }

        public IConfiguration Configuration { get; }
        public ILoggerFactory LoggerFactory { get; }

        public CompositeApplication(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        /// <summary>
        /// Object instances representing each discovered plug-in.
        /// </summary>
        public Plugin[] Plugins { get; set; } = Array.Empty<Plugin>();

        /// <summary>
        /// The application process hosting the application container.
        /// This will be a WebApi application or a Console executable.
        /// </summary>
        public Plugin AppHostPlugin => 
            Plugins.First(p => p.Manifest is IAppHostPluginManifest);

        /// <summary>
        /// All plug-ins containing application components that are specific to the application.
        /// These plug-ins will contain Domain Entities, Services, and Repositories, etc...
        /// </summary>
        public IEnumerable<Plugin> AppComponentPlugins => 
            Plugins.Where(p => p.Manifest is IAppComponentPluginManifest);

        /// <summary>
        /// All plug-ins containing core components that are generic and reusable across applications.
        /// These plug-ins often implement cross-cutting concerns.
        /// </summary>
        public IEnumerable<Plugin> CorePlugins => 
            Plugins.Where(p => p.Manifest is ICorePluginManifest);

        /// <summary>
        /// Returns types associated with a specific category of plug-in.
        /// </summary>
        /// <param name="pluginTypes">The category of plug-ins to limit the return types.</param>
        /// <returns>List of limited plug in types or all plug-in types if no category is specified.</returns>
        public IEnumerable<PluginType> GetPluginTypes(params PluginTypes[] pluginTypes)
        {
            if (pluginTypes == null) throw new ArgumentNullException(nameof(pluginTypes),
                "List of Plug-in types cannot be null.");

            if (pluginTypes.Length == 0)
            {
                return Plugins.SelectMany(p => p.PluginTypes);
            }

            return Plugins.SelectMany(p => p.PluginTypes)
                .Where(pt => pluginTypes.Contains(pt.Plugin.PluginType));
        }

        /// <summary>
        /// List of all modules defined within all plug-ins.
        /// </summary>
        public IEnumerable<IPluginModule> AllPluginModules => Plugins.SelectMany(p => p.Modules);


        //------------------------------------------ Plug-in Component Registration ------------------------------------------//

        /// <summary>
        /// Populates the service collection with services registered by plug-in modules.
        /// </summary>
        /// <param name="services">The service collection to be populated.</param>
        public void PopulateServices(IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services), "Service Collection not specified.");

            InitializePluginModules();

            // Note that the order is important.  If a service type is registered more than once,
            // the last registered component is used.  This allows application plug-in services
            // to override core defines services.
            RegisterDefaultPluginServices(services);
            RegisterCorePluginServices(services);
            RegisterAppPluginServices(services);
        }

        // Populates the context that can be referenced by each plug-in module.
        private void InitializePluginModules()
        {
            InitializePluginModules(CorePlugins);
            InitializePluginModules(AppComponentPlugins);
            InitializePluginModules(new[] { AppHostPlugin });
        }

        private void InitializePluginModules(IEnumerable<Plugin> plugins)
        {
            foreach (Plugin plugin in plugins)
            {
                foreach (IPluginModule module in plugin.Modules)
                {
                    //ımodule.Context = new ModuleContext(this, plugin, module);
                    module.Initialize();
                }

                foreach (IPluginModule module in plugin.Modules)
                {
                    module.Configure();
                }
            }
        }

        // First allow all plug-ins to register any default services implementations
        // that can be optionally overridden by other plug-ins.  
        private void RegisterDefaultPluginServices(IServiceCollection services)
        {
            foreach (IPluginModule module in AllPluginModules)
            {
                module.RegisterDefaultServices(services);
            }
        }

        private void RegisterCorePluginServices(IServiceCollection services)
        {
            IEnumerable<PluginType> allPluginTypes = GetPluginTypes().ToArray();
            foreach (Plugin plugin in CorePlugins)
            {
                ScanPluginForServices(plugin, services);
                RegisterServices(plugin, services);

                // Core modules may override one or both of the following depending 
                // on the scope of the search.
                ScanAllOtherPluginsForServices(plugin, services, allPluginTypes);
                ScanApplicationPluginsForServices(plugin, services);
            }
        }

        // Allows for each plug-in module to scan *its* types for any
        // service components to be registered in the Service Collection.
        private static void ScanPluginForServices(Plugin plugin, IServiceCollection services)
        {
            var typeCatalog = services.CreateCatalog(plugin.PluginTypes);
            foreach (IPluginModule module in plugin.Modules)
            {
                module.ScanPlugin(typeCatalog);
            }
        }

        // Allows the each plug-in module to manually register
        // any needed service components with the Service Collection.
        private static void RegisterServices(Plugin plugin, IServiceCollection services)
        {
            foreach (IPluginModule module in plugin.Modules)
            {
                module.RegisterServices(services);
            }
        }

        // Allows a plug-in to scan all specified plug-in types, *excluding* types
        // defined within *it's* plug-in, for components to be registered in the
        // Service Collection.
        private static void ScanAllOtherPluginsForServices(Plugin plugin, IServiceCollection services,
             IEnumerable<PluginType> sourceTypes)
        {
            var typeCatalog = services.CreateCatalog(sourceTypes.Except(plugin.PluginTypes));
            foreach (IPluginModule module in plugin.Modules)
            {
                module.ScanAllOtherPlugins(typeCatalog);
            }
        }
        
        private void ScanApplicationPluginsForServices(Plugin plugin, IServiceCollection services)
        {
            IEnumerable<PluginType> appPluginTypes = GetPluginTypes(PluginTypes.AppComponentPlugin, PluginTypes.AppHostPlugin);
            var typeCatalog = services.CreateCatalog(appPluginTypes);

            foreach (var module in plugin.Modules)
            {
                module.ScanApplicationPlugins(typeCatalog);
            }
        }

        private void RegisterAppPluginServices(IServiceCollection services)
        {
            IEnumerable<PluginType> allAppPluginTypes = GetPluginTypes(
                PluginTypes.AppComponentPlugin, 
                PluginTypes.AppHostPlugin).ToArray();

            // Application Components:
            foreach (Plugin plugin in AppComponentPlugins)
            {
                ScanPluginForServices(plugin, services);
                RegisterServices(plugin, services);
                ScanAllOtherPluginsForServices(plugin, services, allAppPluginTypes);
            }

            // Application Host:
            ScanPluginForServices(AppHostPlugin, services);
            RegisterServices(AppHostPlugin, services);
            ScanAllOtherPluginsForServices(AppHostPlugin, services, allAppPluginTypes);
        }

        //------------------------------------------ Start Plug-in Modules ------------------------------------------//

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
            StartPluginModules(services, AppComponentPlugins);
            StartPluginModules(services, new[] { AppHostPlugin });

            // Last phase to allow any modules to execute any processing that
            // might be dependent on another module being started.
            foreach (IPluginModule module in AllPluginModules)
            {
                module.RunModule(services);
            }
        }

        private static void StartPluginModules(IServiceProvider services, IEnumerable<Plugin> plugins)
        {
            foreach (IPluginModule module in plugins.SelectMany(p => p.Modules))
            {
                module.StartModule(services);
            }
        }

        //------------------------------------------Stop Plug-in Modules------------------------------------------//

        /// <summary>
        /// Stops all plug-in modules in the reverse order from which they were started.
        /// </summary>
        /// <param name="services">Scoped service provider.</param>
        public void StopPluginModules(IServiceProvider services)
        {
            StopPluginModules(services, new[] { AppHostPlugin });
            StopPluginModules(services, AppComponentPlugins);
            StopPluginModules(services, CorePlugins);
    
            IsStarted = false;
        }

        private static void StopPluginModules(IServiceProvider services, IEnumerable<Plugin> plugins)
        {
            foreach (IPluginModule module in plugins.SelectMany(p => p.Modules))
            {
                module.StopModule(services);
            }
        }
    }
}

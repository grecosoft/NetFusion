using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Dependencies;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;

namespace NetFusion.Bootstrap.Refactors
{
    public class CompositeApp
    {
        public bool IsStarted { get; private set; }
        
        public IPluginDefinition[] AllPlugins { get; }
        
        public IPluginDefinition HostPlugin { get; private set; }
        public IPluginDefinition[] AppPlugins { get; private set; }
        public IPluginDefinition[] CorePlugins { get; private set; }
        
        public ILoggerFactory LoggerFactory { get; }
        public IConfiguration Configuration { get; }

        public IEnumerable<IPluginModule> AllModules => AllPlugins.SelectMany(p => p.Modules);
        
        public CompositeApp(
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IEnumerable<IPluginDefinition> plugins)
        {
            LoggerFactory = loggerFactory;
            Configuration = configuration;
            AllPlugins = plugins.ToArray();
        }

        public void Validate()
        {
            HostPlugin = AllPlugins.First(p => p.PluginType == PluginDefinitionTypes.HostPlugin);
            AppPlugins = AllPlugins.Where(p => p.PluginType == PluginDefinitionTypes.ApplicationPlugin).ToArray();
            CorePlugins = AllPlugins.Where(p => p.PluginType == PluginDefinitionTypes.CorePlugin).ToArray();
        }
        
        /// <summary>
        /// Returns types associated with a specific category of plug-in.
        /// </summary>
        /// <param name="pluginTypes">The category of plug-ins to limit the return types.</param>
        /// <returns>List of limited plug in types or all plug-in types if no category is specified.</returns>
        public IEnumerable<Type> GetPluginTypes(params PluginDefinitionTypes[] pluginTypes)
        {
            if (pluginTypes == null) throw new ArgumentNullException(nameof(pluginTypes),
                "List of Plug-in types cannot be null.");

            return pluginTypes.Length == 0 ? AllPlugins.SelectMany(p => p.Types) 
                : AllPlugins.Where(p => pluginTypes.Contains(p.PluginType)).SelectMany(p => p.Types);
        }
        
        //------------------------------------ Plugin Service Population ------------------------------//

        public void PopulateServices(IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            
            ConfigurePlugins();

            RegisterDefaultPluginServices(services);
            ScanPluginForServices(services);
            RegisterPluginServices(services);
        }

        private void ConfigurePlugins()
        {
            CorePlugins.ForEach(ConfigureModules);
            AppPlugins.ForEach(ConfigureModules);
            
            ConfigureModules(HostPlugin);
        }

        private void ConfigureModules(IPluginDefinition plugin)
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
        
        // First allow all plug-ins to register any default services implementations
        // that can be optionally overridden by other plug-ins.  
        private void RegisterDefaultPluginServices(IServiceCollection services)
        {
            foreach (IPluginModule module in AllPlugins.SelectMany(p => p.Modules))
            {
                module.RegisterDefaultServices(services);
            }
        }

        private void RegisterPluginServices(IServiceCollection services)
        {
            foreach (IPluginModule module in AllPlugins.SelectMany(p => p.Modules))
            {
                module.RegisterServices(services);
            }
        }
        
        private void ScanPluginForServices(IServiceCollection services)
        {
            foreach (IPluginDefinition plugin in AllPlugins)
            {
                var sourceTypes = FilteredTypesByPluginType(plugin);
                var typeCatalog = services.CreateCatalog(sourceTypes);

                foreach (IPluginModule module in plugin.Modules)
                {
                    module.ScanPlugin(typeCatalog);
                }
            }
        }
        
        private IEnumerable<Type> FilteredTypesByPluginType(IPluginDefinition plugin)
        {
            // Core plug-in can access types from all other plug-in types.
            if (plugin.PluginType == PluginDefinitionTypes.CorePlugin)
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

        private static void StartPluginModules(IServiceProvider services, IEnumerable<IPluginDefinition> plugins)
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

        private static void StopPluginModules(IServiceProvider services, IEnumerable<IPluginDefinition> plugins)
        {
            foreach (IPluginModule module in plugins.SelectMany(p => p.Modules))
            {
                module.StopModule(services);
            }
        }
    }
}
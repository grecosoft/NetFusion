using Autofac;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Manifests;
using NetFusion.Bootstrap.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Represents an application that is composed of plug-ins that are used
    /// to create a run-time environment and dependency injection container.
    /// </summary>
    public class CompositeApplication
    {
        public bool IsStarted { get; private set; }

        internal ILoggerFactory LoggerFactory { get; set; }

        /// <summary>
        /// Object instances representing each discovered plug-in.
        /// </summary>
        public Plugin[] Plugins { get; internal set; } = Array.Empty<Plugin>();

        /// <summary>
        /// The application process hosting the application container.
        /// This will be a WebApi application or a Console executable.
        /// </summary>
        public Plugin AppHostPlugin
        {
            get { return Plugins.First(p => p.Manifest is IAppHostPluginManifest); }
        }

        /// <summary>
        /// All plug-ins containing application components that are specific to the application.
        /// These plug-ins will contain Domain Entities, Services, and Repositories.
        /// </summary>
        public IEnumerable<Plugin> AppComponentPlugins
        {
            get { return Plugins.Where(p => p.Manifest is IAppComponentPluginManifest); }
        }

        /// <summary>
        /// All plug-ins containing core components that are generic and reusable across applications.
        /// These plug-ins often implement cross-cutting concerns.
        /// </summary>
        public IEnumerable<Plugin> CorePlugins
        {
            get { return Plugins.Where(p => p.Manifest is ICorePluginManifest); }
        }

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
        public IEnumerable<IPluginModule> AllPluginModules
        {
            get { return Plugins.SelectMany(p => p.Modules); }
        }

        //------------------------------------------ Plug-in Component Registration ------------------------------------------//

        /// <summary>
        /// Populates the dependency injection container with services registered by plug-in modules.
        /// </summary>
        /// <param name="builder">The DI container builder.</param>
        public void RegisterComponents(Autofac.ContainerBuilder builder)
        {            
            if (builder == null) throw new ArgumentNullException(nameof(builder), 
                "DI Container cannot be null.");            

            InitializePluginModules();

            // Note that the order is important.  In Autofac, if a service type 
            // is registered more than once, the last registered component is
            // used.  This is the default configuration.
            RegisterDefaultPluginComponents(builder);
            RegisterCorePluginComponents(builder);
            RegisterAppPluginComponents(builder);
        }

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
                foreach (IPluginModule module in plugin.IncludedModules)
                {
                    module.Context = new ModuleContext(LoggerFactory, this, plugin);
                    module.Initialize();
                }

                foreach (IPluginModule module in plugin.IncludedModules)
                {
                    module.Configure();
                }
            }
        }

        // First allow all plug-ins to register any default component implementations
        // that can be optionally overridden by other plug-ins.  This will be the
        // component instances that will be used if not overridden.  
        // Null implementations can be registered here.
        private void RegisterDefaultPluginComponents(Autofac.ContainerBuilder builder)
        {
            foreach (IPluginModule module in AllPluginModules)
            {
                module.RegisterDefaultComponents(builder);
            }
        }

        private void RegisterCorePluginComponents(Autofac.ContainerBuilder builder)
        {
            IEnumerable<PluginType> allPluginTypes = GetPluginTypes();
            foreach (Plugin plugin in CorePlugins)
            {
                ScanPluginTypes(plugin, builder);
                RegisterComponents(plugin, builder);

                // Core modules may override one or both of the following depending 
                // on the scope of the search.
                ScanAllOtherPluginTypes(plugin, builder, allPluginTypes);
                ScanOnlyApplicationPluginTypes(plugin, builder);
            }
        }

        // Allows for each plug-in module to scan its types for any
        // service components to be registered in the Autofac Container.
        private void ScanPluginTypes(Plugin plugin, Autofac.ContainerBuilder builder)
        {
            var typeRegistration = new TypeRegistration(builder, plugin.PluginTypes);
            foreach (IPluginModule module in plugin.IncludedModules)
            {
                module.ScanPlugin(typeRegistration);
            }
        }

        // Allows the each plug-in module to manually register
        // any needed service components with the Autofac Container.
        private void RegisterComponents(Plugin plugin, Autofac.ContainerBuilder builder)
        {
            foreach (IPluginModule module in plugin.IncludedModules)
            {
                module.RegisterComponents(builder);
            }
        }

        // Allows a plug-in to scan all specified plug-in types, excluding types
        // defined within it's plug-in, for components to be registered in the
        // Autofac container.
        private void ScanAllOtherPluginTypes(Plugin plugin, Autofac.ContainerBuilder builder,
            IEnumerable<PluginType> sourceTypes)
        {
            var typeRegistration = new TypeRegistration(
                builder,
                sourceTypes.Except(plugin.PluginTypes));

            foreach (IPluginModule module in plugin.IncludedModules)
            {
                module.ScanAllOtherPlugins(typeRegistration);
            }
        }

        private void ScanOnlyApplicationPluginTypes(Plugin plugin, Autofac.ContainerBuilder builder)
        {
            IEnumerable<PluginType> appPluginTypes = GetPluginTypes(PluginTypes.AppComponentPlugin, PluginTypes.AppHostPlugin);

            var typeRegistration = new TypeRegistration(
                builder,
                appPluginTypes);

            foreach (var module in plugin.IncludedModules)
            {
                module.ScanApplicationPlugins(typeRegistration);
            }
        }

        private void RegisterAppPluginComponents(Autofac.ContainerBuilder builder)
        {
            IEnumerable<PluginType> allAppPluginTypes = GetPluginTypes(PluginTypes.AppComponentPlugin, PluginTypes.AppHostPlugin);

            // Application Components:
            foreach (Plugin plugin in AppComponentPlugins)
            {
                ScanPluginTypes(plugin, builder);
                RegisterComponents(plugin, builder);
                ScanAllOtherPluginTypes(plugin, builder, allAppPluginTypes);
            }

            // Application Host:
            ScanPluginTypes(AppHostPlugin, builder);
            RegisterComponents(AppHostPlugin, builder);
            ScanAllOtherPluginTypes(AppHostPlugin, builder, allAppPluginTypes);
        }

        //------------------------------------------ Start Plug-in Modules ------------------------------------------//

        /// <summary>
        /// This is the last step of the bootstrap process.  Each module is passed the instance of 
        /// the created container so that it can start any runtime services requiring the container.
        /// </summary>
        /// <param name="container">The built container.</param>
        public void StartPluginModules(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container), 
                "DI Container cannot be null.");

            // Start the plug-in modules in dependent order starting with core modules 
            // and ending with the application host modules.
            IsStarted = true;

            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                StartPluginModules(container, scope, CorePlugins);
                StartPluginModules(container, scope, AppComponentPlugins);
                StartPluginModules(container, scope, new[] { AppHostPlugin });

                // Last phase to allow any modules to execute any processing that
                // might be dependent on another module being started.
                foreach (IPluginModule module in Plugins.SelectMany(p => p.IncludedModules))
                {
                    module.RunModule(scope);
                }
            }
        }

        private void StartPluginModules(IContainer container, ILifetimeScope scope, IEnumerable<Plugin> plugins)
        {
            foreach (IPluginModule module in plugins.SelectMany(p => p.IncludedModules))
            {
                module.StartModule(container, scope);
            }
        }

        //------------------------------------------Stop Plug-in Modules------------------------------------------//

        /// <summary>
        /// Stops all plug-in modules in the reverse order from which they were started.
        /// </summary>
        /// <param name="container">The build container.</param>
        public void StopPluginModules(IContainer container)
        {
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                StopPluginModules(scope, new[] { AppHostPlugin });
                StopPluginModules(scope, AppComponentPlugins);
                StopPluginModules(scope, CorePlugins);
            }

            IsStarted = false;
        }

        private void StopPluginModules(ILifetimeScope scope, IEnumerable<Plugin> plugins)
        {
            foreach (IPluginModule module in plugins.SelectMany(p => p.IncludedModules))
            {
                module.StopModule(scope);
            }
        }
    }
}

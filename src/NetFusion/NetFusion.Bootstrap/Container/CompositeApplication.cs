using Autofac;
using NetFusion.Bootstrap.Extensions;
using NetFusion.Bootstrap.Logging;
using NetFusion.Bootstrap.Manifests;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common;
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
        public string[] SearchPatterns { get; }

        internal IContainerLogger Logger { get; set; } 

        public CompositeApplication(string[] searchPatterns)
        {
            Check.NotNull(searchPatterns, nameof(searchPatterns));

            this.SearchPatterns = searchPatterns;
        }

        public Plugin[] Plugins { get; set; }

        public Plugin AppHostPlugin
        {
            get { return this.Plugins.First(p => p.Manifest is IAppHostPluginManifest); }
        }

        public IEnumerable<Plugin> AppComponentPlugins
        {
            get { return this.Plugins.Where(p => p.Manifest is IAppComponentPluginManifest); }
        }

        public IEnumerable<Plugin> CorePlugins
        {
            get { return this.Plugins.Where(p => p.Manifest is ICorePluginManifest); }
        }

        /// <summary>
        /// Returns types associated with a specific category of plug-in.
        /// </summary>
        /// <param name="pluginTypes">The category of plug-ins to limit the return types.</param>
        /// <returns>List of limited plug in types or all plug-in types if no category is specified.</returns>
        public IEnumerable<PluginType> GetPluginTypesFrom(params PluginTypes[] pluginTypes)
        {
            Check.NotNull(pluginTypes, nameof(pluginTypes));

            if (pluginTypes.Length == 0)
            {
                return this.Plugins.SelectMany(p => p.PluginTypes);
            }

            return this.Plugins.SelectMany(p => p.PluginTypes)
                .Where(pt => pluginTypes.Contains(pt.Plugin.PluginType));
        }

        public IEnumerable<IPluginModule> AllPluginModules
        {
            get { return this.Plugins?.SelectMany(p => p.PluginModules); }
        }

        /// <summary>
        /// Populates the dependency injection container with services
        /// registered by plug-in modules.
        /// </summary>
        /// <param name="builder">The DI container builder.</param>
        public void RegisterComponents(Autofac.ContainerBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            InitializePluginModules();

            // Note that the order is important.  In Autofac, if a service type 
            // is registered more than once, the last registered component is
            // used.  This is the default configuration.
            RegisterCorePluginComponents(builder);
            RegisterAppPluginComponents(builder);
        }

        private void InitializePluginModules()
        {
            InitializePluginModules(this.CorePlugins);
            InitializePluginModules(this.AppComponentPlugins);
            InitializePluginModules(new[] { this.AppHostPlugin });
        }

        private void InitializePluginModules(IEnumerable<Plugin> plugins)
        {
            foreach (Plugin plugin in plugins)
            {
                foreach (IPluginModule module in plugin.IncludedModules())
                {
                    module.Context = new ModuleContext(this.Logger, this, plugin);
                    module.Initialize();
                }

                foreach (IPluginModule module in plugin.IncludedModules())
                {
                    module.Configure();
                }
            }
        }

        private void RegisterCorePluginComponents(Autofac.ContainerBuilder builder)
        {
            IEnumerable<PluginType> allPluginTypes = GetPluginTypesFrom();
            foreach (Plugin plugin in this.CorePlugins)
            {
                ScanPluginTypes(plugin, builder);
                RegisterComponents(plugin, builder);

                // Core modules may override one of the following depending 
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
            foreach (IPluginModule module in plugin.IncludedModules())
            {
                module.ScanPlugin(typeRegistration);
            }
        }

        // Allows the each plug-in module to manually register
        // any needed service components with the Autofac Container.
        private void RegisterComponents(Plugin plugin, Autofac.ContainerBuilder builder)
        {
            foreach (IPluginModule module in plugin.IncludedModules())
            {
                module.RegisterComponents(builder);
            }
        }

        // Allows a plug-in to scan all specified plug-in types, excluding types
        // defined within it's plug-in, for components to be registered in the
        // Autofac container.
        private void ScanAllOtherPluginTypes(Plugin plugin,
            Autofac.ContainerBuilder builder,
            IEnumerable<PluginType> sourceTypes)
        {
            var typeRegistration = new TypeRegistration(
                builder,
                sourceTypes.Except(plugin.PluginTypes));

            foreach (IPluginModule module in plugin.IncludedModules())
            {
                module.ScanAllOtherPlugins(typeRegistration);
            }
        }

        private void ScanOnlyApplicationPluginTypes(Plugin plugin,
            Autofac.ContainerBuilder builder)
        {
            IEnumerable<PluginType> appPluginTypes = GetPluginTypesFrom(PluginTypes.AppComponentPlugin, PluginTypes.AppHostPlugin);

            var typeRegistration = new TypeRegistration(
                builder,
                appPluginTypes);

            foreach (var module in plugin.IncludedModules())
            {
                module.ScanApplicationPluginTypes(typeRegistration);
            }
        }

        private void RegisterAppPluginComponents(Autofac.ContainerBuilder builder)
        {
            IEnumerable<PluginType> allAppPluginTypes = GetPluginTypesFrom(PluginTypes.AppComponentPlugin, PluginTypes.AppHostPlugin);

            // Application Components:
            foreach (Plugin plugin in this.AppComponentPlugins)
            {
                ScanPluginTypes(plugin, builder);
                RegisterComponents(plugin, builder);
                ScanAllOtherPluginTypes(plugin, builder, allAppPluginTypes);
            }

            // Application Host:
            ScanPluginTypes(this.AppHostPlugin, builder);
            RegisterComponents(this.AppHostPlugin, builder);
            ScanAllOtherPluginTypes(this.AppHostPlugin, builder, allAppPluginTypes);
        }

        /// <summary>
        /// This is the last step of the bootstrap process.  Each module
        /// is passed the instance of the created container so that it
        /// can enable any runtime services requiring the container.
        /// </summary>
        /// <param name="container">The built container.</param>
        public void StartPluginModules(IContainer container)
        {
            Check.NotNull(container, nameof(container));

            // Start the plug-in modules in dependent order starting with core modules 
            // and ending with the application host modules.
            this.IsStarted = true;

            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                StartPluginModules(scope, this.CorePlugins);
                StartPluginModules(scope, this.AppComponentPlugins);
                StartPluginModules(scope, new[] { this.AppHostPlugin });
               
                foreach (IPluginModule module in this.Plugins.SelectMany(p => p.IncludedModules()))
                {
                    module.RunModule(scope);
                }
            }
        }

        private void StartPluginModules(ILifetimeScope scope, IEnumerable<Plugin> plugins)
        {
            foreach (IPluginModule module in plugins.SelectMany(p => p.IncludedModules()))
            {
                module.StartModule(scope);
            }
        }

        /// <summary>
        /// Stops all plug-in modules in the reverse order in which they were started.
        /// </summary>
        /// <param name="container">The build container.</param>
        public void StopPluginModules(IContainer container)
        {
            using (ILifetimeScope scope = container.BeginLifetimeScope())
            {
                StopPluginModules(scope, new[] { this.AppHostPlugin });
                StopPluginModules(scope, this.AppComponentPlugins);
                StopPluginModules(scope, this.CorePlugins);
            }

            this.IsStarted = false;
        }

        private void StopPluginModules(ILifetimeScope scope, IEnumerable<Plugin> plugins)
        {
            foreach (var module in plugins.SelectMany(p => p.IncludedModules()))
            {
                module.StopModule(scope);
            }
        }  
    }
}

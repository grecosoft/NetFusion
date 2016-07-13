using Autofac;
using NetFusion.Bootstrap.Extensions;
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
    internal class CompositeApplication 
    {
        public bool IsStarted { get; private set; }
        public string[] SearchPatterns { get; }

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
        /// <param name="pluginTypes">The category of plug-ins to limit the
        /// return types.</param>
        /// <returns>List of limited plug in types or all plug-in types if no
        /// category is specified.</returns>
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

            // Note that the order is important.  In Autofac, if a service type 
            // is registered more than once, the last registered component is
            // used.  This is the default configuration.
            InitializePluginModules();
            RegisterCorePluginComponents(builder);
            RegisterAppPluginComponents(builder);
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

            // Start the plug-in modules in dependent order starting with the core plug-in
            // modules and ending with the app-host modules.
            this.IsStarted = true;
            foreach (var module in this.CorePlugins.SelectMany(p => p.IncludedModules()))
            {
                module.StartModule(container);
            }

            foreach (var module in this.AppComponentPlugins.SelectMany(p => p.IncludedModules()))
            {
                module.StartModule(container);
            }

            foreach (var module in this.AppHostPlugin.IncludedModules())
            {
                module.StartModule(container);
            }
        }

        public void StopPluginModules(IContainer container)
        {
            foreach (var module in this.AppHostPlugin.IncludedModules())
            {
                module.StopModule(container);
            }

            foreach (var module in this.AppComponentPlugins.SelectMany(p => p.IncludedModules()))
            {
                module.StopModule(container);
            }

            foreach (var module in this.CorePlugins.SelectMany(p => p.IncludedModules()))
            {
                module.StopModule(container);
            }

            this.IsStarted = false;
        }

        private void InitializePluginModules()
        {
            InitializePluginModules(this.CorePlugins);
            InitializePluginModules(this.AppComponentPlugins);
            InitializePluginModules(new[] { this.AppHostPlugin });
        }

        private void InitializePluginModules(IEnumerable<Plugin> plugins)
        {
            foreach (var plugin in plugins)
            {
                foreach (var module in plugin.IncludedModules())
                {
                    module.Context = new ModuleContext(this, plugin);
                    module.Initialize();
                }

                foreach (var module in plugin.IncludedModules())
                {
                    module.Configure();
                }
            }
        }

        private void RegisterCorePluginComponents(Autofac.ContainerBuilder builder)
        {
            var allPluginTypes = GetPluginTypesFrom();
            foreach (var plugin in this.CorePlugins)
            {
                ScanPluginTypes(plugin, builder);
                RegisterComponents(plugin, builder);
                ScanOtherPluginTypes(plugin, builder, allPluginTypes);
                ScanApplicationPluginTypes(plugin, builder);
            }
        }

        // Allows for each plug-in module to scan its types for any
        // service components to be registered in the Autofac Container.
        private void ScanPluginTypes(Plugin plugin, Autofac.ContainerBuilder builder)
        {
            var typeRegistration = new TypeRegistration(builder, plugin.PluginTypes);
            foreach (var module in plugin.IncludedModules())
            {
                module.ScanPlugin(typeRegistration);
            }
        }

        // Allows the each plug-in module to manually register
        // any needed service components with the Autofac Container.
        private void RegisterComponents(Plugin plugin, Autofac.ContainerBuilder builder)
        {
            foreach (var module in plugin.IncludedModules())
            {
                module.RegisterComponents(builder);
            }
        }

        // Allows a plug-in to scan all specified plug-in types, excluding types
        // defined within it's plug-in, for components to be registered in the
        // Autofac container.
        private void ScanOtherPluginTypes(Plugin plugin,
            Autofac.ContainerBuilder builder,
            IEnumerable<PluginType> sourceTypes)
        {
            var typeRegistration = new TypeRegistration(
                builder,
                sourceTypes.Except(plugin.PluginTypes));

            foreach (var module in plugin.IncludedModules())
            {
                module.ScanAllOtherPlugins(typeRegistration);
            }
        }

        private void ScanApplicationPluginTypes(Plugin plugin,
            Autofac.ContainerBuilder builder)
        {
            var appPluginTypes = GetPluginTypesFrom(PluginTypes.AppHostPlugin, PluginTypes.AppComponentPlugin);

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
            var allAppPluginTypes = GetPluginTypesFrom(PluginTypes.AppComponentPlugin, PluginTypes.AppHostPlugin);

            // Application Components:
            foreach (var plugin in this.AppComponentPlugins)
            {
                ScanPluginTypes(plugin, builder);
                RegisterComponents(plugin, builder);
                ScanOtherPluginTypes(plugin, builder, allAppPluginTypes);
            }

            // Application Host:
            ScanPluginTypes(this.AppHostPlugin, builder);
            RegisterComponents(this.AppHostPlugin, builder);
            ScanOtherPluginTypes(this.AppHostPlugin, builder, allAppPluginTypes);
        }
    }
}

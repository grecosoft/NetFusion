using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Extensions;
using NetFusion.Bootstrap.Manifests;
using NetFusion.Bootstrap.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetFusion.Test.Plugins
{
    /// <summary>
    /// Implementation of ITypeResolver that delegates to the default
    /// implementation but provides custom logic for Mock Plug-ins.
    /// </summary>
    public class TestTypeResolver : ITypeResolver
    {
        private readonly ITypeResolver _delegateResolver;
        private readonly List<MockPlugin> _plugins;
        private readonly Assembly _scanAssembly;

        public TestTypeResolver(Type scanTypesAssembly = null)
        {
            _plugins = new List<MockPlugin>();
            _delegateResolver = new TypeResolver();
            _scanAssembly = scanTypesAssembly?.GetTypeInfo().Assembly;
        }

        public void Initialize(ILoggerFactory loggerFactory)
        {
            _delegateResolver.Initialize(loggerFactory);
        }

        public void SetPluginManifests(ManifestRegistry registry)
        {
            registry.AllManifests = _plugins.Cast<IPluginManifest>().ToList();
        }

        public void SetPluginResolvedTypes(Plugin plugin)
        {
            var mockPlugin = plugin.Manifest as MockPlugin;

            if (mockPlugin == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(TestTypeResolver)} can only operate on types of {nameof(MockPlugin)}");
            }

            var pluginTypes = mockPlugin.PluginTypes;

            // Automatically add types contained within the assembly of the specified type.
            // For example, when running in LINQ Pad, this will automatically add any types
            // defined within the editor.
            if (_scanAssembly != null && plugin.Manifest is MockAppHostPlugin)
            {
                pluginTypes = pluginTypes.Union(_scanAssembly.GetTypes());
            }

            var allPluginTypes = pluginTypes.Select(t => 
                new PluginType(plugin, t, mockPlugin.AssemblyName)).ToArray();

            var pluginModules = allPluginTypes.CreateInstancesDerivingFrom<IPluginModule>().ToArray();

            plugin.SetPluginResolvedTypes(allPluginTypes, pluginModules);
        }

        public void SetPluginModuleKnownTypes(IPluginModule forModule, IEnumerable<PluginType> fromPluginTypes)
        {
            _delegateResolver.SetPluginModuleKnownTypes(forModule, fromPluginTypes);
        }

        /// <summary>
        /// Adds one or more mock plug-ins to the type-resolver
        /// </summary>
        /// <param name="plugins">The plug-ins to be added.</param>
        public void AddPlugin(params MockPlugin[] plugins)
        {
            if (plugins == null) throw new ArgumentNullException(nameof(plugins));
            _plugins.AddRange(plugins);
        }

        public MockPlugin GetHostPlugin()
        {
            var hostPlugin = _plugins.OfType<MockAppHostPlugin>().FirstOrDefault();
            if (hostPlugin == null)
            {
                throw new InvalidOperationException("Mock Application Host Plug-in not registered by unit-test.");
            }

            return hostPlugin;
        }

        /// <summary>
        /// Adds a mock plug-in of a specific type to the type-resolver.
        /// </summary>
        /// <typeparam name = "T" > The type of the mock plug-in.</typeparam>
        /// <returns>Reference to the created plug-in to which types can
        /// be added.</returns>
        public MockPlugin AddPlugin<T>()
            where T : MockPlugin, new()
        {
            var plugin = new T();
            _plugins.Add(plugin);
            return plugin;
        }
    }
}

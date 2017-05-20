using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Manifests;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common;
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
        private ITypeResolver _delegateResolver;
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

        public void SetPluginTypes(Plugin plugin)
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

            plugin.PluginTypes = pluginTypes.Select(t => 
                new PluginType(plugin, t, mockPlugin.AssemblyName)).ToArray();
        }

        public void SetPluginModules(Plugin plugin)
        {
            _delegateResolver.SetPluginModules(plugin);
        }

        public IEnumerable<Type> SetPluginModuleKnownTypes(IPluginModule forModule, IEnumerable<PluginType> fromPluginTypes)
        {
            return _delegateResolver.SetPluginModuleKnownTypes(forModule, fromPluginTypes);
        }

        /// <summary>
        /// Adds one or more mock plug-ins to the type-resolver
        /// </summary>
        /// <param name="plugins">The plug-ins to be added.</param>
        public void AddPlugin(params MockPlugin[] plugins)
        {
            Check.NotNull(plugins, nameof(plugins), "plug-ins not specified");
            _plugins.AddRange(plugins);
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

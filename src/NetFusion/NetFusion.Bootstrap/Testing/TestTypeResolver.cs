using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Manifests;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NetFusion.Bootstrap.Testing
{
    /// <summary>
    /// Implementation of ITypeResolver that delegates to the default
    /// implementation but provides custom logic for Mock Plug-ins.
    /// </summary>
    public class TestTypeResolver : TypeResolver
    {
        private readonly string _assemblyPath;
        private readonly string[] _searchPatterns;
        private readonly List<MockPlugin> _plugins;

        /// <summary>
        /// Indicates if the application-host should have it types loaded
        /// from the assembly in which it is contained.  By default, this
        /// is false.  When running in a host such a LinqPad, setting this
        /// value to true will cause the types specified within the LinqPad
        /// editor to automatically loaded into the application host plug-in.
        /// </summary>
        public bool LoadAppHostFromAssembly { get; set; } = false;

        public TestTypeResolver()
        {
            _plugins = new List<MockPlugin>();
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="assemblyPath">The path to search for plug-in assemblies.</param>
        /// <param name="searchPatterns">The pattern used to filter assemblies.</param>
        public TestTypeResolver(string assemblyPath, params string[] searchPatterns)
        {
            Check.NotNull(assemblyPath, nameof(assemblyPath), "assembly path not specified");
            Check.NotNull(assemblyPath, nameof(searchPatterns), "search patterns not specified");

            _plugins = new List<MockPlugin>();

            _assemblyPath = assemblyPath;
            _searchPatterns = searchPatterns;
        }

        public override void SetManifests(ManifestRegistry registry)
        {
            // This is the case for unit-tests.  All unit-tests should setup 
            // the container for test and never reference any runtime assemblies.
            if (_assemblyPath == null)
            {
                registry.AllManifests = _plugins.Cast<IPluginManifest>().ToList();
                return;
            }

            // This is the case for running in a host such as LinqPad where you
            // want to load plug-ins that are in a specified path.
            var assemblyNames = ProbeForMatchingAssemblyNames(new DirectoryInfo(_assemblyPath), _searchPatterns);
            var assemblies = LoadAssemblies(assemblyNames).ToArray();

            SetManifestTypes(registry, assemblies);
 
            // Add the plug-ins that have been manually added.
            registry.AllManifests.AddRange(_plugins);
        }

        public override void SetPluginTypes(Plugin plugin)
        {
            var mockPlugin = plugin.Manifest as MockPlugin;

            if (this.LoadAppHostFromAssembly && mockPlugin is IAppHostPluginManifest)
            {
                base.SetPluginTypes(plugin);
                return;
            }
            
            if (mockPlugin != null)
            {
                plugin.PluginTypes = mockPlugin.PluginTypes
                       .Select(t => new PluginType(plugin, t, mockPlugin.AssemblyName))
                       .ToArray();

                return;
            }

            // Delegate to the base implementation to discover types from the plug-in's assembly.
            base.SetPluginTypes(plugin);
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
        /// <typeparam name="T">The type of the mock plug-in.</typeparam>
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

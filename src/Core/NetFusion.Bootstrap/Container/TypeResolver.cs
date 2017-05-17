using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Plugins;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Extensions;
using NetFusion.Bootstrap.Logging;
using NetFusion.Bootstrap.Manifests;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common;
using NetFusion.Common.Extensions.Collection;
using NetFusion.Common.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Implements the discovery of assemblies containing manifests that represent plug-ins.  
    /// Also responsible for loading a plug-in's types and modules.  Any assembly containing
    /// a IPluginManifest derived type signifies that the assembly is a plug-in.
    /// 
    /// Having this component load the plug-in types decouples the AppContainer 
    /// from .NET assemblies and makes the design easy to unit-test.
    /// </summary>
    public class TypeResolver : ITypeResolver
    {
        private List<IContainerConfig> _importedConfigs = new List<IContainerConfig>();
        private string[] _searchPatterns;
        private ILogger<TypeResolver> _logger;

        /// <summary>
        /// Creates a type resolver that will load assembles representing plug-ins 
        /// contained within the application's base directory.
        /// </summary>
        /// <param name="searchPattern">Patterns used to filter the assemblies that
        /// represent plug-ins.
        ///</param>
        public TypeResolver(params string[] searchPattern)
        {
            Check.NotNull(searchPattern, nameof(searchPattern));

            _searchPatterns = AddDefaultSearchPatterns(searchPattern);
        }

        public void Initialize(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TypeResolver>();
        }

        private static string[] AddDefaultSearchPatterns(string[] searchPatterns)
        {
            var patterns = new List<string>(searchPatterns.Select(p => p.ToLower()));
            patterns.AddRange(new[] { "netfusion.*", "netfusion.*.*" });
            return patterns.Distinct().ToArray();
        }

        public virtual void SetPluginManifests(ManifestRegistry registry)
        {
            Check.NotNull(registry, nameof(registry), "registry not specified");

            Assembly[] pluginAssemblies = GetPluginAssemblies(_searchPatterns);
            SetManifestTypes(registry, pluginAssemblies);
        }

        private Assembly[] GetPluginAssemblies(string[] searchPatterns)
        {
            AssemblyName[] filteredAssemblyNames = DependencyContext.Default.ProbeForMatchingAssemblyNames(searchPatterns);
            LogMatchingAssemblies(searchPatterns, filteredAssemblyNames);

            Assembly[] assemblies = LoadAssemblies(filteredAssemblyNames).ToArray();

            try
            {
                return assemblies.Where(a => a.GetTypes()
                    .Any(t => t.IsConcreteTypeDerivedFrom<IPluginManifest>()))
                    .ToArray();
            }
            catch (ReflectionTypeLoadException ex)
            {
                var loadErrors = ex.LoaderExceptions.Select(le => le.Message).Distinct().ToList();
                throw new ContainerException("Error loading plug-in assembly.", loadErrors, ex);
            }
        }

        private void LogMatchingAssemblies(string[] searchPatterns, IEnumerable<AssemblyName> matchingAssemblies)
        {
            _logger.LogDebugDetails(BootstrapLogEvents.BOOTSTRAP_INITIALIZE, "Type Resolver", new {
                SearchPatterns = searchPatterns,
                MatchingAssemblies = matchingAssemblies.Select(a => a.FullName)
            });
        }

        protected IEnumerable<Assembly> LoadAssemblies(IEnumerable<AssemblyName> assemblyNames)
        {
            var loadedAssemblies = new List<Assembly>();

            foreach (AssemblyName assemblyName in assemblyNames)
            {
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.Load(assemblyName);
                    loadedAssemblies.Add(assembly);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    var loadErrors = ex.LoaderExceptions.Select(le => le.Message).Distinct().ToList();
                    throw new ContainerException("Error loading plug-in assembly.", loadErrors, ex);
                }
                catch (Exception ex)
                {
                    throw new ContainerException(
                        $"Error loading assembly: {assemblyName.Name}",
                        ex);
                }
            }
            return loadedAssemblies;
        }

        protected void SetManifestTypes(ManifestRegistry registry, Assembly[] pluginAssemblies)
        {
            IEnumerable<Type> pluginTypes = pluginAssemblies.SelectMany(pa => pa.GetTypes());
            registry.AllManifests = pluginTypes.CreateInstancesDerivingFrom<IPluginManifest>().ToList();

            foreach (IPluginManifest manifest in registry.AllManifests)
            {
                Assembly assembly = manifest.GetType().GetTypeInfo().Assembly;
                manifest.AssemblyName = assembly.FullName;
                manifest.AssemblyVersion = assembly.GetName().Version.ToString();
                manifest.MachineName = Environment.MachineName;
            }
        }

        public virtual void SetPluginTypes(Plugin plugin)
        {
            Check.NotNull(plugin, nameof(plugin), "plug-in not specified");

            var manifestTypeInfo = plugin.Manifest.GetType().GetTypeInfo();
            var pluginAssembly = manifestTypeInfo.Assembly;

            plugin.PluginTypes = pluginAssembly.GetTypes()
                .Select(t => new PluginType(plugin, t, pluginAssembly.GetName().Name))
                .ToArray();
        }

        public void SetPluginModules(Plugin plugin)
        {
            Check.NotNull(plugin, nameof(plugin), "plug-in not specified");

            if (plugin.PluginTypes == null)
            {
                throw new InvalidOperationException(
                    "plug-in types must loaded before modules can be discovered");
            }

            plugin.PluginModules = plugin.PluginTypes.CreateInstancesDerivingFrom<IPluginModule>().ToArray();
        }

        // Automatically populates all properties on a plug-in module that are an enumeration of
        // a derived IPluginKnownType.  The plug-in known types specific by the module are returned
        // for use by the consumer. 
        public IEnumerable<Type> SetPluginModuleKnownTypes(IPluginModule forModule, IEnumerable<PluginType> fromPluginTypes)
        {
            Check.NotNull(forModule, nameof(forModule), "module to discover known types not specified");
            Check.NotNull(fromPluginTypes, nameof(fromPluginTypes), "list of plug-in types not specified");

            IEnumerable<PropertyInfo> knownTypeProps = GetKnownTypeProperties(forModule);
            knownTypeProps.ForEach(ktp => SetKnownPropertyInstances(forModule, ktp, fromPluginTypes));
            return knownTypeProps.Select(ktp => ktp.PropertyType.GenericTypeArguments.First());
        }

        private IEnumerable<PropertyInfo> GetKnownTypeProperties(IPluginModule module)
        {
            BindingFlags bindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            return module.GetType().GetProperties(bindings)
                .Where(p =>
                    p.PropertyType.IsClosedGenericTypeOf(typeof(IEnumerable<>), typeof(IKnownPluginType))
                    && p.CanWrite);
        }

        private void SetKnownPropertyInstances(IPluginModule forModule, PropertyInfo KnownTypeProperty,
           IEnumerable<PluginType> fromPluginTypes)
        {
            var knownType = KnownTypeProperty.PropertyType.GetGenericArguments().First();
            var discoveredInstances = fromPluginTypes.CreateInstancesDerivingFrom(knownType).ToList();

            // Create an array based on the known type and populate it from discovered instances.
            Array array = Array.CreateInstance(knownType, discoveredInstances.Count());
            for (var i = 0; i < discoveredInstances.Count(); i++)
            {
                array.SetValue(discoveredInstances.ElementAt(i), i);
            }

            // Set the corresponding property on the module.
            KnownTypeProperty.SetValue(forModule, array);
        }
    }
}
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Plugins;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Extensions;
using NetFusion.Bootstrap.Logging;
using NetFusion.Bootstrap.Manifests;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetFusion.Bootstrap.Refactors;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Implements the discovery of assemblies containing manifests that represent plug-ins.  
    /// Also responsible for loading a plug-in's types and modules.  Any assembly containing
    /// a IPluginManifest derived type signifies that the assembly is a plug-in.
    /// 
    /// Having this component load the plug-in types decouples the AppContainer 
    /// from .NET assemblies and makes the design easier to unit-test and extend.
    /// </summary>
    public class TypeResolver : ITypeResolver
    {
        public TypeResolver()
        {
            
        }

        public void SetPluginMeta(IPluginDefinition plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            
            Assembly assembly = plugin.GetType().Assembly;
            
            plugin.SetPluginMeta(
                assembly.FullName, 
                assembly.GetName().Version.ToString(),
                assembly.GetExportedTypes());
        }

        public void ComposePlugin(IPluginDefinition plugin, IEnumerable<Type> fromPluginTypes)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            if (fromPluginTypes == null) throw new ArgumentNullException(nameof(fromPluginTypes));

            foreach (IPluginModule module in plugin.Modules)
            {
                IEnumerable<PropertyInfo> knownTypeProps = GetKnownTypeProperties(module);
                knownTypeProps.ForEach(ktp => SetKnownPropertyInstances(module, ktp, fromPluginTypes));
            }
        }
        
        private static IEnumerable<PropertyInfo> GetKnownTypeProperties(IPluginModule module)
        {
            const BindingFlags bindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            return module.GetType().GetProperties(bindings)
                .Where(p =>
                    p.PropertyType.IsClosedGenericTypeOf(typeof(IEnumerable<>), typeof(IKnownPluginType))
                    && p.CanWrite);
        }
        
        private static void SetKnownPropertyInstances(IPluginModule forModule, PropertyInfo knownTypeProperty,
            IEnumerable<Type> fromPluginTypes)
        {
            var knownType = knownTypeProperty.PropertyType.GetGenericArguments().First();
            var discoveredInstances = fromPluginTypes.CreateInstancesDerivingFrom(knownType).ToArray();

            // Create an array based on the known type and populate it from discovered instances.
            Array array = Array.CreateInstance(knownType, discoveredInstances.Count());
            for (var i = 0; i < discoveredInstances.Count(); i++)
            {
                array.SetValue(discoveredInstances.ElementAt(i), i);
            }

            // Set the corresponding property on the module.
            knownTypeProperty.SetValue(forModule, array);
        }
     
        
        
        private readonly string[] _searchPatterns;
        private ILogger<TypeResolver> _logger;

        /// <summary>
        /// Creates a type resolver that will load assembles representing plug-ins contained 
        /// within the application's base directory.
        /// </summary>
        /// <param name="searchPattern">Patterns used to filter the assemblies that represent plug-ins.
        ///</param>
        public TypeResolver(params string[] searchPattern)
        {
            if (searchPattern == null) throw new ArgumentNullException(nameof(searchPattern),
                "Search Pattern cannot be null.");
            
            _searchPatterns = AddDefaultSearchPatterns(searchPattern);
        }

        public void Initialize(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory), 
                "Logger Factory cannot be null.");
            
            _logger = loggerFactory.CreateLogger<TypeResolver>();
        }

        private static string[] AddDefaultSearchPatterns(IEnumerable<string> searchPatterns)
        {
            var patterns = new List<string>(searchPatterns.Select(p => p.ToLower()));
            patterns.AddRange(new[] { "netfusion.*", "netfusion.*.*" });
            return patterns.Distinct().ToArray();
        }

        public virtual void SetPluginManifests(ManifestRegistry registry)
        {
            if (registry == null) throw new ArgumentNullException(nameof(registry),
                "Registry cannot be null.");
            
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
                var loadErrors = ex.LoaderExceptions.Select(le => le.Message).Distinct().ToArray();
                throw new ContainerException("Error Loading Plug-In Assembly.", ex, "LoadErrors", loadErrors);
            }
        }

        private void LogMatchingAssemblies(IEnumerable searchPatterns, IEnumerable<AssemblyName> matchingAssemblies)
        {
            _logger.LogDebugDetails(BootstrapLogEvents.BootstrapInitialize, "Type Resolver", new {
                SearchPatterns = searchPatterns,
                MatchingAssemblies = matchingAssemblies.Select(a => a.FullName)
            });
        }

        protected static IEnumerable<Assembly> LoadAssemblies(IEnumerable<AssemblyName> assemblyNames)
        {
            var loadedAssemblies = new List<Assembly>();

            foreach (AssemblyName assemblyName in assemblyNames)
            {
                try
                {
                    var assembly = Assembly.Load(assemblyName);
                    loadedAssemblies.Add(assembly);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    var loadErrors = ex.LoaderExceptions.Select(le => le.Message).Distinct().ToArray();
                    throw new ContainerException("Error Loading Plug-In Assembly.", ex, "LoadErrors", loadErrors);
                }
                catch (Exception ex)
                {
                    throw new ContainerException($"Error Loading Plug-In Assembly: {assemblyName.Name}", ex);
                }
            }
            return loadedAssemblies;
        }

        protected static void SetManifestTypes(ManifestRegistry registry, IEnumerable<Assembly> pluginAssemblies)
        {
            IEnumerable<Type> pluginTypes = pluginAssemblies.SelectMany(pa => pa.GetTypes());
            registry.AllManifests = pluginTypes.CreateInstancesDerivingFrom<IPluginManifest>().ToList();

            foreach (IPluginManifest manifest in registry.AllManifests)
            {
                Assembly assembly = manifest.GetType().GetTypeInfo().Assembly;
                manifest.AssemblyName = assembly.FullName;
                manifest.AssemblyVersion = assembly.GetName().Version.ToString();
            }
        }

        public virtual void SetPluginResolvedTypes(Plugin plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin), 
                "Plug-In cannot be null.");

            // Get all types contained within the same assembly as the discovered
            // Plug-in Manifest type.  This is all types belonging to the plug-in.
            var manifestTypeInfo = plugin.Manifest.GetType().GetTypeInfo();
            var pluginAssembly = manifestTypeInfo.Assembly;

            var pluginTypes = pluginAssembly.GetTypes()
                .Select(t => new PluginType(plugin, t, pluginAssembly.GetName().Name))
                .ToArray();

            // Create instances of all plugin-modules.  These modules are what will be calling during 
            // the bootstrap process to allow plug-ins to register services and to initialize and cache 
            // any information needed during execution of its services during normal application operation.
            var pluginModules = pluginTypes.CreateInstancesDerivingFrom<IPluginModule>()
                .Where(m => ! m.IsExcluded)
                .ToArray();

            plugin.SetPluginResolvedTypes(pluginTypes, pluginModules);
        }

        // Automatically populates all properties on a plug-in module that are an enumeration of
        // a derived IPluginKnownType.  This can be thought of as a very simple implementation of
        // MEF doing only what is needed.
        public void SetPluginModuleKnownTypes(IPluginModule forModule, IEnumerable<PluginType> fromPluginTypes)
        {
            if (forModule == null) throw new ArgumentNullException(nameof(forModule), 
                "Module to discover know types cannot be null.");

            if (fromPluginTypes == null) throw new ArgumentNullException(nameof(fromPluginTypes), 
                "List of Plug-in types to search for know types cannot be null.");
            
            IEnumerable<PropertyInfo> knownTypeProps = GetKnownTypeProperties(forModule);
            knownTypeProps.ForEach(ktp => SetKnownPropertyInstances(forModule, ktp, fromPluginTypes));
        }

        private static void SetKnownPropertyInstances(IPluginModule forModule, PropertyInfo knownTypeProperty,
           IEnumerable<PluginType> fromPluginTypes)
        {
            var knownType = knownTypeProperty.PropertyType.GetGenericArguments().First();
            var discoveredInstances = fromPluginTypes.CreateInstancesDerivingFrom(knownType).ToArray();

            // Create an array based on the known type and populate it from discovered instances.
            Array array = Array.CreateInstance(knownType, discoveredInstances.Count());
            for (var i = 0; i < discoveredInstances.Count(); i++)
            {
                array.SetValue(discoveredInstances.ElementAt(i), i);
            }

            // Set the corresponding property on the module.
            knownTypeProperty.SetValue(forModule, array);
        }
    }
}
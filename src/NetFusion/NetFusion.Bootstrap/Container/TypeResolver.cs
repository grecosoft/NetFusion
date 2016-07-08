using NetFusion.Bootstrap.Exceptions;
using NetFusion.Bootstrap.Extensions;
using NetFusion.Bootstrap.Manifests;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common;
using NetFusion.Common.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NetFusion.Bootstrap.Container
{
    /// <summary>
    /// Implements the discovery of assemblies containing manifests
    /// that represent plug-ins.  Also responsible for loading a 
    /// plug-ins types and modules.  
    /// 
    /// Having this component load the plug-in types decouples the AppContainer 
    /// from .NET assemblies and makes the design easy to unit-test.
    /// </summary>
    public class TypeResolver : ITypeResolver
    {
        private readonly string[] _searchPatterns;

        protected TypeResolver()
        {

        }

        public TypeResolver(string[] searchPatterns)
        {
            Check.NotNull(searchPatterns, nameof(searchPatterns));
            _searchPatterns = searchPatterns;
        }

        public virtual void DiscoverManifests(ManifestRegistry registry)
        {
            Check.NotNull(registry, nameof(registry), "registry not specified");

            var pluginAssemblies = GetPluginAssemblies(_searchPatterns);
            SetManifestTypes(registry, pluginAssemblies);
        }

        public virtual void LoadPluginTypes(Plugin plugin)
        {
            Check.NotNull(plugin, nameof(plugin), "plug-in not specified");

            var pluginAssembly = plugin.Manifest.GetType().Assembly;
            plugin.PluginTypes = pluginAssembly.GetTypes()
                .Select(t => new PluginType(plugin, t, pluginAssembly.GetName().Name))
                .ToArray();
        }

        public void DiscoverModules(Plugin plugin)
        {
            Check.NotNull(plugin, nameof(plugin), "plug-in not specified");

            if (plugin.PluginTypes == null)
            {
                throw new InvalidOperationException(
                    "Plug-in types must loaded before modules can be discovered.");
            }
            plugin.PluginModules = plugin.PluginTypes.CreateMatchingInstances<IPluginModule>().ToArray();
        }

        // Automatically populates all properties on a plug-in module that are an enumeration of
        // a derived IPluginKnownType.  The plug-in known types specific by the module are returned
        // for use by the consumer. 
        public IEnumerable<Type> DiscoverKnownTypes(IPluginModule forModule, IEnumerable<PluginType> fromPluginTypes)
        {
            Check.NotNull(forModule, nameof(forModule), "module to discover known types not specified");
            Check.NotNull(fromPluginTypes, nameof(fromPluginTypes), "list of plug-in types not specified");

            var knownTypeProps = GetKnownTypeProperties(forModule);
            knownTypeProps.ForEach(ktp => SetKnownPropertyInstances(forModule, ktp, fromPluginTypes));
            return knownTypeProps.Select(ktp => ktp.PropertyType.GenericTypeArguments.First());
        }

        public string GetResourceAsText(Plugin plugin, string resourceName)
        {
            var pluginAssembly = plugin.Manifest.GetType().Assembly;

            using (Stream stream = pluginAssembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private Assembly[] GetPluginAssemblies(string[] searchPatterns)
        {
            var probeDirectory = GetAssemblyProbeDirectory();
            var filteredAssemblyNames = ProbeForMatchingAssemblyNames(probeDirectory, searchPatterns);

            var assemblies = GetAssemblies(filteredAssemblyNames);
            return assemblies.Where(a => a.GetTypes()
                .Any(t => t.IsDerivedFrom<IPluginManifest>())).ToArray();
        }

        private DirectoryInfo GetAssemblyProbeDirectory()
        {
            return new DirectoryInfo(AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory);
        }

        protected AssemblyName[] ProbeForMatchingAssemblyNames(DirectoryInfo probeDirectory, string[] searchPatterns)
        {
            var fileNames = probeDirectory.GetFileNames(searchPatterns);
            return fileNames.Select(AssemblyName.GetAssemblyName).ToArray();
        }

        private Assembly[] GetAssemblies(AssemblyName[] filteredAssemblyNames)
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var loadedCodeBases = loadedAssemblies.Select(a => a.CodeBase);

            var nonLoadedAssemblyNames = filteredAssemblyNames.Where(fa => !loadedCodeBases.Contains(
                fa.CodeBase, StringComparer.OrdinalIgnoreCase));

            return loadedAssemblies.Concat(LoadAssemblies(nonLoadedAssemblyNames))
                .ToArray();
        }

        protected IEnumerable<Assembly> LoadAssemblies(IEnumerable<AssemblyName> assemblyNames)
        {
            foreach(var assemblyName in assemblyNames)
            {
                // TODO:  Add details for load exceptions.
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.Load(assemblyName); 
                }
                catch (Exception ex)
                {
                    throw new ContainerException(
                        $"Error loading assembly: {assemblyName.CodeBase}", 
                        ex);
                }

                // Excluding this assembly since it contains classes deriving from IPluginManifest
                // that are mocks used for testing and should never identify this assembly as being
                // a plug-in assembly.
                if (assembly == this.GetType().Assembly) continue;

                yield return assembly;
            }
        }

        protected void SetManifestTypes(ManifestRegistry manifestTypes, Assembly[] pluginAssemblies)
        {
            var pluginTypes = pluginAssemblies.SelectMany(pa => pa.GetTypes());
            manifestTypes.AllManifests = pluginTypes.CreateMatchingInstances<IPluginManifest>().ToList();
            manifestTypes.AllManifests.ForEach(m => m.AssemblyName = m.GetType().Assembly.FullName);
        }

        private IEnumerable<PropertyInfo> GetKnownTypeProperties(IPluginModule module)
        {
            return module.GetType().GetProperties()
                .Where(p => p.PropertyType.IsClosedGenericTypeOf(typeof(IEnumerable<>), typeof(IKnownPluginType)));
        }

        private void SetKnownPropertyInstances(IPluginModule module, PropertyInfo KnownTypeProperty, 
            IEnumerable<PluginType> pluginTypes)
        {
            var knownType = KnownTypeProperty.PropertyType.GetGenericArguments().First();
            var discoveredInstances = pluginTypes.CreateMatchingInstances(knownType).ToList();

            // Set the module property to the collection of objects matching its derived known type.
            ArrayList list = new ArrayList(discoveredInstances.ToArray());
            Array newArray = list.ToArray(knownType);

            KnownTypeProperty.SetValue(module, newArray);
        }
    }
}

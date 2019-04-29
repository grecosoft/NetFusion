using NetFusion.Base.Plugins;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;
using System;
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
     
        
        
        

      

       


    

      

     
    }
}
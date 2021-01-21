using NetFusion.Base.Plugins;
using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetFusion.Base.Logging;

namespace NetFusion.Bootstrap.Container
{
    /// <summary> 
    /// Having this component load the plug-in types decouples the CompositeContainer 
    /// from .NET assemblies and makes the design easier to unit-test and extend.
    /// </summary>
    public class TypeResolver : ITypeResolver
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly IExtendedLogger _logger;
        
        public TypeResolver(IExtendedLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public void SetPluginMeta(IPlugin plugin)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            
            Assembly assembly = plugin.GetType().Assembly;
            
            plugin.SetPluginMeta(
                assembly.FullName, 
                assembly.GetName().Version?.ToString() ?? "",
                assembly.GetExportedTypes());
        }

        // For each plugin module, finds all properties that are an IEnumerable of IKnownPluginType
        // and populates it from all concrete implementations contained within the list of provided
        // plugin-types.  Think of this as very simple version of MEF without all the fat.
        public void ComposePlugin(IPlugin plugin, IEnumerable<Type> fromPluginTypes)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            if (fromPluginTypes == null) throw new ArgumentNullException(nameof(fromPluginTypes));

            foreach (IPluginModule module in plugin.Modules)
            {
                module.KnownTypeProperties = new Dictionary<PropertyInfo, Tuple<Type, Type[]>>();
                
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
        
        private static void SetKnownPropertyInstances(IPluginModule module, PropertyInfo knownTypeProperty,
            IEnumerable<Type> fromPluginTypes)
        {
            var knownType = knownTypeProperty.PropertyType.GetGenericArguments().First();
            var discoveredInstances = fromPluginTypes.CreateInstancesDerivingFrom(knownType).ToArray();

            RecordKnowProperties(module, knownTypeProperty, knownType, discoveredInstances);

            // Create an array based on the known type and populate it from discovered instances.
            Array array = Array.CreateInstance(knownType, discoveredInstances.Length);
            for (var i = 0; i < discoveredInstances.Length; i++)
            {
                array.SetValue(discoveredInstances.ElementAt(i), i);
            }

            // Set the corresponding property on the module.
            knownTypeProperty.SetValue(module, array);
        }

        // Record the discovered properties to it can be logged later.
        private static void RecordKnowProperties(IPluginModule module, PropertyInfo knownTypeProperty, 
            Type knownType, 
            IEnumerable<object> discoveredInstances)
        {
            module.KnownTypeProperties[knownTypeProperty] = new Tuple<Type, Type[]>(
                knownType, 
                discoveredInstances.Select(i => i.GetType()).ToArray());
        }
    }
}
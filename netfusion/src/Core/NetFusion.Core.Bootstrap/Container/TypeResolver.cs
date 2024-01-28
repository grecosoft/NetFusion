using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Core.Bootstrap.Container;

/// <summary> 
/// Having this component load the plug-in types decouples the CompositeAppBuilder 
/// from .NET assemblies and makes the design easier to unit-test and extend.
/// </summary>
public class TypeResolver : ITypeResolver
{
    public void SetPluginMeta(IPlugin plugin)
    {
        ArgumentNullException.ThrowIfNull(plugin);

        Assembly assembly = plugin.GetType().Assembly;

        plugin.SetPluginMeta(
            assembly.FullName!,
            assembly.GetName().Version?.ToString() ?? string.Empty,
            assembly.GetExportedTypes());
    }

    // For each plugin module, finds all properties that are an IEnumerable of IKnownPluginType
    // and populates it from all concrete implementations contained within the list of provided
    // plugin-types.  Think of this as very simple version of MEF without all the complexity.
    public void ComposePlugin(IPlugin plugin, IEnumerable<Type> fromPluginTypes)
    {
        ArgumentNullException.ThrowIfNull(plugin);
        ArgumentNullException.ThrowIfNull(fromPluginTypes);

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
                p.PropertyType.IsClosedGenericTypeOf(typeof(IEnumerable<>), typeof(IPluginKnownType))
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

    // Record the discovered properties for logging.
    private static void RecordKnowProperties(IPluginModule module, PropertyInfo knownTypeProperty, 
        Type knownType, 
        IEnumerable<object> discoveredInstances)
    {
        module.KnownTypeProperties[knownTypeProperty] = new Tuple<Type, Type[]>(
            knownType, 
            discoveredInstances.Select(i => i.GetType()).ToArray());
    }
}
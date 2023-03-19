using System;
using System.Collections.Generic;
using NetFusion.Core.Bootstrap.Plugins;

namespace NetFusion.Core.TestFixtures.Plugins;

/// <summary>
/// Base plugin from which the different plugins are derived.  A MockPlugin allows for testing
/// of plugins since it allows the programmer to create a fully configured container that can
/// be used to test plugin component interactions.
/// </summary>
public abstract class MockPlugin : PluginBase
{
    private string _pluginId = Guid.NewGuid().ToString();
    private string _name;
    private readonly List<Type> _pluginTypes = new();

    public override string PluginId => _pluginId;
    public override string Name => _name;
    public override PluginTypes PluginType { get; }
        
    protected MockPlugin(PluginTypes pluginType) 
    {
        Types = _pluginTypes;
        PluginType = pluginType;

        _name = GetType().Name;

        AssemblyName = "unit-test";
        AssemblyVersion = "unit-test-version";
    }

    public void AddPluginType<T>()
    {
        _pluginTypes.Add(typeof(T));
    }

    public void AddPluginType(params Type[] types)
    {
        _pluginTypes.AddRange(types);
    }

    public new void AddModule<TModule>() where TModule : IPluginModule, new()
    {
        base.AddModule<TModule>();
    }

    public new void AddConfig<TConfig>() where TConfig : IPluginConfig, new()
    {
        base.AddConfig<TConfig>();
    }

    public void SetPluginId(string pluginId)
    {
        _pluginId = pluginId;
    }

    public void SetPluginName(string name)
    {
        _name = name;
    } 
}
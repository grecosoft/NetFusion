namespace NetFusion.Core.Bootstrap.Plugins;

/// <summary>
/// Read-only class providing a summary information for a plugin.
/// </summary>
public class PluginSummary
{
    public string PluginId { get; }
    public string Name { get; }
    public string AssemblyName { get; }
    public string AssemblyVersion { get; }

    public PluginSummary(IPlugin plugin)
    {
        System.ArgumentNullException.ThrowIfNull(plugin);

        PluginId = plugin.PluginId;
        Name = plugin.Name;
        AssemblyName = plugin.AssemblyName;
        AssemblyVersion = plugin.AssemblyVersion;
    }
}
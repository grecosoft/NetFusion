using NetFusion.Common.Base.Validation;
using NetFusion.Core.Settings;

namespace NetFusion.Integration.ServiceBus.Plugin.Settings;

/// <summary>
/// Settings specifying Azure Service Bus namespace configurations.
/// </summary>
[ConfigurationSection("netfusion:azure:serviceBus")]
public class BusSettings : IAppSettings,
    IValidatableType
{
    /// <summary>
    /// The configured namespaces.
    /// </summary>
    public IDictionary<string, NamespaceSettings> Namespaces { get; set; } = new Dictionary<string, NamespaceSettings>();
    
    public void InitConfiguration()
    {
        foreach (var (name, ns) in Namespaces)
        {
            ns.Name = name;
        }
    }
    
    public void Validate(IObjectValidator validator)
    {
        validator.AddChildren(Namespaces.Values);
    }
}
namespace NetFusion.Integration.ServiceBus.Plugin.Configs;

/// <summary>
/// Configurations associated with the Azure Service Bus plugin.
/// </summary>
public class ServiceBusConfig : IPluginConfig
{
    /// <summary>
    /// Determines if queue and topic namespace entities should automatically
    /// be created when the microservice bootstraps. 
    /// </summary>
    public bool IsAutoCreateEnabled { get; set; } = false;
}
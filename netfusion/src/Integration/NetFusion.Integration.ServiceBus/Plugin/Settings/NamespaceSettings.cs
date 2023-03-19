using System.ComponentModel.DataAnnotations;
using Azure.Messaging.ServiceBus;
using NetFusion.Common.Base.Validation;

namespace NetFusion.Integration.ServiceBus.Plugin.Settings;

/// <summary>
/// Settings for a specific Azure Service Bus namespace.
/// </summary>
public class NamespaceSettings : IValidatableType
{
    /// <summary>
    /// The name of the namespace.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// The connection string used to connect to the namespace.
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Azure Service Bus Connection required.")]
    public string ConnString { get; set; } = null!;

    /// <summary>
    /// The type of protocol and transport that will be used for communicating with the Service Bus service.
    /// </summary>
    public ServiceBusTransportType? TransportType { get; set; }

    /// <summary>
    /// Setting to be used to influence how dropped connections are retried.
    /// </summary>
    public RetrySettings? RetrySettings { get; set; }

    /// <summary>
    /// Settings to be applied to defined queues overriding any corresponding settings specified in code.
    /// </summary>
    public Dictionary<string, QueueSettings> Queues { get; set; } = new();

    /// <summary>
    /// Settings to be applied to defined topics overriding any corresponding settings specified in code.
    /// </summary>
    public Dictionary<string, TopicSettings> Topics { get; set; } = new();

    /// <summary>
    /// Setting overrides for any defined entity subscriptions.
    /// </summary>
    public Dictionary<string, SubscriptionSettings> Subscriptions { get; set; } = new();
    
    /// <summary>
    /// Unique value identifying the host microservice.
    /// </summary>
    internal string HostPluginId { get; set; } = string.Empty;

    public void Validate(IObjectValidator validator)
    {
        validator.AddChildren(
            Queues.Values,
            Topics.Values,
            Subscriptions.Values);
    }
}
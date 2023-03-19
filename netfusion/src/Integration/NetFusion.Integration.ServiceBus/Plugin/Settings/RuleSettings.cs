using System.ComponentModel.DataAnnotations;

namespace NetFusion.Integration.ServiceBus.Plugin.Settings;

/// <summary>
/// Settings for storing subscription rules externally within
/// the application's configuration.
/// </summary>
public class RuleSettings
{
    /// <summary>
    /// The filter used to match messages to be delivered to a subscription.
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Filter must have value specified.")]
    public string Filter { get; set; } = null!;

    /// <summary>
    /// Optional action that should be applied to the message matching the filter.
    /// A string value added to the message's properties on which other filters can
    /// be based.
    /// </summary>
    public string? Action { get; set; }
}
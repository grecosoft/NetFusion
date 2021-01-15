using System.ComponentModel.DataAnnotations;

namespace NetFusion.Azure.ServiceBus.Settings
{
    /// <summary>
    /// Settings for storing subscription rules externally within
    /// the application's configuration.
    /// </summary>
    public class RuleSettings
    {
        /// <summary>
        /// The filter used to match messages that should be delivered to a subscription.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Filter must have value specified.")]
        public string Filter { get; set; }
        
        /// <summary>
        /// Optional action that should be applied to the message matching the filter.
        /// </summary>
        public string Action { get; set; }
    }
}
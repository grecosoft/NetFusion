using System.ComponentModel.DataAnnotations;

namespace NetFusion.Azure.Messaging.Settings
{
    /// <summary>
    /// The settings for a specific namespace.
    /// </summary>
    public class NamespaceSettings
    {
        /// <summary>
        /// The Azure Service Bus defined namespace to which messages are sent.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Namespace { get; set; }
        
        /// <summary>
        /// The namespace key defined by Azure.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string NamespaceKey { get; set; }
        
        /// <summary>
        /// The name of the Azure policy to use when making the call.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string PolicyName { get; set; }

        /// <summary>
        /// The port to use to connect to the service bus.
        /// </summary>
        public int Port { get; set; } = 5671;
    }
}
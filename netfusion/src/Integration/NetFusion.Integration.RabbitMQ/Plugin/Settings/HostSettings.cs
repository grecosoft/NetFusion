using System.ComponentModel.DataAnnotations;

namespace NetFusion.Integration.RabbitMQ.Plugin.Settings
{
    /// <summary>
    /// Settings for a host associated with a specific bus connection.
    /// </summary>
    public class HostSettings
    {
        /// <summary>
        /// The name of the host computer running RabbitMQ.
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "HostName Required")]
        public string HostName { get; set; } = string.Empty;

        /// <summary>
        /// The connection port to use.  Defaults to 5672.
        /// </summary>
        public ushort Port { get; set; } = 5672;
    }
}
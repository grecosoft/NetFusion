    using System.ComponentModel.DataAnnotations;

namespace NetFusion.AMQP.Settings
{
    /// <summary>
    /// The settings for a specific namespace.
    /// </summary>
    public class HostSettings
    {
        /// <summary>
        /// The name of the host.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string HostName { get; set; }
        
        /// <summary>
        /// The address of the host to which messages are received and sent.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string HostAddress { get; set; }
        
        /// <summary>
        /// The username used to authenticate with the host.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Username { get; set; }
        
        /// <summary>
        /// The password used to authenticate with the host.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Password { get; set; }

        /// <summary>
        /// The port to use to connect to the host.
        /// </summary>
        public int Port { get; set; } = 5671;
    }
}
using NetFusion.Settings;
using System.Collections.Generic;

namespace NetFusion.Rest.Config
{
    /// <summary>
    /// Contains configuration settings for all external API services that
    /// can be consumed by host application.
    /// </summary>
    [ConfigurationSection("netfusion:plugins:client-factory")]
    public class ClientFactorySettings : AppSettings
    {
        public ClientFactorySettings()
        {
            Clients = new List<ClientSettings>();
        }

        /// <summary>
        /// List of clients to external API services that should be created
        /// and made available to the host application.
        /// </summary>
        public IList<ClientSettings> Clients { get; set; }
    }
}

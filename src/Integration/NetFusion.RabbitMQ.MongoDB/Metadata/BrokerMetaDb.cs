using NetFusion.MongoDB.Configs;
using NetFusion.Settings;
using System.ComponentModel.DataAnnotations;

namespace NetFusion.RabbitMQ.MongoDB.Metadata
{
    /// <summary>
    /// The database to store the current exchange and queue meta-data.
    /// </summary>
    [ConfigurationSection("integration:brokerMetadata")]
    public class BrokerMetaDb : MongoSettings
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Collection Name is Required.")]
        public string CollectionName { get; set; } = "NetFusion.BrokerMeta";
    }
}

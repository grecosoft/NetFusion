using NetFusion.MongoDB.Configs;
using NetFusion.Settings;
using System.ComponentModel.DataAnnotations;

namespace NetFusion.RabbitMQ.MongoDB.Metadata
{
    [ConfigurationSection("integration:brokerMetadata")]
    public class BrokerMetaDb : MongoSettings
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Collection Name is Required.")]
        public string CollectionName { get; set; } = "NetFusion.BrokerMeta";
    }
}

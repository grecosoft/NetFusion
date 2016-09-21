using NetFusion.MongoDB.Configs;
using System.ComponentModel.DataAnnotations;

namespace NetFusion.Integration.RabbitMQ
{
    public class BrokerMetaDb : MongoSettings
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Collection Name is Required.")]
        public string CollectionName { get; set; } = "NetFusion.BrokerMeta";
    }
}

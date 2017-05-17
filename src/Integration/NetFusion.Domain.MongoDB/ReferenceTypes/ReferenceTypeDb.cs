using NetFusion.MongoDB.Configs;
using NetFusion.Settings;

namespace NetFusion.Domain.MongoDB.ReferenceTypes
{
    [ConfigurationSection("reference:metadata")]
    public class ReferenceTypeDb : MongoSettings
    {
        public string CollectionName { get; set; } = "reference.Types";
    }
}

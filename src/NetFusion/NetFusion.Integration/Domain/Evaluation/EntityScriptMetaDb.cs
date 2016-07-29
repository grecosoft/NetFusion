using NetFusion.MongoDB.Configs;

namespace NetFusion.Integration.Domain.Evaluation
{
    public class EntityScriptMetaDb : MongoSettings
    {
        public string CollectionName { get; set; }
    }
}

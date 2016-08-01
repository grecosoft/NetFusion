using NetFusion.MongoDB.Configs;

namespace NetFusion.Integration.Domain.Scripting
{
    public class EntityScriptMetaDb : MongoSettings
    {
        public string CollectionName { get; set; }
    }
}

using NetFusion.MongoDB.Configs;

namespace NetFusion.Integration.Domain
{
    public class NetFusionExpressionDb : MongoSettings
    {
        public string CollectionName { get; set; }
    }
}

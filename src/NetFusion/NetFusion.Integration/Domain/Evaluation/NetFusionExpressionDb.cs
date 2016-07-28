using NetFusion.MongoDB.Configs;

namespace NetFusion.Integration.Domain.Evaluation
{
    public class NetFusionExpressionDb : MongoSettings
    {
        public string CollectionName { get; set; }
    }
}

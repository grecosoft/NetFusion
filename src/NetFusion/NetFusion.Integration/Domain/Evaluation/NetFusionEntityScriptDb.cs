using NetFusion.MongoDB.Configs;

namespace NetFusion.Integration.Domain.Evaluation
{
    public class NetFusionEvaluationDb : MongoSettings
    {
        public string CollectionName { get; set; }
    }
}

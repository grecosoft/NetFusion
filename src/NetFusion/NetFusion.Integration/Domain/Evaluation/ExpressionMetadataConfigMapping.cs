using NetFusion.MongoDB;

namespace NetFusion.Integration.Domain.Evaluation
{
    public class ExpressionMetadataConfigMapping : EntityClassMap<ExpressionMetadataConfig>
    {
        public ExpressionMetadataConfigMapping()
        {
            AutoMap();
            //MapStringObjectIdProperty(p => p.Id);
        }
    }
}

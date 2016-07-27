using NetFusion.MongoDB;

namespace NetFusion.Integration.Domain
{
    public class ExpressionMetadataConfigMapping : EntityClassMap<ExpressionMetadataConfig>
    {
        public ExpressionMetadataConfigMapping()
        {
            AutoMap();
            MapStringObjectIdProperty(p => p.Id);
        }
    }
}

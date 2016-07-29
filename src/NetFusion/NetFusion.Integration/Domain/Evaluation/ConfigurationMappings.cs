using NetFusion.MongoDB;

namespace NetFusion.Integration.Domain.Evaluation
{
    public class EntityEvaluationSetConfigMap : EntityClassMap<EntityScriptConfig>
    {
        public EntityEvaluationSetConfigMap()
        {
            AutoMap();
            MapStringObjectIdProperty(e => e.Id);
        }
    }

    public class EntityExpressionConfgMap : EntityClassMap<EntityExpressionConfig>
    {
        public EntityExpressionConfgMap()
        {
            AutoMap();
        }
    }
}

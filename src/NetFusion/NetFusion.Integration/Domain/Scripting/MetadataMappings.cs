using NetFusion.MongoDB;

namespace NetFusion.Integration.Domain.Scripting
{
    public class EntityScriptMetaMap : EntityClassMap<EntityScriptMeta>
    {
        public EntityScriptMetaMap()
        {
            AutoMap();
            MapStringPropertyToObjectId(e => e.Id);
        }
    }

    public class EntityExpressionMetaMap : EntityClassMap<EntityExpressionMeta>
    {
        public EntityExpressionMetaMap()
        {
            AutoMap();
        }
    }
}

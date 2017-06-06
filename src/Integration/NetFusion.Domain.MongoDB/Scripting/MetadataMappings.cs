using NetFusion.MongoDB;

namespace NetFusion.Domain.MongoDB.Scripting
{
    public class EntityScriptMetaMap : EntityClassMap<EntityScriptMeta>
    {
        public EntityScriptMetaMap()
        {
            AutoMap();
            MapStringPropertyToObjectId(e => e.ScriptId);
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

using NetFusion.MongoDB;

namespace NetFusion.Domain.MongoDB.Scripting
{
    /// <summary>
    /// Data model MongoDB collection mapping for collection.
    /// </summary>
    public class EntityScriptMetaMap : EntityClassMap<EntityScriptMeta>
    {
        public EntityScriptMetaMap()
        {
            AutoMap();
            MapStringPropertyToObjectId(e => e.ScriptId);
        }
    }

    /// <summary>
    /// Data model MongoDB mapping for collection child.
    /// </summary>
    public class EntityExpressionMetaMap : EntityClassMap<EntityExpressionMeta>
    {
        public EntityExpressionMetaMap()
        {
            AutoMap();
        }
    }
}

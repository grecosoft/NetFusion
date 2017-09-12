using MongoDB.Driver;
using NetFusion.Base.Scripting;
using NetFusion.Common;
using NetFusion.MongoDB;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetFusion.Domain.MongoDB.Scripting
{
    public class EntityScriptMetaRepository : IEntityScriptMetaRepository
    {
        private readonly IMongoDbClient<EntityScriptMetaDb> _mongoClient;
        private readonly IMongoCollection<EntityScriptMeta> _scriptMetadataColl;

        public EntityScriptMetaRepository(EntityScriptMetaDb settings, IMongoDbClient<EntityScriptMetaDb> mongoClient)
        {
            _mongoClient = mongoClient;
            _scriptMetadataColl = mongoClient.GetCollection<EntityScriptMeta>(settings.CollectionName);
        }

        public async Task<IEnumerable<EntityScript>> ReadAll()
        {
            var scriptMetadata = await _scriptMetadataColl.Find(_ => true).ToListAsync();
            return scriptMetadata.Select(ec => ec.ToEntity());
        }

        public async Task<string> SaveAsync(EntityScript script)
        {
            Check.NotNull(script, nameof(script), "script not specified");

            EntityScriptMeta scriptMetadata = new EntityScriptMeta(script);

            if (scriptMetadata.ScriptId == null)
            {
                await _scriptMetadataColl.InsertOneAsync(scriptMetadata);
                return scriptMetadata.ScriptId;
            }

            var filter = Builders<EntityScriptMeta>.Filter.Where(s => s.ScriptId == script.ScriptId);
            var options = new FindOneAndReplaceOptions<EntityScriptMeta, EntityScriptMeta> { IsUpsert = true };

            await _scriptMetadataColl.ReplaceOneAsync(s => s.ScriptId == script.ScriptId, scriptMetadata);
            return null;
        }
    }
}

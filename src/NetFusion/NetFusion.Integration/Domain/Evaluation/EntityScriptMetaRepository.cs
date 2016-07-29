using MongoDB.Driver;
using NetFusion.Domain.Scripting;
using NetFusion.MongoDB;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetFusion.Integration.Domain.Evaluation
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
    }
}

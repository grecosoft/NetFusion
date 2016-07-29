using MongoDB.Driver;
using NetFusion.Domain.Scripting;
using NetFusion.MongoDB;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetFusion.Integration.Domain.Evaluation
{
    public class EntityExpresionSetRepository : IEntityScriptRepository
    {
        private readonly IMongoDbClient<NetFusionEvaluationDb> _mongoClient;
        private readonly IMongoCollection<EntityScriptConfig> _expressionSetColl;

        public EntityExpresionSetRepository(NetFusionEvaluationDb settings, IMongoDbClient<NetFusionEvaluationDb> mongoClient)
        {
            _mongoClient = mongoClient;
            _expressionSetColl = mongoClient.GetCollection<EntityScriptConfig>(settings.CollectionName);
        }

        public async Task<IEnumerable<EntityScript>> ReadAll()
        {
            var expressionConfigs = await _expressionSetColl.Find(_ => true).ToListAsync();
            return expressionConfigs.Select(ec => ec.ToEntity());
        }
    }
}

using MongoDB.Driver;
using NetFusion.Domain.Entity;
using NetFusion.Domain.Services;
using NetFusion.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetFusion.Integration.Domain.Evaluation
{
    public class ExpressionMetadataRepository : IExpressionMetadataRepository
    {
        private readonly IMongoDbClient<NetFusionExpressionDb> _mongoClient;
        private readonly IMongoCollection<EntityMetadataConfig> _expressionColl;

        public ExpressionMetadataRepository(NetFusionExpressionDb settings, IMongoDbClient<NetFusionExpressionDb> mongoClient)
        {
            _mongoClient = mongoClient;
            _expressionColl = mongoClient.GetCollection<EntityMetadataConfig>(settings.CollectionName);
        }

        public async Task<IEnumerable<EntityExpressionSet>> ReadAll()
        {
            var expressionConfigs = await _expressionColl.Find(_ => true).ToListAsync();
            return expressionConfigs.Select(ec => ec.ToEntity());
        }

        public Task SaveExpression(EntityExpression expression)
        {
            throw new NotImplementedException();
        }

        public Task SaveExpressions(IEnumerable<EntityExpression> expressions)
        {
            throw new NotImplementedException();
        }
    }
}

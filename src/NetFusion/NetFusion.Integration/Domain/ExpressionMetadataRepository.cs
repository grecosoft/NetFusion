using MongoDB.Driver;
using NetFusion.Domain.Entity;
using NetFusion.Domain.Services;
using NetFusion.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetFusion.Integration.Domain
{
    public class ExpressionMetadataRepository : IExpressionMetadataRepository
    {
        private readonly IMongoDbClient<NetFusionExpressionDb> _mongoClient;
        private readonly IMongoCollection<ExpressionMetadataConfig> _expressionColl;

        public ExpressionMetadataRepository(NetFusionExpressionDb settings, IMongoDbClient<NetFusionExpressionDb> mongoClient)
        {
            _mongoClient = mongoClient;
            _expressionColl = mongoClient.GetCollection<ExpressionMetadataConfig>(settings.CollectionName);
        }

        public async Task<IEnumerable<EntityPropertyExpression>> ReadAll()
        {
            var expressionConfigs = await _expressionColl.Find(_ => true).ToListAsync();
            return expressionConfigs.Select(ec => ec.ToEntity());
        }

        public Task SaveExpression(EntityPropertyExpression expression)
        {
            throw new NotImplementedException();
        }

        public Task SaveExpressions(IEnumerable<EntityPropertyExpression> expressions)
        {
            throw new NotImplementedException();
        }
    }
}

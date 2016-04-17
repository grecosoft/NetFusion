using MongoDB.Driver;
using NetFusion.MongoDB;
using NetFusion.RabbitMQ.Exchanges;
using NetFusion.RabbitMQ.Integration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetFusion.Integration.RabbitMQ
{
    public class ExchangeMetadataRepository : IExchangeRepository
    {
        private readonly IMongoDbClient<NetFusionDb> _dbClient;

        public ExchangeMetadataRepository(IMongoDbClient<NetFusionDb> dbClient)
        {
            _dbClient = dbClient;
        }

        public async Task<IEnumerable<ExchangeConfig>> LoadAsync(string brokerName)
        {

            var filter = Builders<ExchangeConfig>.Filter;
            return await _dbClient.GetCollection<ExchangeConfig>()
                .Find(filter.Where(e => e.BrokerName == null))
                .ToListAsync();
        }

        public void Save(IEnumerable<ExchangeConfig> exchanges)
        {
            _dbClient.GetCollection<ExchangeConfig>().InsertManyAsync(exchanges).Wait();
        }
    }
}

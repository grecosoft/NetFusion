using MongoDB.Driver;
using NetFusion.MongoDB;
using NetFusion.RabbitMQ.Core;
using NetFusion.RabbitMQ.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetFusion.RabbitMQ.MongoDB.Metadata
{
    /// <summary>
    /// Repository used to store exchange and queue meta-data used during a connection 
    /// failure when reconnecting to a potentially different broker.
    /// </summary>
    public class BrokerMetaRepository : IBrokerMetaRepository
    {
        private readonly IMongoDbClient<BrokerMetaDb> _dbClient;
        private readonly IMongoCollection<BrokerMeta> _brokerColl;

        public BrokerMetaRepository(BrokerMetaDb settings, IMongoDbClient<BrokerMetaDb> dbClient)
        {
            _dbClient = dbClient ?? throw new ArgumentNullException(nameof(dbClient));

            _brokerColl = _dbClient.GetCollection<BrokerMeta>(settings.CollectionName);
        }

        public async Task<IEnumerable<BrokerMeta>> LoadAsync(string brokerName)
        {
            if (brokerName == null) throw new ArgumentNullException(nameof(brokerName));

            var filter = Builders<BrokerMeta>.Filter;
            return await _brokerColl.Find(filter.Where(e => e.BrokerName == brokerName))
                .ToListAsync();
        }

        public async Task SaveAsync(IEnumerable<BrokerMeta> brokers)
        {
            brokers = brokers.ToList();
            if (brokers.Any())
            {
                foreach(BrokerMeta broker in brokers)
                {
                    await UpsertApplicationBrokerMetaAsync(broker);
                }
            }
        }

        private Task UpsertApplicationBrokerMetaAsync(BrokerMeta brokerMeta)
        {
            var filter = Builders<BrokerMeta>.Filter.Where(b => 
                b.BrokerName == brokerMeta.BrokerName 
                && b.ApplicationId == brokerMeta.ApplicationId);

            var options = new FindOneAndReplaceOptions<BrokerMeta, BrokerMeta>
            {
                IsUpsert = true
            };

            return _brokerColl.FindOneAndReplaceAsync(
                filter,
                brokerMeta, 
                options);
        }
    }
}

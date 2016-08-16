using NetFusion.MongoDB;
using NetFusion.RabbitMQ.Integration;

namespace NetFusion.Integration.RabbitMQ
{
    public class BrokerMetaMapping : EntityClassMap<BrokerMeta>
    {
        public BrokerMetaMapping()
        {
            this.CollectionName = "NetFusion.BrokerMeta";
            this.AutoMap();
            this.MapStringObjectIdProperty(p => p.BrockerConfigId);
            this.GetMemberMap(m => m.BrockerConfigId).SetIgnoreIfDefault(true);
        }
    }
}

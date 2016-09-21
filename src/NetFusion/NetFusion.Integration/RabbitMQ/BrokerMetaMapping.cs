using NetFusion.MongoDB;
using NetFusion.RabbitMQ.Integration;

namespace NetFusion.Integration.RabbitMQ
{
    public class BrokerMetaMapping : EntityClassMap<BrokerMeta>
    {
        public BrokerMetaMapping()
        {
            this.AutoMap();
            this.MapStringPropertyToObjectId(p => p.BrockerConfigId);
            this.GetMemberMap(m => m.BrockerConfigId).SetIgnoreIfDefault(true);
        }
    }
}

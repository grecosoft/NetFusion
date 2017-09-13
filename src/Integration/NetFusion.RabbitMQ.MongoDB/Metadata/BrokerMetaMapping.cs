using NetFusion.MongoDB;
using NetFusion.RabbitMQ.Integration;

namespace NetFusion.RabbitMQ.MongoDB.Metadata
{
    /// <summary>
    /// MongoDB data model collection mapping.
    /// </summary>
    public class BrokerMetaMapping : EntityClassMap<BrokerMeta>
    {
        public BrokerMetaMapping()
        {
            AutoMap();
            MapStringPropertyToObjectId(p => p.BrockerConfigId);
            GetMemberMap(m => m.BrockerConfigId).SetIgnoreIfDefault(true);
        }
    }
}

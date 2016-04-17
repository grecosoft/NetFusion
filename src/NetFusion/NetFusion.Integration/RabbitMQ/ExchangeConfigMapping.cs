using NetFusion.MongoDB;
using NetFusion.RabbitMQ.Integration;

namespace NetFusion.Integration.RabbitMQ
{
    public class ExchangeConfigMapping : EntityClassMap<ExchangeConfig>
    {
        public ExchangeConfigMapping()
        {
            this.CollectionName = "NetFusion.Exchanges";
            this.AutoMap();
            this.MapStringObjectIdProperty(p => p.ExchangeConfigId);
        }
    }
}

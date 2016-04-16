using System.Collections.Generic;
using System.Threading.Tasks;
using NetFusion.RabbitMQ.Integration;

namespace NetFusion.RabbitMQ.Exchanges
{
    public interface IExchangeRepository
    {
        void Save(IEnumerable<ExchangeConfig> exchanges);
        Task<IEnumerable<ExchangeConfig>> LoadAsync(string brokerName);
    }
}

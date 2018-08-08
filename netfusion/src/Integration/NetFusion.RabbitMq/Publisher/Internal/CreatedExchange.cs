using EasyNetQ.Topology;
using EasyNetQ;
using NetFusion.RabbitMQ.Metadata;

namespace NetFusion.RabbitMQ.Publisher.Internal
{
    /// <summary>
    /// Records the exchange that has been created for an associated definition.
    /// </summary>
    public class CreatedExchange
    {   
        /// <summary>
        /// The bus instance on which the exchange was created.
        /// </summary>
        public IBus Bus { get; }
        
        /// <summary>
        /// Reference to the created exchange.
        /// </summary>
        public IExchange Exchange { get; }
        
        /// <summary>
        /// The definition on which the created exchange was based.
        /// </summary>
        public ExchangeMeta Meta { get; }

        public CreatedExchange(
            IBus bus,
            IExchange exchange,
            ExchangeMeta defintion)
        {
            Bus = bus ?? throw new System.ArgumentNullException(nameof(bus));
            Exchange = exchange ?? throw new System.ArgumentNullException(nameof(exchange));
            Meta = defintion ?? throw new System.ArgumentNullException(nameof(defintion));
        }
    }
}
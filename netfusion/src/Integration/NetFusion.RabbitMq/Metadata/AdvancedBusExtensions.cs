using System;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;

namespace NetFusion.RabbitMQ.Metadata
{
    /// <summary>
    /// Extension methods for the EasyNetQ IAdvancedBus interface based on metadata classes
    /// used to specify exchanges and queues.
    /// </summary>
    public static class AdvancedBusExtensions
    {
        /// <summary>
        /// Creates an exchange on the message broker.  If the exchange is the default exchange
        /// with a defined queue, the queue is created on the RabbitMQ default exchange.
        /// </summary>
        /// <param name="bus">Reference to the advanced bus.</param>
        /// <param name="meta">The metadata describing the exchange.</param>
        /// <returns>Reference to the created exchange.</returns>
        public static async Task<IExchange> ExchangeDeclareAsync(this IAdvancedBus bus, ExchangeMeta meta)
        {
            if (bus == null) throw new ArgumentNullException(nameof(bus));
            if (meta == null) throw new ArgumentNullException(nameof(meta));

            // If the queue meta is specified on the exchange meta, this indicates that
            // a queue should be created on the default exchange.
            if (meta.QueueMeta != null)
            {
                await bus.QueueDeclareAsync(meta.QueueMeta.ScopedQueueName,
                    durable: meta.QueueMeta.IsDurable,
                    autoDelete: meta.QueueMeta.IsAutoDelete,
                    exclusive: meta.QueueMeta.IsExclusive,
                    passive: meta.QueueMeta.IsPassive,
                    maxPriority: meta.QueueMeta.MaxPriority,
                    deadLetterExchange: meta.QueueMeta.DeadLetterExchange,
                    deadLetterRoutingKey: meta.QueueMeta.DeadLetterRoutingKey,
                    perQueueMessageTtl: meta.QueueMeta.PerQueueMessageTtl);

                return Exchange.GetDefault();
            }
            
            return await bus.ExchangeDeclareAsync(meta.ExchangeName, meta.ExchangeType, 
                durable: meta.IsDurable,
                autoDelete: meta.IsAutoDelete, 
                passive: meta.IsPassive,
                alternateExchange: meta.AlternateExchangeName);
        }

        /// <summary>
        /// Creates a queue and its associated exchange on the message broker.  If the associated
        /// exchange is the RabbitMQ default exchange, only the queue is created.  For a non-default
        /// exchange, the queue is bound to the exchange.  if the metadata has route-keys specified,
        /// the queue is bound the to exchange for each specified key. 
        /// </summary>
        /// <param name="bus">Reference to the advanced bus.</param>
        /// <param name="meta">The metadata describing the queue.</param>
        /// <returns>Reference to the created queue.</returns>
        public static IQueue QueueDeclare(this IAdvancedBus bus, QueueMeta meta)
        {
            if (bus == null) throw new ArgumentNullException(nameof(bus));
            if (meta == null) throw new ArgumentNullException(nameof(meta));

            IExchange exchange = Exchange.GetDefault();
            ExchangeMeta exchangeMeta = meta.Exchange;
            
            if (! meta.Exchange.IsDefaultExchange)
            {
                exchange =  bus.ExchangeDeclare(exchangeMeta.ExchangeName, exchangeMeta.ExchangeType, 
                    durable: exchangeMeta.IsDurable,
                    autoDelete: exchangeMeta.IsAutoDelete, 
                    passive: exchangeMeta.IsPassive,
                    alternateExchange: exchangeMeta.AlternateExchangeName);
            }
            
            IQueue queue = bus.QueueDeclare(meta.ScopedQueueName, 
                durable: meta.IsDurable,
                autoDelete: meta.IsAutoDelete,
                exclusive: meta.IsExclusive,
                passive: meta.IsPassive,
                maxPriority: meta.MaxPriority,
                deadLetterExchange: meta.DeadLetterExchange,
                deadLetterRoutingKey: meta.DeadLetterRoutingKey,
                perQueueMessageTtl: meta.PerQueueMessageTtl);


            // Queues defined on the default exchange do not need to be bound.
            if (exchangeMeta.IsDefaultExchange)
            {
                return queue;
            }

            string[] routekeys = meta.RouteKeys ?? new string[] { };
            if (routekeys.Length > 0)
            {
                // If route-keys specified, bind the queue to the exchange
                // for each route-key.
                foreach (string routeKey in meta.RouteKeys ?? new string[] {})
                {
                    bus.Bind(exchange, queue, routeKey);
                }
            } 
            else 
            {
                bus.Bind(exchange, queue, string.Empty);
            }
            
            return queue;
        }
    }
}
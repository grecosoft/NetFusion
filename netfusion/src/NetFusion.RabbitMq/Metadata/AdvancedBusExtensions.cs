using System;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;

namespace NetFusion.RabbitMQ.Metadata
{
    /// <summary>
    /// Extension methods for the EasyNetQ IAdvancedBus interface based on metadata classes
    /// used to specify exchanges and queues.
    /// 
    /// https://github.com/EasyNetQ/EasyNetQ/wiki/The-Advanced-API
    /// </summary>
    public static class AdvancedBusExtensions
    {
        // Predefined Exchange and Queue names
        private const string UndeliveredExchangeName = "NetFusion_Undelivered";
        private const string UndeliveredQueueName = "NetFusion_Undelivered_Messages";
        private const string DeadLetterExchangeName = "NetFusion_DeadLetter";
        private const string DeadLetterQueueName = "NetFusion_DeadLetter_Messages";
        
        // --------------------- [Exchange and Queue Creation] ------------------- 
        
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
            // https://www.rabbitmq.com/tutorials/amqp-concepts.html#exchange-default
            if (meta.QueueMeta != null)
            {
                await CreateQueue(bus, meta.QueueMeta);
                return Exchange.GetDefault();
            }

            return await CreateExchange(bus, meta);
        }

        /// <summary>
        /// Creates a queue and its associated exchange on the message broker.  If the associated
        /// exchange is the RabbitMQ default exchange, only the queue is created.  For a non-default
        /// exchange, the queue is bound to the exchange.  If the metadata has route-keys specified,
        /// the queue is bound the to exchange for each specified key. 
        /// </summary>
        /// <param name="bus">Reference to the advanced bus.</param>
        /// <param name="meta">The metadata describing the queue.</param>
        /// <returns>Reference to the created queue.</returns>
        public static async Task<IQueue> QueueDeclare(this IAdvancedBus bus, QueueMeta meta)
        {
            if (bus == null) throw new ArgumentNullException(nameof(bus));
            if (meta == null) throw new ArgumentNullException(nameof(meta));

            IExchange exchange = Exchange.GetDefault(); //  Assume default exchange.
            if (! meta.ExchangeMeta.IsDefaultExchange)
            {
                exchange = await CreateExchange(bus, meta.ExchangeMeta);
            }

            IQueue queue = await CreateQueue(bus, meta);
            
            // Queues defined on the default exchange so don't bind.
            if (meta.ExchangeMeta.IsDefaultExchange)
            {
                return queue;
            }

            BindQueueToExchange(meta, bus, queue, exchange);
            return queue;
        }
        
        private static void BindQueueToExchange(QueueMeta meta, IAdvancedBus bus, IQueue queue, IExchange exchange)
        {
            string[] routeKeys = meta.RouteKeys ?? Array.Empty<string>();
            if (routeKeys.Length > 0)
            {
                // If route-keys specified, bind the queue to the exchange for each route-key.
                foreach (string routeKey in routeKeys)
                {
                    bus.Bind(exchange, queue, routeKey);
                }
            } 
            else 
            {
                bus.Bind(exchange, queue, string.Empty);
            }
        }
        
        
        // ---------------------- [Exchange Creation] -----------------------
        
        private static async Task<IExchange> CreateExchange(IAdvancedBus bus, ExchangeMeta meta)
        {
            IExchange undeliveredExchange = await CreateUndeliverableExchange(bus, meta);
            return await bus.ExchangeDeclareAsync(meta.ExchangeName, config =>
            {
                PopulateConfig(config, meta);
                if (undeliveredExchange != null)
                {
                    config.WithAlternateExchange(undeliveredExchange);
                }
            });
        }
        
        private static void PopulateConfig(IExchangeDeclareConfiguration config, ExchangeMeta meta)
        {
            config.WithType(meta.ExchangeType);
            config.AsDurable(meta.IsDurable);
            config.AsAutoDelete(meta.IsAutoDelete);
        }
        
        private static async Task<IExchange> CreateUndeliverableExchange(IAdvancedBus bus, ExchangeMeta meta)
        {
            if (! meta.IsNonRoutedSaved) return null; 
            
            // Declare an exchange that will be sent all un-routed messages.
            var altExchange = await bus.ExchangeDeclareAsync(UndeliveredExchangeName, 
                config => config.WithType(ExchangeType.Fanout).AsDurable(true)
            );

            var undeliveredQueue = await bus.QueueDeclareAsync(UndeliveredQueueName, config => config.AsDurable(true));
            
            bus.Bind(altExchange, undeliveredQueue, string.Empty);
            return altExchange;
        }
        
        
        // ---------------------- [Queue Creation] -----------------------
        
        private static async Task<IQueue> CreateQueue(IAdvancedBus bus, QueueMeta meta)
        {
            IExchange deadLetterExchange = await CreateDeadLetterExchange(bus, meta);
            return await bus.QueueDeclareAsync(meta.ScopedQueueName, config =>
            {
                PopulateConfig(config, meta);
                if (deadLetterExchange != null)
                {
                    config.WithDeadLetterExchange(deadLetterExchange);
                }
            });
        }

        private static void PopulateConfig(IQueueDeclareConfiguration config, QueueMeta meta)
        {
            config.AsDurable(meta.IsDurable);
            config.AsAutoDelete(meta.IsAutoDelete);
            config.AsExclusive(meta.IsExclusive);

            if (meta.MaxPriority != null)
            {
                config.WithMaxPriority(meta.MaxPriority.Value);
            }

            if (meta.PerQueueMessageTtl != null)
            {
                config.WithMessageTtl(TimeSpan.FromSeconds(meta.PerQueueMessageTtl.Value));
            }
        }
        
        private static async Task<IExchange> CreateDeadLetterExchange(IAdvancedBus bus, QueueMeta meta)
        {
            if (! meta.IsUnacknowledgedSaved) return null; 
            
            // Declare an exchange that will be sent all unprocessed messages.
            var deadLetterExchange = await bus.ExchangeDeclareAsync(DeadLetterExchangeName, 
                config => config.WithType(ExchangeType.Fanout).AsDurable(true)
            );

            var undeliveredQueue = await bus.QueueDeclareAsync(DeadLetterQueueName, config => config.AsDurable(true));
            
            bus.Bind(deadLetterExchange, undeliveredQueue, string.Empty);
            return deadLetterExchange;
        }
    }
}
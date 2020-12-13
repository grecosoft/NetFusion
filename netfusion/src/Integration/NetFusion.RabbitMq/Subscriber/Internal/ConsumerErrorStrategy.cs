using System;
using EasyNetQ.Consumer;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// The following will disable the default error strategy provided
    /// by EasyNetQ allowing the RabbitMQ x-dead-letter-exchange queue
    /// attribute to be used.
    /// </summary>
    public class ConsumerErrorStrategy : IConsumerErrorStrategy
    {
        public void Dispose()
        {
            
        }

        public AckStrategy HandleConsumerError(ConsumerExecutionContext context, Exception exception)
        {
           return AckStrategies.NackWithoutRequeue;
        }

        public AckStrategy HandleConsumerCancelled(ConsumerExecutionContext context)
        {
            return AckStrategies.NackWithRequeue;
        }
    }
}
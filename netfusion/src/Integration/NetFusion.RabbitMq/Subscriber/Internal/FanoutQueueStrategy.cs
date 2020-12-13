using System;
using System.Threading.Tasks;
using EasyNetQ.Topology;
using NetFusion.Messaging.Logging;
using NetFusion.RabbitMQ.Metadata;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// Factory for creating queues on a fanout exchange.  Sets the default conventions 
    /// used for a fanout exchange and created queues.
    /// 
    /// https://www.rabbitmq.com/tutorials/amqp-concepts.html#exchange-fanout
    /// </summary>
    internal class FanoutQueueStrategy : IQueueStrategy
    {
        public QueueMeta CreateQueueMeta(SubscriberQueueAttribute attribute)
        {
            var exchange = ExchangeMeta.Define(attribute.BusName, attribute.ExchangeName, ExchangeType.Fanout,
                config =>
                {
                    config.IsAutoDelete = true;
                    config.IsDurable = false;
                    config.IsPersistent = false;
                    config.IsNonRoutedSaved = attribute.IsNonRoutedSaved;
                });
            
            var queue = QueueMeta.Define(attribute.QueueName, exchange,
                config =>
                {
                    config.IsAutoDelete = true;
                    config.IsDurable = false;
                    config.IsExclusive = true;
                    config.AppendUniqueId = true;
                    config.IsUnacknowledgedSaved = attribute.IsUnacknowledgedSaved;
                });

            return queue;
        }

        public async Task OnMessageReceivedAsync(ConsumeContext context)
        {
            var message = context.DeserializeIntoMessage();
            
            var msgLog = new MessageLog(message, LogContextType.ReceivedMessage);
            msgLog.SentHint("subscribe-rabbitmq");
            
            context.LogReceivedMessage(message);
            context.AddMessageContextToLog(msgLog);
            
            try
            {
                await context.MessagingModule.InvokeDispatcherInNewLifetimeScopeAsync(
                    context.Subscriber.DispatchInfo, 
                    message);
            }
            catch (Exception ex)
            {
                msgLog.AddLogError("Queue Subscription", ex);
                throw;
            }
            finally { await context.MessageLogger.LogAsync(msgLog); }
        }
    }
}
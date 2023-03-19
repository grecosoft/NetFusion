using System;
using System.Threading.Tasks;
using EasyNetQ.Topology;
using NetFusion.Messaging.Logging;
using NetFusion.RabbitMQ.Metadata;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// Factory for creating queues on a topic exchange.  Sets the default conventions 
    /// used for a topic exchange and created queues.
    /// 
    /// https://www.rabbitmq.com/tutorials/amqp-concepts.html#exchange-topic
    /// </summary>
    internal class TopicQueueStrategy : IQueueStrategy
    {
        public QueueMeta CreateQueueMeta(SubscriberQueueAttribute attribute)
        {
            var exchange = ExchangeMeta.Define(attribute.BusName, attribute.ExchangeName, ExchangeType.Topic,
                meta =>
                {
                    meta.IsAutoDelete = false;
                    meta.IsDurable = true;
                    meta.IsPersistent = true;
                    meta.IsNonRoutedSaved = attribute.IsNonRoutedSaved;
                });
            
            var queue = QueueMeta.Define(attribute.QueueName, exchange,
                meta =>
                {
                    meta.IsAutoDelete = false;
                    meta.IsDurable = true;
                    meta.IsExclusive = false;
                    meta.AppendHostId = true;
                    meta.RouteKeys = attribute.RouteKeys;
                    meta.IsUnacknowledgedSaved = attribute.IsUnacknowledgedSaved;
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
            {    msgLog.AddLogError("Queue Subscription", ex);
                throw;
            }
            finally { await context.MessageLogger.LogAsync(msgLog); }
        }
    }
}
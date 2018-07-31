using EasyNetQ;
using NetFusion.Messaging.Modules;
using NetFusion.RabbitMQ.Modules;
using NetFusion.RabbitMQ.Serialization;

namespace NetFusion.RabbitMQ.Subscriber.Internal
{
    /// <summary>
    /// Class aggregating a set of references associated with
    /// the context of a received message.
    /// </summary>
    public class ConsumeContext 
    {
        public MessageProperties MessageProps { get; set; }
        public MessageReceivedInfo MessageReceiveInfo { get; set; }
        public MessageQueueSubscriber Subscriber { get; set; }

        // Services:
        public IBusModule BusModule { get; set; }
        public IMessageDispatchModule MessagingModule { get; set; }
        public ISerializationManager Serialization { get; set; }
    }
}
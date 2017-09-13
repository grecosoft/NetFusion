using NetFusion.Common;
using NetFusion.Messaging.Core;
using NetFusion.RabbitMQ.Consumers;
using NetFusion.RabbitMQ.Exchanges;
using System.Collections.Generic;

namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// Contains meta-data used when registering a consumer with a queue.  
    /// This meta-data extends the messaging dispatch meta-data defined by 
    /// the messaging plug-in.  The dispatch information is used to determine 
    /// the consumer to dispatch when a message arrives in the queue.
    /// </summary>
    public class MessageConsumer
    {
        private readonly BrokerAttribute _brokerAttrib;
        private readonly QueueConsumerAttribute _queueAttrib;
        private string _queueName;
        private string[] _routeKeys;

        /// <summary>
        /// The dispatch information used to invoke to consumer's message handler.
        /// </summary>
        public MessageDispatchInfo DispatchInfo { get; }

        public MessageConsumer(
            BrokerAttribute brokerAttrib,
            QueueConsumerAttribute queueAttrib,
            MessageDispatchInfo dispatchInfo)
        {
            Check.NotNull(brokerAttrib, nameof(brokerAttrib));
            Check.NotNull(queueAttrib, nameof(queueAttrib));
            Check.NotNull(dispatchInfo, nameof(dispatchInfo));

            _brokerAttrib = brokerAttrib;
            _queueAttrib = queueAttrib;
            _queueName = _queueAttrib.QueueName;
            _routeKeys = _queueAttrib.RouteKeys ?? new string[] { };

            MessageHandlers = new List<MessageHandler>();

            DispatchInfo = dispatchInfo;
        }

        /// <summary>
        /// The broker to which the consumer is connected.
        /// </summary>
        public string BrokerName
        {
            get { return _brokerAttrib.BrokerName; }
        }

        /// <summary>
        /// The name of the queue that the consumer is subscribed and
        /// will receive messages from.
        /// </summary>
        public string QueueName
        {
            get { return _queueName ?? ""; }
            set { _queueName = value; }
        }

        /// <summary>
        /// Indicates that the name of the queue was assigned by the broker.
        /// </summary>
        public bool IsBrokerAssignedName
        {
            get { return _queueAttrib.QueueSettings.IsBrokerAssignedName; }
        }

        /// <summary>
        /// The name of the associated exchange to which the queue is bound.
        /// </summary>
        public string ExchangeName
        {
            get { return _queueAttrib.ExchangeName ?? ""; }
        }

        /// <summary>
        /// Settings associated with the queue to which the consumer is subscribed.
        /// </summary>
        public QueueSettings QueueSettings => _queueAttrib.QueueSettings;

        /// <summary>
        /// How the consumer was bound to the queue.  The consumer will either 
        /// bind to an existing queue or indicate that they want to create a
        /// new queue specific to their needs with a specific routing keys.
        /// </summary>
        public QueueBindingTypes BindingType => _queueAttrib.BindingType; 

        /// <summary>
        /// The routing keys to use when binding a new consumer specific queue
        /// to an exchange.
        /// </summary>
        public string[] RouteKeys
        {
            get { return _routeKeys; }
            set { _routeKeys = value; }
        }
        
        /// <summary>
        /// These are the list of RabbitMQ consumers that have been created to process received messages 
        /// and dispatch them to the corresponding message handler method associated with the queue.  
        /// For heavy loads, the number of handler threads can be increased to handle multiple messages 
        /// concurrently.  This is specified within the broker configuration file.
        /// </summary>
        public List<MessageHandler> MessageHandlers { get; private set; }

    }
}

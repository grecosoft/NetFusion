using NetFusion.Common;
using NetFusion.Messaging.Core;
using NetFusion.RabbitMQ.Consumers;
using NetFusion.RabbitMQ.Exchanges;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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

            this.DispatchInfo = dispatchInfo;
        }

        public string BrokerName
        {
            get { return _brokerAttrib.BrokerName; }
        }

        public string QueueName
        {
            get { return _queueName ?? ""; }
            set { _queueName = value; }
        }

        public bool IsBrokerAssignedName
        {
            get { return _queueAttrib.QueueSettings.IsBrokerAssignedName; }
        }

        public string ExchangeName
        {
            get { return _queueAttrib.ExchangeName ?? ""; }
        }

        public QueueSettings QueueSettings => _queueAttrib.QueueSettings;
        public QueueBindingTypes BindingType => _queueAttrib.BindingType; 

        public string[] RouteKeys
        {
            get { return _routeKeys; }
            set { _routeKeys = value; }
        }
         
        // Properties set when the consumer is being bound to the queue
        // by the Message Broker class.
        public IModel Channel { get; set; }

        // Reference to the consumer of the queue.
        public EventingBasicConsumer Consumer { get; set; }
    }
}

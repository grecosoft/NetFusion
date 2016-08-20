using NetFusion.Common;
using NetFusion.Messaging.Core;
using NetFusion.RabbitMQ.Consumers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// Contains meta-data used when creating and binding
    /// the queue to an exchange.  This meta-data extends
    /// the eventing dispatch meta-data defined by the
    /// messaging plug-in.
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
            _routeKeys = _queueAttrib.RouteKeyValues ?? new string[] { };

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

        public QueueSettings QueueSettings { get { return _queueAttrib.QueueSettings; } }
        public QueueBindingTypes BindingType { get { return _queueAttrib.BindingType; } }

        public string[] RouteKeys
        {
            get { return _routeKeys; }
            set { _routeKeys = value; }
        }
         
        // Properties set when the consumer is being bound to the queue
        // by the Message Broker class.
        public IModel Channel { get; set; }

        public EventingBasicConsumer Consumer { get; set; }
    }
}

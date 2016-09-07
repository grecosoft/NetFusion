using NetFusion.Common;
using NetFusion.Common.Extensions;
using NetFusion.Domain.Scripting;
using NetFusion.Messaging;
using NetFusion.RabbitMQ.Exchanges;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetFusion.RabbitMQ.Core
{
    /// <summary>
    /// Declares exchanges to which messages can be published.  The base
    /// class from which specific type of exchanges derive.  Also responsible
    /// for creating queues and binding them to the exchange.
    /// </summary>
    public abstract class BrokerExchange : IMessageExchange
    {
        private readonly List<ExchangeQueue> _queues;
        private readonly QueueSettings _queueSettings;

        public ExchangeSettings Settings { get; }

        protected BrokerExchange()
        {
            _queues = new List<ExchangeQueue>();
            _queueSettings = new QueueSettings();

            this.Settings = new ExchangeSettings();
        }

        public string BrokerName { get { return Settings.BrokerName; } }
        public string ExchangeName { get { return Settings.ExchangeName; } }

        public Type MessageType { get; protected set; }
        public IEnumerable<ExchangeQueue> Queues { get { return _queues; } }

        /// <summary>
        /// Derived well know exchange type base classes such as Topic, Direct,
        /// Fan-out, RPC, and Work-queue can specify the default settings that
        /// should be used when creating a queue on the exchange.  These
        /// default settings are cloned to allow each specific application
        /// exchange to override the defaults.
        /// </summary>
        protected QueueSettings QueueSettings { get { return _queueSettings; } }

        // Called by Message Broker when defining exchanges and allows a 
        // specific declared application exchange to specific its settings.
        public void InitializeSettings()
        {
            SetOptionalScriptSettings();
            OnDeclareExchange();
            ValidateConfiguration();
        }

        // Called by Messaging Broker when the exchange is to be created.
        public virtual void Declare(IModel channel)
        {
            Check.NotNull(channel, nameof(channel));

            // Declare the exchange and its queues.
            CreateExchange(channel, this.Settings);
            CreateQueues(channel);
        }

        /// <summary>
        /// Implemented by derived exchanges to specify specific exchange settings
        /// and optionally the queues that should be created on the exchange.
        /// </summary>
        protected abstract void OnDeclareExchange();

        private void SetOptionalScriptSettings()
        {
            var scriptAttrib = this.GetAttribute<ApplyScriptPredicateAttribute>();
            if (scriptAttrib != null)
            {
                this.Settings.Predicate = scriptAttrib.ToPredicate();
            }
        }

        internal virtual void ValidateConfiguration()
        {
            if (Settings.BrokerName.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException(
                    "Broker Name must be set for all exchange types.");
            }
        }

        internal void ValidateRequireRouteKey()
        {
            if (this.Queues.Any(q => q.RouteKeys == null || q.RouteKeys.Empty()))
            {
                throw new InvalidOperationException(
                    $"For this type of exchange, all queues must have a route specified-" +
                    $"Exchange Type: {this.GetType()}.");
            }
        }

        /// <summary>
        /// Allows a derived exchange to configure queues that should be declared
        /// when the exchange is created.
        /// </summary>
        /// <param name="name">The name of the queue.</param>
        /// <param name="config">Configuration delegate that is specified
        /// to configure the queue.</param>
        protected void QueueDeclare(string name, Action<ExchangeQueue> config)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));
            Check.NotNull(config, nameof(config), "configuration delegate not specified");

            var exchangeQueue = new ExchangeQueue
            {
                QueueName = name,
                Settings = (QueueSettings)_queueSettings.Clone()
            };

            config(exchangeQueue);
            _queues.Add(exchangeQueue);
        }

        private void CreateExchange(IModel channel, ExchangeSettings settings)
        {
            // The default exchange is being used.
            if (settings.ExchangeType == null) return;

            if (this.ExchangeName.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException(
                    "Exchange name must be specified for non-default exchange type.");
            }

            channel.ExchangeDeclare(settings);
        }

        private void CreateQueues(IModel channel)
        {
            foreach (var queue in this.Queues)
            {
                channel.QueueDeclare(queue.QueueName, queue.Settings);
                if (queue.RouteKeys == null) continue;
                foreach (var routeKey in queue.RouteKeys)
                {
                    channel.QueueBind(queue.QueueName, this.ExchangeName, routeKey);
                }
            }
        }

        /// <summary>
        /// Publishes a message to the exchange.  The derived exchange can override how the
        /// message is published if needed.
        /// </summary>
        /// <param name="channel">The channel to publish the message on.</param>
        /// <param name="message">The message being published.</param>
        /// <param name="messageBody">The serialized message body.</param>
        /// <param name="replyToQueueName">For a RPC type message, the queue the client
        /// should use to respond.</param>
        public virtual void Publish(IModel channel, IMessage message, byte[] messageBody,
            string replyToQueueName = null)
        {
            Check.NotNull(channel, nameof(channel));
            Check.NotNull(message, nameof(message));
            Check.NotNull(messageBody, nameof(messageBody));

            var routeKey = message.GetRouteKey();
            var props = GetBasicProperties(channel, message);
            if (replyToQueueName != null)
            {
                props.ReplyTo = replyToQueueName;
            }

            channel.BasicPublish(this.ExchangeName ?? "", routeKey ?? "", props, messageBody);
        }

        /// <summary>
        /// Allows the derived exchange to specific the basic properties that should be
        /// specified when the message is published.
        /// </summary>
        /// <param name="channel">The Channel on which the message is to be published.</param>
        /// <param name="message">The message being published.</param>
        /// <returns>Configured set of properties.</returns>
        public virtual IBasicProperties GetBasicProperties(IModel channel, IMessage message)
        {
            Check.NotNull(channel, nameof(channel));
            Check.NotNull(message, nameof(message));

            string contentType = message.GetContentType();
            if (contentType == null)
            {
                throw new InvalidOperationException(
                    $"Content-Type not specified for message of type {message.GetType()}.");
            }

            var basicProps = channel.CreateBasicProperties();
            basicProps.ContentType = contentType;
            return basicProps;
        }

        public virtual bool Matches(IMessage message)
        {
            return true;
        }
    }
}

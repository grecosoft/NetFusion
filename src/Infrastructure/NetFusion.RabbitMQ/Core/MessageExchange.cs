using NetFusion.Base.Scripting;
using NetFusion.Common;
using NetFusion.Common.Extensions;
using NetFusion.Common.Extensions.Collection;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Domain.Messaging;
using NetFusion.RabbitMQ.Configs;
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
    public abstract class MessageExchange : IMessageExchange
    {
        private readonly List<ExchangeQueue> _queues;
        private readonly QueueSettings _queueSettings;

        public ExchangeSettings Settings { get; }

        protected MessageExchange()
        {
            _queues = new List<ExchangeQueue>();
            _queueSettings = new QueueSettings();

            this.Settings = new ExchangeSettings();
        }

        public string BrokerName => Settings.BrokerName;
        public string ExchangeName => Settings.ExchangeName;

        // The type of IMessage associated with the exchange.
        public Type MessageType { get; protected set; }
        public IEnumerable<ExchangeQueue> Queues => _queues;

        /// <summary>
        /// Derived well-know exchange style base classes such as Topic, Direct,
        /// Fan-out, RPC, and Work-queue can specify the default settings that
        /// should be used when creating a queue on the exchange. 
        /// </summary>
        protected QueueSettings QueueSettings => _queueSettings;

        // Called by Message Broker when defining exchanges and allows a 
        // specific declared application exchange to specific its settings.
        public void InitializeSettings(BrokerSettings brokerSettings)
        {
            SetOptionalScriptSettings();
            OnDeclareExchange();
            OnApplyConventions();

            brokerSettings.ApplyQueueSettings(this);

            ValidateConfiguration();
        }

        // The exchange definition can specify a script-predicate that
        // should be evaluated against the message to determine if it
        // should be published to the exchange.
        private void SetOptionalScriptSettings()
        {
            var scriptAttrib = this.GetAttribute<ApplyScriptPredicateAttribute>();
            if (scriptAttrib != null)
            {
                this.Settings.Predicate = scriptAttrib.ToPredicate();
            }
        }

        /// <summary>
        /// Implemented by derived exchanges to specify specific exchange settings
        /// and optionally the queues that should be created on the exchange.
        /// </summary>
        protected abstract void OnDeclareExchange();

        /// <summary>
        /// Allows the derived specific exchange type to set any conventions
        /// specific to its exchange type.
        /// </summary>
        protected virtual void OnApplyConventions()
        {

        }

        internal virtual void ValidateConfiguration()
        {
            if (Settings.BrokerName.IsNullOrWhiteSpace())
            {
                throw new BrokerException(
                    "Broker Name must be set for all exchange types.");
            }
        }

        // Called by Messaging Broker when the exchange is to be created
        // on the channel.
        public virtual void Declare(IModel channel)
        {
            Check.NotNull(channel, nameof(channel));

            // Declare the exchange and its queues.
            CreateExchange(channel, this.Settings);
            CreateQueues(channel);
        }

        private void CreateExchange(IModel channel, ExchangeSettings settings)
        {
            // The default exchange is being used if the ExchangeType is null.
            if (settings.ExchangeType == null) return;

            if (this.ExchangeName.IsNullOrWhiteSpace())
            {
                throw new BrokerException(
                    "Exchange name must be specified for non-default exchange type.");
            }

            channel.ExchangeDeclare(settings);
        }

        private void CreateQueues(IModel channel)
        {
            foreach (ExchangeQueue queue in this.Queues)
            {
                AssertQueue(queue);

                channel.QueueDeclare(queue.QueueName, queue.Settings);
                if (queue.RouteKeys == null) continue;

                foreach (string routeKey in queue.RouteKeys)
                {
                    channel.QueueBind(queue.QueueName, this.ExchangeName, routeKey.ToUpper());
                }
            }
        }

        private void AssertQueue(ExchangeQueue queue)
        {
            if (queue.RouteKeys != null)
            {
                if (queue.RouteKeys.Any(k => k == null))
                {
                    throw new InvalidOperationException(
                        $"The queue: {queue.QueueName} defined on exchange: {this.ExchangeName} has null route key values.");
                }
            }
        }

        public virtual bool Matches(IMessage message)
        {
            return true;
        }

        /// <summary>
        /// Allows a derived exchange to configure queues that should be declared
        /// when the exchange is created.
        /// </summary>
        /// <param name="name">The name of the queue.</param>
        /// <param name="config">Configuration delegate that is specified
        /// to configure the queue.</param>
        protected void QueueDeclare(string name, Action<ExchangeQueue> config = null)
        {
            Check.NotNullOrWhiteSpace(name, nameof(name));

            var exchangeQueue = new ExchangeQueue
            {
                QueueName = name,
                Settings = (QueueSettings)_queueSettings.Clone()
            };

            config?.Invoke(exchangeQueue);
            _queues.Add(exchangeQueue);
        }

        internal void ValidateRequiredRouteKey()
        {
            if (this.Queues.Any(q => q.RouteKeys == null || q.RouteKeys.Empty()))
            {
                throw new BrokerException(
                    $"For this type of exchange, all queues must have a route-key specified. " +
                    $"Exchange Type: {this.GetType()}.");
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

            string routeKey = message.GetRouteKey();
            var props = GetPublisherBasicProperties(channel, message);
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
        private IBasicProperties GetPublisherBasicProperties(IModel channel, IMessage message)
        {
            Check.NotNull(channel, nameof(channel));
            Check.NotNull(message, nameof(message));

            string contentType = message.GetContentType();
            if (contentType == null)
            {
                throw new BrokerException(
                    $"Content-Type not specified for message of type {message.GetType()}.");
            }

            IBasicProperties properties = channel.CreateBasicProperties();
            properties.ContentType = contentType;

            OnSetPublisherBasicProperties(channel, message, properties);

            return properties;
        }

        protected virtual void OnSetPublisherBasicProperties(IModel channel, IMessage message, IBasicProperties properties)
        {

        }
    }

    /// <summary>
    /// Message exchange for which specific message types can be published.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public abstract class MessageExchange<TMessage> : MessageExchange
        where TMessage : IMessage
    {
        public MessageExchange()
        {
            this.MessageType = typeof(TMessage);
        }

        public override bool Matches(IMessage message)
        {
            Check.NotNull(message, nameof(message));
            return Matches((TMessage)message);
        }

        /// <summary>
        /// Can be specified by a derived exchange to determine if the message
        /// passes the criteria required to be published to the exchange.
        /// </summary>
        /// <param name="message">The message about to be published to the
        /// exchange.</param>
        /// <returns>True if the message should be published to the exchange.
        /// Otherwise, false.</returns>
        protected virtual bool Matches(TMessage message)
        {
            return true;
        }
    }
}

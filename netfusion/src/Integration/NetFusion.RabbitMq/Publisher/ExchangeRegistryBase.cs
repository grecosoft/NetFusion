using System;
using System.Collections.Generic;
using EasyNetQ.Topology;
using NetFusion.Messaging.Types;
using NetFusion.RabbitMQ.Publisher.Internal;

namespace NetFusion.RabbitMQ.Publisher
{
    /// <summary>
    /// Base class used to specify exchanges/queues to which a given type of message
    /// should be delevered when published.
    /// </summary>
    public abstract class ExchangeRegistryBase : IExchangeRegistry
    {
        private readonly List<ExchangeDefinition> _exchanges = new List<ExchangeDefinition>();

        IEnumerable<ExchangeDefinition> IExchangeRegistry.GetDefinitions()
        {
            OnRegister();
            return _exchanges;
        }

        protected abstract void OnRegister();

        
        /// <summary>
        /// Defines a topic exchange to which messages of a specific type should be delevered
        /// when published.  For this type of exchange, the route-key associated with the message
        /// consists of a period delimited set of values.  Example:  Make.Model.Year.  Subscribers
        /// can specify patterns such as (where * means all values):  VW.GTI.2018, VW.*.2015.
        /// When multiple consumers are subscribed to defined queues on the exchange, messages are
        /// are delivered round-robin.
        /// </summary>
        /// <param name="name">The name of the exchange.</param>
        /// <param name="busName">The configured bus name on which the exchange should be created.</param>
        /// <param name="config">Delegate specified to set additional configurations.</param>
        /// <typeparam name="TMessage">The type of the domain-event associated with the exchange.</typeparam>
        /// <returns>Self reference.</returns>
        protected IExchangeRegistry DefineTopicExchange<TMessage>(string name, string busName = null,
            Action<ExchangeDefinition> config = null) 
            where TMessage : IDomainEvent
        {
           return CreateExchange<TMessage>(ExchangeType.Topic, name, busName, 
                defaults => {
                    // The default conventions for a topic exchange.
                    defaults.IsDurable = true;
                    defaults.IsAutoDelete = false;
                    defaults.IsPersistent = true;
                }, config);
        }

        /// <summary>
        /// Defines a direct exchange to which messages of a specific type should be delevered
        /// when published.  For this type of exchange, the route-key associated with the message
        /// is used to route messages to queues.  This is a simplified version of a topic exchange
        /// since no interpertation of the value is performed.  When multiple consumers are subscribed
        /// to the defined queues on the exchange, messages are delivered round-robin.
        /// </summary>
        /// <param name="name">The name of the exchange.</param>
        /// <param name="busName">The configured bus name on which the exchange should be created.</param>
        /// <param name="config">Delegate specified to set additional configurations.</param>
        /// <typeparam name="TMessage">The type of the domain-event associated with the exchange.</typeparam>
        /// <returns>Self reference.</returns>
        protected IExchangeRegistry DefineDirectExchange<TMessage>(string name, string busName = null,
            Action<ExchangeDefinition> config = null) 
            where TMessage : IDomainEvent
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return CreateExchange<TMessage>(ExchangeType.Direct, name, busName, 
                defaults => {
                    // The default conventions for a direct exchange.
                    defaults.IsDurable = true;
                    defaults.IsAutoDelete = false;
                    defaults.IsPersistent = true;
                }, config);
        }

        /// <summary>
        /// Defines a fanout exchange.  This type of exchange is often used for broadcasting notifications
        /// to all queues bound to the exchange.  For this type of exchange, mesages are only delivered
        /// when there is one or more subscribed consumer.  Each connected subscriber is assigned a queue
        /// on the exchange and messages are delivered to all subscribers.  When the subscriber disconnects
        /// from the exchange, the queue is deleted.  Likewise, when there are no subscribers connected to
        /// to the exchange, the exchange is deleted.
        /// </summary>
        /// <param name="name">The name of the exchange.</param>
        /// <param name="busName">The configured bus name on which the exchange should be created.</param>
        /// <param name="config">Delegate specified to set additional configurations.</param>
        /// <typeparam name="TMessage"></typeparam>
        /// <returns>Self Reference.</returns>
        protected IExchangeRegistry DefineFanoutExchange<TMessage>(string name, string busName = null,
            Action<ExchangeDefinition> config = null) 
            where TMessage : IDomainEvent
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            return CreateExchange<TMessage>(ExchangeType.Fanout, name, busName, 
                defaults => {
                    // The default conventions for a fanout exchange.
                    defaults.IsDurable = false;
                    defaults.IsAutoDelete = true;
                    defaults.IsPersistent = false;
                },config);
        }

        /// <summary>
        /// Defines a queue on the default exchange to which commands are delivered for processing.
        /// When messages are delivered, they are sent to the queue having a name matching that of
        /// the message's route-key.  All consumers subscribe to the same queues and messages are
        /// delivered round-robin for processing.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="busName">The configured bus name on which the queue should be created.</param>
        /// <typeparam name="TMessage">The type of the command associated with the queue.</typeparam>
        /// <returns>Self Reference.</returns>
        protected IExchangeRegistry DefineWorkQueue<TMessage>(string queueName, string busName = null)
            where TMessage : ICommand
        {
            if (queueName == null) throw new ArgumentNullException(nameof(queueName));

            var exDef = new ExchangeDefinition(busName, typeof(TMessage), queueName) {RouteKey = queueName};
            _exchanges.Add(exDef);

            return this;
        }

        /// <summary>
        /// Defines an exchange to which RPC style messages can be published.
        /// </summary>
        /// <param name="name">The name of the exchange to which the command should be published.</param>
        /// <param name="actionName">The value used by the consumer to identify the command.</param>
        /// <param name="busName">The configured bus name on which the queue should be created.</param>
        /// <typeparam name="TMessage">The type of the command published to the consumer.</typeparam>
        /// <returns>Self Reference.</returns>
        protected IExchangeRegistry DefineRpcExchange<TMessage>(string name, string actionName, 
            string busName = null)
            
            where TMessage : ICommand
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Exchange Name not Specified.", nameof(name));

            if (string.IsNullOrWhiteSpace(actionName))
                throw new ArgumentException("RPC Action Name not Specified.", nameof(actionName));

            var publisherStrategy = new RpcPublisherStrategy();

             return CreateExchange<TMessage>(ExchangeType.Direct, name, busName, 
                defaults => {
                    // The default conventions for a RPC style exchange.
                    defaults.IsDurable = false;
                    defaults.IsAutoDelete = true;
                    defaults.IsPersistent = false;
                    defaults.RouteKey = actionName;
                    defaults.IsRpcExchange = true;
                    defaults.SetPublisherStrategy(publisherStrategy);
                });
        }

        private IExchangeRegistry CreateExchange<TMessage>(string exchangeType, string exchangeName,
                string busName,
                Action<ExchangeDefinition> setDefaults,
                Action<ExchangeDefinition> config = null)
            where TMessage : IMessage
        {
            // Create the exchange defintion and call the delegate specified by the caller
            // to set the default exchange values to be used based on type of exchange.
            var exDef = new ExchangeDefinition(busName, typeof(TMessage), exchangeType, exchangeName);
            setDefaults(exDef);

            // Allow the initiating caller defining the exchange to set any additional configurations.
            config?.Invoke(exDef);

            _exchanges.Add(exDef);
            return this;
        }
    }
}


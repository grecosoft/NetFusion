using System;
using System.Collections.Generic;
using EasyNetQ.Topology;
using NetFusion.Messaging.Types;
using NetFusion.RabbitMQ.Metadata;
using NetFusion.RabbitMQ.Publisher.Internal;

namespace NetFusion.RabbitMQ.Publisher
{
    /// <summary>
    /// Base implementation of the IExchangeRegistry containing known message
    /// exchange patterns that can be derived from.
    /// </summary>
    public abstract class ExchangeRegistryBase : IExchangeRegistry
    {
        private readonly List<ExchangeMeta> _exchanges = new List<ExchangeMeta>();

        IEnumerable<ExchangeMeta> IExchangeRegistry.GetDefinitions()
        {
            OnRegister();
            return _exchanges;
        }

        /// <summary>
        /// Derived implementation should call one or more base exchange
        /// message pattern methods to declare exchanges and queues.
        /// </summary>
        protected abstract void OnRegister();

        /// <summary>
        /// Defines a topic exchange.  This type of exchange will route messages
        /// to bound queues matching a pattern based on the message's route-key.
        /// Example:  RouteKey:  VW.GTI.2018  Pattern:  VW.*.2018 
        /// </summary>
        /// <param name="name">The name of the exchange.</param>
        /// <param name="busName">The bus name key used to lookup connection.</param>
        /// <typeparam name="TMessage">The domain event assocated with the exchange.</typeparam>
        protected void DefineTopicExchange<TMessage>(string name, string busName) 
            where TMessage : IDomainEvent
        {
            var exchange = ExchangeMeta.Define(busName, name, ExchangeType.Topic, typeof(TMessage),
                config =>
            {
                config.IsAutoDelete = false;
                config.IsDurable = true;
                config.IsPersistent = true;
                config.IsPassive = false;
            });
            
            _exchanges.Add(exchange);
        }

        /// <summary>
        /// Defines a direct exchange.  This type of exchange will route messages
        /// to bound queues matching the message's route-key.
        /// </summary>
        /// <param name="name">The name of the exchange.</param>
        /// <param name="busName">The bus name key used to lookup connection.</param>
        /// <typeparam name="TMessage">The domain event assocated with the exchange.</typeparam>
        protected void DefineDirectExchange<TMessage>(string name, string busName = null) 
            where TMessage : IDomainEvent
        {
            var exchange = ExchangeMeta.Define(busName, name, ExchangeType.Direct, typeof(TMessage),
                config =>
            {
                config.IsAutoDelete = false;
                config.IsDurable = true;
                config.IsPersistent = true;
                config.IsPassive = false;
            });
            
            _exchanges.Add(exchange);
        }

        /// <summary>
        /// Defines a fanout exchange that will broadcast messages to all bound message queues.
        /// This should be used for notification type messages where the subscriber only cares
        /// about current occurring messages and does not care about prior missed messages that
        /// happened when they were offline.
        /// </summary>
        /// <param name="name">The name of the exchange.</param>
        /// <param name="busName">The bus name key used to lookup connection.</param>
        /// <typeparam name="TMessage">The domain event assocated with the exchange.</typeparam>
        protected void DefineFanoutExchange<TMessage>(string name, string busName) 
            where TMessage : IDomainEvent
        {
            var exchange = ExchangeMeta.Define(busName, name, ExchangeType.Fanout, typeof(TMessage),
                config =>
            {
                config.IsAutoDelete = true;
                config.IsDurable = false;
                config.IsPersistent = false;
                config.IsPassive = false;
            });
            
            _exchanges.Add(exchange);
        }

        /// <summary>
        /// Defines a work queue.  For the other type of exchanges, the publisher does not know
        /// the consumers subscribing to the queues defined on the exchange.  A work queue is used
        /// to make a request of another service by sending a command to the queue for processing.
        /// The processing is asynchronous and the publisher does not wait for a response.  This
        /// should be used by a publisher to submit a time consuming task to a specific subscriber
        /// for processing.
        /// </summary>
        /// <param name="queueName">The name of the queue on which the subscriber will recieve commands.</param>
        /// <param name="busName">The bus name key used to lookup connection.</param>
        /// <typeparam name="TMessage">The command type associated with the queue.</typeparam>
        protected void DefineWorkQueue<TMessage>(string queueName, string busName)
            where TMessage : ICommand
        {
            var exchange = ExchangeMeta.DefineDefault(busName, queueName, typeof(TMessage),
                config =>
            {
                config.IsAutoDelete = false;
                config.IsDurable = true;
                config.IsPassive = false;
                config.IsExclusive = false;
            });

            exchange.RouteKey = queueName;
            _exchanges.Add(exchange);
        }

        /// <summary>
        /// Implements RPC over an asynchronous message bus.  While the publisher does not block
        /// and can complete other unrelated work, the publisher is synchronous in terms that they
        /// can't complete their entire tasks until the reponse is received.
        /// </summary>
        /// <param name="queueName">The name of the queue on which the subscriber will recieve commands.</param>
        /// <param name="actionName">For a given RPC command, identifies to the subscriber to associated action.</param>
        /// <param name="busName">The bus name key used to lookup connection.</param>
        /// <typeparam name="TMessage">The command type associated with the queue.</typeparam>
        protected void DefineRpcQueue<TMessage>(string queueName, string actionName, 
            string busName)
            
            where TMessage : ICommand
        {
            if (string.IsNullOrWhiteSpace(actionName))
                throw new ArgumentException("RPC Action Name not Specified.", nameof(actionName));

            var publisherStrategy = new RpcPublisherStrategy();
            
            var exchange = ExchangeMeta.DefineDefault(busName, queueName, typeof(TMessage),
                config =>
                {
                    config.IsAutoDelete = true;
                    config.IsDurable = false;
                    config.IsPassive = false;
                    config.IsExclusive = false;
                });

            exchange.IsRpcExchange = true;
            exchange.ActionName = actionName;
            exchange.SetPublisherStrategy(publisherStrategy);
            
            _exchanges.Add(exchange);
        }
    }
}


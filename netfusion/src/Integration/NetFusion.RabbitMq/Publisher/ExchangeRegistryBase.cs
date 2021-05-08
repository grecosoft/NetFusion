using System;
using System.Collections.Generic;
using EasyNetQ.Topology;
using NetFusion.Messaging.Types.Contracts;
using NetFusion.RabbitMQ.Metadata;
using NetFusion.RabbitMQ.Publisher.Internal;

namespace NetFusion.RabbitMQ.Publisher
{
    /// <summary>
    /// Base implementation of the IExchangeRegistry containing known message
    /// exchange patterns.  Applications derive from this class to specify the
    /// Exchange/Queue to which a given message type should be published.
    /// </summary>
    public abstract class ExchangeRegistryBase : IExchangeRegistry
    {
        private readonly List<ExchangeMeta> _exchanges = new();

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
        /// 
        /// https://www.rabbitmq.com/tutorials/amqp-concepts.html#exchange-topic
        /// </summary>
        /// <param name="name">The name of the exchange.</param>
        /// <param name="busName">The bus name key used to lookup connection.</param>
        /// <param name="settings">Optional delegate called allowing caller to set
        /// additional exchange properties.</param>
        /// <typeparam name="TMessage">The domain-event type associated with the exchange.</typeparam>
        protected void DefineTopicExchange<TMessage>(string name, string busName,
            Action<ExchangeMeta<TMessage>> settings = null) 
            where TMessage : IDomainEvent
        {
            var exchange = ExchangeMeta.Define<TMessage>(busName, name, ExchangeType.Topic,
                meta =>
            {
                meta.IsAutoDelete = false;
                meta.IsDurable = true;
                meta.IsPersistent = true;
            });
            
            _exchanges.Add(exchange);
            settings?.Invoke(exchange);
        }

        /// <summary>
        /// Defines a direct exchange.  This type of exchange will route messages
        /// to bound queues matching the message's route-key.
        /// 
        /// https://www.rabbitmq.com/tutorials/amqp-concepts.html#exchange-direct
        /// </summary>
        /// <param name="name">The name of the exchange.</param>
        /// <param name="busName">The bus name key used to lookup connection.</param>
        /// <param name="settings">Optional delegate called allowing caller to set
        /// additional exchange properties.</param>
        /// <typeparam name="TMessage">The domain-event type associated with the exchange.</typeparam>
        protected void DefineDirectExchange<TMessage>(string name, string busName = null,
            Action<ExchangeMeta<TMessage>> settings = null)
            where TMessage : IDomainEvent
        {
            var exchange = ExchangeMeta.Define<TMessage>(busName, name, ExchangeType.Direct,
                meta =>
            {
                meta.IsAutoDelete = false;
                meta.IsDurable = true;
                meta.IsPersistent = true;
            });
            
            _exchanges.Add(exchange);
            settings?.Invoke(exchange);
        }

        /// <summary>
        /// Defines a fan-out exchange that will broadcast messages to all bound message queues.
        /// This should be used for notification type messages where the subscriber only cares
        /// about current occurring messages and not about prior missed messages that happened
        /// when they were offline.
        /// 
        /// https://www.rabbitmq.com/tutorials/amqp-concepts.html#exchange-fanout
        /// </summary>
        /// <param name="name">The name of the exchange.</param>
        /// <param name="busName">The bus name key used to lookup connection.</param>
        /// <param name="settings">Optional delegate called allowing caller to set
        /// additional exchange properties.</param>
        /// <typeparam name="TMessage">The domain-event type associated with the exchange.</typeparam>
        protected void DefineFanoutExchange<TMessage>(string name, string busName,
            Action<ExchangeMeta<TMessage>> settings = null) 
            where TMessage : IDomainEvent
        {
            var exchange = ExchangeMeta.Define<TMessage>(busName, name, ExchangeType.Fanout,
                meta =>
            {
                meta.IsAutoDelete = true;
                meta.IsDurable = false;
                meta.IsPersistent = false;
            });
            
            _exchanges.Add(exchange);
            settings?.Invoke(exchange);
        }

        /// <summary>
        /// Defines a work queue.  For the other type of exchanges, the publisher does not know
        /// the consumers subscribing to the queues defined on the exchange.  A work queue is used
        /// to make a request of another service by sending a command to the queue for processing.
        /// The processing is asynchronous and the publisher does not wait for a response.  This
        /// should be used by a publisher to submit a time consuming task to a specific service
        /// for processing.
        /// </summary>
        /// <param name="queueName">The name of the queue on which the consuming service will receive commands.</param>
        /// <param name="busName">The bus name key used to lookup connection.</param>
        /// <param name="settings">Optional delegate called allowing caller to set
        /// additional exchange properties.</param>
        /// <typeparam name="TMessage">The command type associated with the queue.</typeparam>
        protected void DefineWorkQueue<TMessage>(string queueName, string busName,
            Action<ExchangeMeta<TMessage>> settings = null) 
            where TMessage : ICommand
        {
            var exchange = ExchangeMeta.DefineDefault<TMessage>(busName, queueName,
                meta =>
            {
                meta.IsAutoDelete = false;
                meta.IsDurable = true;
                meta.IsExclusive = false;
            });

            exchange.RouteKey = queueName;

            _exchanges.Add(exchange);
            settings?.Invoke(exchange);
        }

        /// <summary>
        /// Implements RPC over an asynchronous message bus.  While the publisher does not block
        /// and can complete other unrelated work, the publisher is synchronous in terms that they
        /// can't complete their entire work until the response is received.
        /// </summary>
        /// <param name="queueName">The name of the queue on which the subscriber will receive commands.</param>
        /// <param name="actionNamespace">For a given RPC command, identifies to the consumer the associated action.</param>
        /// <param name="busName">The bus name key used to lookup connection.</param>
        /// <param name="settings">Optional delegate called allowing caller to set additional exchange properties.</param>
        /// <typeparam name="TMessage">The command type associated with the queue.</typeparam>
        protected void DefineRpcQueue<TMessage>(string queueName, string actionNamespace, string busName,
            Action<ExchangeMeta<TMessage>> settings = null)
            where TMessage : ICommand
        {
            if (string.IsNullOrWhiteSpace(actionNamespace))
                throw new ArgumentException("RPC Action Namespace not Specified.", nameof(actionNamespace));

            var publisherStrategy = new RpcPublisherStrategy();
            
            var exchange = ExchangeMeta.DefineDefault<TMessage>(busName, queueName,
                meta =>
                {
                    meta.IsAutoDelete = true;
                    meta.IsDurable = false;
                    meta.IsExclusive = false;
                });

            exchange.IsAutoDelete = true;
            exchange.IsRpcExchange = true;
            exchange.ActionNamespace = actionNamespace;
            exchange.SetPublisherStrategy(publisherStrategy);
            
            _exchanges.Add(exchange);
            settings?.Invoke(exchange);
        }
    }
}


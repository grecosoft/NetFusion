using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetFusion.Azure.ServiceBus.Namespaces.Internal;
using NetFusion.Azure.ServiceBus.Publisher;
using NetFusion.Azure.ServiceBus.Subscriber;
using NetFusion.Azure.ServiceBus.Subscriber.Internal;
using NetFusion.Messaging.Types;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Azure.ServiceBus.Namespaces
{
    /// <summary>
    /// Derived instances are used to configure metadata about the namespace entities to be created.
    /// The metadata specifies to which message broker entities commands and domain-events are published.  
    /// </summary>
    public abstract class NamespaceRegistryBase : INamespaceRegistry
    {
        public string NamespaceName { get; }
        
        protected NamespaceRegistryBase(string namespaceName)
        {
            if (string.IsNullOrWhiteSpace(namespaceName))
                throw new ArgumentException("Namespace must be specified.", nameof(namespaceName));
            
            NamespaceName = namespaceName;
        }
        
        private readonly List<NamespaceEntity> _namespaceEntities = new();
        private EntitySubscription[] _subscriptions = Array.Empty<EntitySubscription>();
        
        // ---------------------- Explicit Interface --------------------------
        
        IEnumerable<NamespaceEntity> INamespaceRegistry.GetNamespaceEntities()
        {
            OnDefineNamespace();
            return _namespaceEntities;
        }

        void INamespaceRegistry.ConfigureSubscriptions(IEnumerable<EntitySubscription> subscriptions)
        {
            _subscriptions = subscriptions.ToArray();
            
            OnConfigureSubscriptions();
        }
        
        /// <summary>
        /// Called when the service is bootstrapped to allow derived classes to configure
        /// the entities to be created within the namespace. 
        /// </summary>
        protected abstract void OnDefineNamespace();

        /// <summary>
        /// Called when the service is bootstrapped to allow derived classes to apply
        /// additional configurations to the created subscriptions. 
        /// </summary>
        protected virtual void OnConfigureSubscriptions() { }
        
        
        // ------------------- Namespace Entities ----------------------
        
        /// <summary>
        /// Used to define a topic to be created within the namespace to which Domain Events
        /// are published.  Other Microservices interesting in the domain-event, create and
        /// subscribe to subscriptions defined on the topic.
        /// </summary>
        /// <param name="topicName">The name of the topic to be declared.</param>
        /// <param name="topicConfig">Optional delegate used to apply additional configurations.</param>
        /// <typeparam name="TDomainEvent">The type of the Domain Event message to be published to the topic.</typeparam>
        protected void CreateTopic<TDomainEvent>(string topicName, Action<TopicMeta<TDomainEvent>> topicConfig = null)
            where TDomainEvent : IDomainEvent
        {
            if (string.IsNullOrWhiteSpace(topicName))
                throw new ArgumentException("Topic Name not specified.", nameof(topicName));
            
            var existingTopic = _namespaceEntities.OfType<TopicMeta>()
                .FirstOrDefault(t => t.EntityName == topicName);
            
            if (existingTopic != null)
            {
                throw new InvalidOperationException(
                    $"A topic named {topicName} has already been declared within namespace {NamespaceName} for " +
                    $"the message {existingTopic.MessageType}.");
            }
            
            var topic = new TopicMeta<TDomainEvent>(NamespaceName, topicName);
            topicConfig?.Invoke(topic);
            
            _namespaceEntities.Add(topic);
        }

        /// <summary>
        /// Creates a queue within the namespace to which other Microservices can route commands.
        /// </summary>
        /// <param name="queueName">The name of the queue within the namespace to be created.</param>
        /// <param name="queueConfig">Optional delegate used to apply additional configurations.</param>
        /// <typeparam name="TCommand">The type of the command message associated with the queue.</typeparam>
        protected void CreateQueue<TCommand>(string queueName, Action<QueueMeta<TCommand>> queueConfig = null)
            where TCommand : ICommand
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue Name not specified.", nameof(queueName));
            
            AssertUniqueQueue(queueName);

            var queue = new QueueMeta<TCommand>(NamespaceName, queueName);
            queueConfig?.Invoke(queue);
            
            _namespaceEntities.Add(queue);
        }
        
        /// <summary>
        /// Creates a queue on the Service Bus to which Azure Service Bus routes messages.
        /// Microservices do not directly send message to these queues.
        /// </summary>
        /// <param name="queueName">The name of the queue within the namespace to be created.</param>
        /// <param name="queueConfig">Optional delegate used to apply additional configurations.</param>
        protected void CreateSecondaryQueue(string queueName, Action<QueueMeta> queueConfig = null)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue Name not specified.", nameof(queueName));
            
            AssertUniqueQueue(queueName);

            var queue = new QueueMeta(NamespaceName, queueName, typeof(IMessage))
            {
                IsSecondaryQueue = true
            };
            
            queueConfig?.Invoke(queue);
            _namespaceEntities.Add(queue);
        }

        /// <summary>
        /// Create a queue to which other services can send RPC based commands.  The response from
        /// the invoked command handler will be delivered back to the publisher on the specified
        /// reply queue.
        /// </summary>
        /// <param name="queueName">The name of the queue RPC commands will be received.</param>
        /// <param name="queueConfig">Optional delegate used to apply additional configurations.</param>
        protected void CreateRpcQueue(string queueName, Action<QueueMeta> queueConfig = null)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue Name not specified.", nameof(queueName));
            
            AssertUniqueQueue(queueName);

            var queue = new QueueMeta(NamespaceName, queueName, typeof(IMessage));
            queueConfig?.Invoke(queue);
            
            _namespaceEntities.Add(queue);
        }

        private void AssertUniqueQueue(string queueName)
        {
            var existingQueue = _namespaceEntities.OfType<QueueMeta>()
                .FirstOrDefault(q => q.EntityName == queueName);

            if (existingQueue != null)
            {
                throw new InvalidOperationException(
                    $"A queue named {queueName} within namespace {NamespaceName} has already been declared " + 
                    $"for the message {existingQueue.MessageType}.");
            }
        }
        
        /// <summary>
        /// Used to define an existing queue, defined and consumed by another Microservice, to which the
        /// current publishing service sends commands. If the command expects an asynchronous response
        /// in the future, the publisher can specify a reply queue using the ReplyToQueueName property
        /// of QueueSourceMeta.     
        /// </summary>
        /// <param name="queueName">The name of the queue within the namespace to send the command.</param>
        /// <param name="queueConfig">Optional delegate used to apply additional configurations.</param>
        /// <typeparam name="TCommand">The type of the Command message to be send to the queue.</typeparam>
        protected void RouteToQueue<TCommand>(string queueName, Action<QueueSourceMeta<TCommand>> queueConfig = null)
            where TCommand : ICommand
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue Name not specified.", nameof(queueName));
            
            if (_namespaceEntities.OfType<QueueMeta>().Any(m => m.EntityName == queueName))
            {
                // If a queue has been defined by this Microservice, then there is no reason 
                // to add an entry indicating the queue to which the message should be routed.
                // Usually, another Microservice defines the queue and others route the message.
                return;
            }

            var existingQueueSource = _namespaceEntities.OfType<QueueSourceMeta>()
                .FirstOrDefault(q => q.EntityName == queueName);

            if (existingQueueSource != null)
            {
                throw new InvalidOperationException(
                    $"A queue named {queueName} within the namespace {NamespaceName} for a command of " + 
                    $"type {existingQueueSource.MessageType} has already been configured.");
            }
            
            var queue = new QueueSourceMeta<TCommand>(NamespaceName, queueName);
            queueConfig?.Invoke(queue);
            
            _namespaceEntities.Add(queue);
        }

        /// <summary>
        /// Specifies the Queue to which a RPC Command should be sent by the publisher.  A unique string referred
        /// to as the message namespace is used to identify the message to the consumer.  The message namespace
        /// allows multiple related RPC based messages to be processed on a single queue.  Unlike a regular Queue,
        /// that can process command results on another queue when ready, the publisher of a RPC command awaits
        /// the response until ready.  Therefore, RPC commands should be used when the response has low latency
        /// and the response can't be processed asynchronously.  
        /// </summary>
        /// <param name="queueName">The Queue name defined by the service that will process the command.</param>
        /// <param name="messageNamespace">The optional namespace used to identify the command.  If not provided,
        /// the namespace will be read from the MessageNamespace attribute specified on the message.  If the
        /// message namespace can't be determined, an exception is raised.</param>
        /// <typeparam name="TCommand"></typeparam>
        protected void RouteToRpcQueue<TCommand>(string queueName, string messageNamespace = null)
            where TCommand : ICommand
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue name not specified.", nameof(queueName));
            
            string assignedMessageNamespace = GetMessageNamespace(typeof(TCommand), messageNamespace); 
            var replayQueue = AssureReplyQueue(queueName);

            var existingQueueSource = _namespaceEntities.OfType<RpcQueueSourceMeta>()
                .FirstOrDefault(q => q.EntityName == queueName && q.MessageNamespace == assignedMessageNamespace);

            if (existingQueueSource != null)
            {
                throw new InvalidOperationException(
                    $"A RPC queue named {queueName} for the message namespace {assignedMessageNamespace} to which a " + 
                    $"command of type {existingQueueSource.MessageType} should be routed has already been configured " +
                    $"for namespace {NamespaceName}.");
            }
            
            var queue = new RpcQueueSourceMeta<TCommand>(assignedMessageNamespace, replayQueue);

            _namespaceEntities.Add(queue);
        }

        private RpcReplyQueryMeta AssureReplyQueue(string queueName)
        {
            var replyQueue = _namespaceEntities.OfType<RpcReplyQueryMeta>()
                .FirstOrDefault(q => q.EntityName == queueName);

            if (replyQueue == null)
            {
                replyQueue = new RpcReplyQueryMeta(NamespaceName, queueName);
                _namespaceEntities.Add(replyQueue);
            }

            return replyQueue;
        }

        private static string GetMessageNamespace(Type messageType, string messageNamespace)
        {
            string assignedNamespace = messageNamespace ?? messageType
                .GetCustomAttribute<MessageNamespaceAttribute>()?.MessageNamespace;
            
            if (assignedNamespace == null)
            {
                throw new InvalidOperationException(
                    $"Message namespace not specified and could not be determined for RPC command {messageType}");
            }

            return assignedNamespace;
        }
        
        /// <summary>
        /// Creates a queue to which a response to a command, processed by another Microservice, is delivered
        /// back to the originating Microservice.
        /// </summary>
        /// <param name="queueName">The name of the receiving queue.</param>
        /// <param name="queueConfig">Optional delegate used to apply additional configurations.</param>
        /// <typeparam name="TMessage">The type of the response message associated with the queue.</typeparam>
        protected void CreateResponseQueue<TMessage>(string queueName, Action<QueueMeta<TMessage>> queueConfig = null)
            where TMessage : IMessage
        {
            AssertUniqueQueue(queueName);
            
            var queue = new QueueMeta<TMessage>(NamespaceName, queueName);
            queueConfig?.Invoke(queue);
            
            _namespaceEntities.Add(queue);
        }

        
        // ---------------- Entity Subscriptions -------------------
        
        /// <summary>
        /// Allows configuring a subscription defined for a specified topic.  This method can be used
        /// to reference the created subscriptions to configure additional properties such as rule filters.
        /// </summary>
        /// <param name="topicName">The name of the topic.</param>
        /// <param name="subscriptionName">The name of the subscription.</param>
        /// <param name="config">Delegate called to apply additional configurations.</param>
        protected void ConfigTopicSubscription(string topicName, string subscriptionName,
            Action<TopicSubscription> config)
        {
            if (string.IsNullOrWhiteSpace(topicName))
                throw new ArgumentException("Topic Name not specified.", nameof(topicName));
            
            if (string.IsNullOrWhiteSpace(subscriptionName))
                throw new ArgumentException("Subscription Name not specified.", nameof(subscriptionName));
            
            if (config == null) throw new ArgumentNullException(nameof(config));
            
            TopicSubscription subscription = GetTopicSubscription(topicName, subscriptionName);
            config(subscription);
        }

        private TopicSubscription GetTopicSubscription(string topicName, string subscriptionName)
        {
            var subscription = _subscriptions.OfType<TopicSubscription>()
                .FirstOrDefault(ts =>
                    ts.EntityName == topicName &&
                    ts.SubscriptionName == subscriptionName);

            if (subscription == null)
            {
                throw new InvalidOperationException(
                    $"Subscription {subscriptionName} for topic {topicName} within namespace {NamespaceName} " +
                    $"is not configured. Validate a message handler method is decorated with {nameof(TopicSubscriptionAttribute)}.");
            }

            return subscription;
        }

        /// <summary>
        /// Allows configuring a subscription defined for a specified queue.  This method can be used
        /// to reference the created subscription to configure additional properties.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <param name="config">Delegate called to apply additional configurations.</param>
        protected void ConfigQueueSubscription(string queueName, Action<QueueSubscription> config)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException($"Queue Name not specified.", nameof(queueName));
            
            if (config is null) throw new ArgumentNullException(nameof(config));

            QueueSubscription subscription = GetQueueSubscription(queueName);
            config(subscription);
        }

        private QueueSubscription GetQueueSubscription(string queueName)
        {
            var subscription = _subscriptions.OfType<QueueSubscription>()
                .FirstOrDefault(qs => qs.EntityName == queueName);

            if (subscription == null)
            {
                throw new InvalidCastException(
                    $"Subscription for queue {queueName} within namespace {NamespaceName} is not configured." +
                    $"Validate a message handler method is decorated with {nameof(QueueSubscriptionAttribute)}.");
            }

            return subscription;
        }
    }
}
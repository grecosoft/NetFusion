﻿using System;

namespace NetFusion.RabbitMQ.Consumers
{
    /// <summary>
    /// Attribute used to specify that a existing queue should be bound to
    /// by the consumer.  The method decorated with the attribute will be
    /// invoked when a new message is received by the queue.  The consumer
    /// will join any other consumers and be invoked round-robin.
    /// </summary>
    public class JoinQueueAttribute : QueueConsumerAttribute
    {
        /// <summary>
        /// Joins to an existing queue on an exchange.  The consumer will
        /// join any other consumers and be invoked round-robin.
        /// </summary>
        /// <param name="queueName">The name of the queue to join.</param>
        /// <param name="exchangeName">The name of the exchange defining
        /// the queue.</param>
        public JoinQueueAttribute(string queueName, string exchangeName) :
            base (queueName, exchangeName, QueueBindingTypes.Join)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue name must be specified.", nameof(queueName));

            if (string.IsNullOrWhiteSpace(exchangeName))
                throw new ArgumentException("Exchange name must be specified.", nameof(exchangeName));
        }

        /// <summary>
        /// Joins to an existing queue on the default exchange.
        /// </summary>
        /// <param name="queueName">The name of the queue to join.</param>
        public JoinQueueAttribute(string queueName) :
           base(queueName, null, QueueBindingTypes.Join)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue name must be specified.", nameof(queueName));
        }
    }
}

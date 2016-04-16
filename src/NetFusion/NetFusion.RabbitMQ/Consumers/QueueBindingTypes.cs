namespace NetFusion.RabbitMQ.Consumers
{
    /// <summary>
    /// Indicates how the consumer should be bound to a queue.
    /// </summary>
    public enum QueueBindingTypes
    {
        /// <summary>
        /// The consumer will bind to an existing queue created by the
        /// exchange.  The consumer will join any other consumers and
        /// be invoked round-robin.
        /// </summary>
        Join = 1,

        /// <summary>
        /// The consumer will create a new queue on the exchange.
        /// </summary>
        Create = 2
    }
}

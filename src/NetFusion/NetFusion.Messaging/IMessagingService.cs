using System.Threading.Tasks;

namespace NetFusion.Messaging
{
    /// <summary>
    /// Service containing methods to publish messages to all registered
    /// message dispatchers for delivery to their corresponding consumers.
    /// </summary>
    public interface IMessagingService
    {
        /// <summary>
        /// Publishes a command to all associated consumer message handlers.
        /// </summary>
        /// <param name="command">The command to be published.</param>
        /// <returns>Future result.</returns>
        Task PublishAsync(ICommand command);

        /// <summary>
        /// Publishes a command to all associated consumer message handlers
        /// and returns the consumer's response.</summary>
        /// <param name="command">The event to be published.</param>
        /// <returns>Future result.</returns>
        Task<TResult> PublishAsync<TResult>(ICommand<TResult> command);

        /// <summary>
        /// Publishes a domain-event to all associated consumer message handlers.
        /// </summary>
        /// <param name="domainEvent">The event to be published.</param>
        /// <returns>Future result.</returns>
        Task PublishAsync(IDomainEvent domainEvent);

        /// <summary>
        /// Publishes a domain-event to all local in-process consumer message handlers.  
        /// If any of the message handlers are asynchronous, an exception is thrown.
        /// </summary>
        /// <param name="domainEvent">The domain event to publish.</param>
        void PublishInProcess(IDomainEvent domainEvent);

        /// <summary>
        /// Publishes domain-events associated with an event source.  
        /// </summary>
        /// <param name="eventSource">The event source having associated events.</param>
        /// <returns>Future result.</returns>
        Task PublishAsync(IEventSource eventSource);
    }
}

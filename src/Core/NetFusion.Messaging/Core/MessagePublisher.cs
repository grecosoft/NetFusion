using NetFusion.Messaging.Types;
using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.Messaging.Core
{
    /// <summary>
    /// Base implementation for a message publisher that can be added to 
    /// the pipeline and called with a message is published. 
    /// </summary>
    public abstract class MessagePublisher : IMessagePublisher
    {
        /// <summary>
        /// Specifies the scope to which publishers send messages to subscribers.
        /// </summary>
        public abstract IntegrationTypes IntegrationType { get; }

        /// <summary>
        /// Override to publish message asynchronously. 
        /// </summary>
        /// <param name="message">The message to be delivered.</param>
        /// <param name="cancellationToken">The optional cancellation token passed to the publisher.</param>
        /// <returns>The task that will be completed when the dispatch has completed.</returns>
        public virtual Task PublishMessageAsync(IMessage message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

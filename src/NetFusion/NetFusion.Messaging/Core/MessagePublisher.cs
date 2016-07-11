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
        /// Override to publish message synchronously.  
        /// </summary>
        /// <param name="message">The message to be delivered.</param>
        public virtual void PublishMessage(IMessage message)
        {

        }

        /// <summary>
        /// Override to publish message asynchronously. 
        /// </summary>
        /// <param name="message">The message to be delivered.</param>
        /// <returns>The task that will be completed when the dispatch has completed.</returns>
        public virtual Task PublishMessageAsync(IMessage message)
        {
            return Task.FromResult(default(object));
        }
    }
}

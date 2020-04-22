using System.Threading.Tasks;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.Enrichers
{
    /// <summary>
    /// Base implementation of a message enricher that can be derived.
    /// </summary>
    public abstract class MessageEnricher : IMessageEnricher
    {
        /// <summary>
        /// Overridden by derived class to enrich the message.
        /// </summary>
        /// <param name="message">Reference to the message to enrich.</param>
        /// <returns>Future Result</returns>
        public virtual Task Enrich(IMessage message)
        {
            return Task.CompletedTask;
        }
    }
}

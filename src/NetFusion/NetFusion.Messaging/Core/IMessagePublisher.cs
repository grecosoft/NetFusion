using NetFusion.Bootstrap.Plugins;
using System.Threading.Tasks;

namespace NetFusion.Messaging.Core
{
    /// <summary>
    /// Called when an message is published to allow plug-ins 
    /// to customize the publishing of messages.
    /// </summary>
    public interface IMessagePublisher : IKnownPluginType
    {
        /// <summary>
        /// Publishes a message synchronously.  
        /// </summary>
        /// <param name="message">The message to be delivered.</param>
        void PublishMessage(IMessage message);

        /// <summary>
        /// Publishes a message asynchronously. 
        /// </summary>
        /// <param name="message">The message to be delivered.</param>
        /// <returns>The task that will be completed when publishing has completed.</returns>
        Task PublishMessageAsync(IMessage message);
    }
}

using System.Threading.Tasks;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Azure.ServiceBus.Subscriber
{
    /// <summary>
    /// Service allowing a correlated reply to a previously received message.
    /// </summary>
    public interface IQueueResponseService
    {
        /// <summary>
        /// Sends a response to the originating request.  An exception is thrown if the message
        /// properties are missing any values required for the original sender to correlate the
        /// response.
        /// </summary>
        /// <param name="command">The command originally received.</param>
        /// <param name="response">The response corresponding to the request.</param>
        /// <returns>Task</returns>
        Task RespondToSenderAsync(ICommand command, object response);
    }
}
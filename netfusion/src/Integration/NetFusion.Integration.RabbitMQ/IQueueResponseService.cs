using EasyNetQ;
using IMessage = NetFusion.Messaging.Types.Contracts.IMessage;

namespace NetFusion.Integration.RabbitMQ;

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
    /// <param name="request">The request originally received.</param>
    /// <param name="response">The response corresponding to the request.</param>
    /// <returns>Task</returns>
    Task RespondToSenderAsync(IMessage request, object response);

    /// <summary>
    /// Sends a response to the originating request.  An exception is thrown if the message
    /// properties are missing any values required for the original sender to correlate the
    /// response.
    /// </summary>
    /// <param name="response">The response corresponding to the request.</param>
    /// <param name="replyToQueue">The encoded bus and queue names indicating the reply-queue.</param>
    /// <param name="messageProps">Properties required for the original sender to correlate the
    /// response back the prior request.</param>
    /// <returns></returns>
    Task RespondToSenderAsync(object response, string replyToQueue, MessageProperties messageProps);
}
    
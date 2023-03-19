namespace NetFusion.Messaging.Filters;

/// <summary>
/// Filter executed after the message has been dispatched to the consumer.
/// </summary>
public interface IPostMessageFilter : IMessageFilter
{
    /// <summary>
    /// Invoked after the message has been dispatched to the consumer.
    /// </summary>
    /// <param name="message">The message being dispatched.</param>
    /// <returns>The task that will be completed when execution is completed.</returns>
    Task OnPostFilterAsync(IMessage message);
}
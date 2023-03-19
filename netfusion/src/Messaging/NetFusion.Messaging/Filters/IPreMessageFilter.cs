namespace NetFusion.Messaging.Filters;

/// <summary>
/// Filter executed before the message is dispatched to the consumer.
/// </summary>
public interface IPreMessageFilter : IMessageFilter
{
    /// <summary>
    /// Invoked before the message is dispatched to the consumer.
    /// </summary>
    /// <param name="message">The message being dispatched.</param>
    /// <returns>The task that will be completed when filter is completed.</returns>
    Task OnPreFilterAsync(IMessage message);
}
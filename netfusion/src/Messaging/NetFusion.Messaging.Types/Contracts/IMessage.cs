namespace NetFusion.Messaging.Types.Contracts;

/// <summary>
/// Interface representing communication between a publisher and consumer.
/// The message can also be attributed with simple key/value pairs.
/// </summary>
public interface IMessage 
{
    /// <summary>
    /// Attributes associated with a message.  These attributes are usually
    /// not used by a business application but by messaging implementations
    /// to pass settings between the publisher and subscriber.
    /// </summary>
    IDictionary<string, string> Attributes { get; set; }

    /// <summary>
    /// Sets an optional context associated with the message.  This can be
    /// used by different message publishers to associate context based on
    /// its messaging implementation.
    /// </summary>
    /// <param name="context">The context associated with message.</param>
    void SetContext(object context);
    
    /// <summary>
    /// Returns a context associated with the message.
    /// </summary>
    /// <typeparam name="TContext">The type of the context to return.</typeparam>
    /// <returns>Reference to the context or an exception if not found.</returns>
    TContext GetContext<TContext>();
}
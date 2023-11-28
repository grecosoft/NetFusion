using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.Types;

/// <summary>
/// Generic message when not classified as a DomainEvent or Command.
/// Often used to model asynchronous responses to commands.
/// </summary>
public class Message : IMessage
{
    private readonly Dictionary<Type, object> _contexts = new();
    
    protected Message()
    {
        Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
        
    /// <summary>
    /// List of arbitrary key value pairs associated with the message. 
    /// </summary>
    public IDictionary<string, string> Attributes { get; set; }

    public void SetContext(object context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        _contexts[context.GetType()] = context;
    }

    public TContext GetContext<TContext>()
    {
        if (!_contexts.TryGetValue(typeof(TContext), out object? context))
        {
            throw new InvalidOperationException(
                $"Message context of type: {typeof(TContext)} not found.");
        }
        
        return (TContext)context;
    }
}
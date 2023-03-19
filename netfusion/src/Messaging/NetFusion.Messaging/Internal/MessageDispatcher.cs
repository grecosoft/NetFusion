using System.Reflection;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Messaging.Routing;

namespace NetFusion.Messaging.Internal;

/// <summary>
/// Contains information used to dispatch a message to a consumer's handler at runtime.
/// This information is gathered by the plugin module during the bootstrap process.
/// </summary>
public class MessageDispatcher
{
    /// <summary>
    /// The type of the message.
    /// </summary>
    /// <returns>Runtime type of the message.</returns>
    public Type MessageType { get; }

    /// <summary>
    /// The component containing a method that can handle the message.
    /// </summary>
    /// <returns>Message consumer runtime type.</returns>
    public Type ConsumerType { get; }

    /// <summary>
    /// Indicates if the message handler should be called for derived message types.
    /// </summary>
    /// <returns>
    /// Returns True if method should be called for derived message types.
    /// </returns>
    public bool IncludeDerivedTypes { get; internal set; }
    
    /// <summary>
    /// Predicate determining if the message applies to the route.
    /// </summary>
    public Predicate<IMessage>? MessagePredicate { get; internal set; }

    /// <summary>
    /// The consumer's message handler method to be called at runtime when the
    /// message is published.
    /// </summary>
    /// <returns>Method handler runtime method information.</returns>
    public MethodInfo MessageHandlerMethod { get; }

    /// <summary>
    /// Indicates that the handler is an asynchronous method.
    /// </summary>
    public bool IsAsync { get; }

    /// <summary>
    /// Indicates that the handler is an asynchronous method returning a value.
    /// </summary>
    public bool IsAsyncWithResult { get; }

    /// <summary>
    /// Indicates if an asynchronous message handler can be canceled.
    /// </summary>
    public bool IsCancellable { get; }
    
    /// <summary>
    /// Additional metadata associated with the route.
    /// </summary>
    public IRouteMeta? RouteMeta { get; }

    private Func<object, object[], object?> Invoker { get; }

    public MessageDispatcher(MessageRoute router)
    {
        ConsumerType = router.ConsumerType ?? 
            throw new NullReferenceException("Dispatcher cannot be created for a route with a null consumer");
        
        MessageHandlerMethod = router.HandlerMethodInfo ??
            throw new NullReferenceException("Dispatcher cannot be created for a route with a null handler-method");
        
        Invoker = router.Invoker ?? 
            throw new NullReferenceException("Dispatcher cannot be created for a route with a null invoker");
        
        MessageType = router.MessageType;
        RouteMeta = router.RouteMeta;
        IsAsync = router.HandlerMethodInfo.IsAsyncMethod();
        IsAsyncWithResult = router.HandlerMethodInfo.IsAsyncMethodWithResult();
        IsCancellable = router.HandlerMethodInfo.IsCancellableMethod();
    }
    
    public override string ToString() => $"{ConsumerType.Name}.{MessageHandlerMethod.Name}({MessageType})";
        
        
    /// <summary>
    /// Dispatches a message to the specified consumer.  The implementation normalizes the
    /// calling for synchronous and asynchronous message handlers.  This allows the method 
    /// handler to be re-factored to one or the other without having to change any of the 
    /// calling code.  This also decouples the publisher from the consumer.  The publisher 
    /// should not be concerned of how the message is handled.
    /// </summary>
    /// <param name="message">The message to be dispatched.</param>
    /// <param name="consumer">Instance of the consumer to have message dispatched.</param>
    /// <param name="cancellationToken">The cancellation token passed to the message handler.</param>
    /// <returns>The response as a future task result.</returns>
    public async Task<object?> Dispatch(IMessage message, object consumer, 
        CancellationToken cancellationToken)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (consumer == null) throw new ArgumentNullException(nameof(consumer));
        
        var taskSource = new TaskCompletionSource<object?>();

        try
        {
            if (IsAsync)
            {
                var invokeParams = new List<object>{ message };
                if (IsCancellable)
                {
                    invokeParams.Add(cancellationToken);
                }

                var asyncResult = (Task?)Invoker(consumer, invokeParams.ToArray());
                if (asyncResult == null)
                {
                    throw new NullReferenceException("Result of Async Message Dispatch can't be null.");
                }
                    
                await asyncResult.ConfigureAwait(false);

                object? result = ProcessResult(message, asyncResult);
                taskSource.SetResult(result);
            }
            else
            {
                object? syncResult = Invoker(consumer, new object[] {message});
                object? result = ProcessResult(message, syncResult);
                taskSource.SetResult(result);
            }
        }
        catch (Exception ex)
        {
            var invokeEx = ex as TargetInvocationException;
            var sourceEx = ex;

            if (invokeEx?.InnerException != null)
            {
                sourceEx = invokeEx.InnerException;
            }

            taskSource.SetException(sourceEx);
        }

        return await taskSource.Task.ConfigureAwait(false);
    }

    private object? ProcessResult(IMessage message, object? result)
    {
        // If we are processing a result for a command, the result
        // needs to be set.  
        if (message is not IMessageWithResult messageResult)
        {
            return null;
        }

        // A Task containing a result is being returned so get the result
        // from the returned task and set it as the command result:
        if (result != null && IsAsyncWithResult)
        {
            dynamic resultTask = result;
            var resultValue = (object)resultTask.Result;

            messageResult.SetResult(resultValue);
            return resultValue;
        }

        // The handler was not asynchronous set the result of the command:
        if (!IsAsync)
        {
            messageResult.SetResult(result);
            return result;
        }

        return null; 
    }
}
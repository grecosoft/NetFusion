using Microsoft.Extensions.Logging;
using NetFusion.Common.Extensions.Reflection;
using NetFusion.Core.Bootstrap.Container;
using NetFusion.Core.Bootstrap.Exceptions;

namespace NetFusion.Messaging.Internal;

public class MessageDispatcherService : IMessageDispatcherService
{
    private readonly ILogger<MessageDispatcherService> _logger;

    public MessageDispatcherService(
        ILogger<MessageDispatcherService> logger)
    {
        _logger = logger;
    }
    
    public async Task<object?> InvokeDispatcherInNewLifetimeScopeAsync(MessageDispatcher dispatcher, IMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(message);

        if (! message.GetType().CanAssignTo(dispatcher.MessageType))
        {
            throw new BootstrapException(
                $"The message type: {message.GetType()} being dispatched does not match or " +
                $"derive from the dispatch information type of: {dispatcher.MessageType}.");
        }

        // Invoke the message consumers in a new lifetime scope.  This is for the case where a message
        // is received outside of the normal lifetime scope such as the one associated with the current
        // web request.

        using var scope = CompositeApp.Instance.CreateServiceScope();
        try
        {
            // Resolve the component and call the message handler.
            var consumer = scope.ServiceProvider.GetService(dispatcher.ConsumerType);
            if (consumer == null)
            {
                throw new InvalidOperationException(
                    $"Message consumer of type: {dispatcher.ConsumerType} not registered.");
            }
            
            return await dispatcher.Dispatch(message, consumer, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Message Dispatch Error Details.");
            throw;
        }
    }
}
using Microsoft.Extensions.Logging;
using NetFusion.Common.Base.Logging;
using NetFusion.Common.Extensions;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Common.Extensions.Tasks;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.Logging;
using NetFusion.Messaging.Plugin;

namespace NetFusion.Messaging.Internal;

/// <summary>
/// This is the default message publisher that dispatches messages locally
/// to message handlers contained within the current microservice process.
/// </summary>
public class InProcessPublisher : IMessagePublisher
{
    private readonly ILogger<InProcessPublisher> _logger;
    private readonly IServiceProvider _services;
    private readonly IMessageDispatchModule _messagingModule;
    private readonly IMessageLogger _messageLogger;

    public InProcessPublisher(
        ILogger<InProcessPublisher> logger,
        IServiceProvider services,
        IMessageDispatchModule messagingModule,
        IMessageLogger messageLogger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _messagingModule = messagingModule ?? throw new ArgumentNullException(nameof(messagingModule));
        _messageLogger = messageLogger ?? throw new ArgumentNullException(nameof(messageLogger));
    }

    // Not used by the implementation, but other plug-ins can use the integration type to apply
    // a subset of the publishers.  i.e. In a unit-of-work, you might want to deliver domain-events
    // in-process before doing so for external integration based events.
    public IntegrationTypes IntegrationType => IntegrationTypes.Internal;

    public async Task PublishMessageAsync(IMessage message, CancellationToken cancellationToken)
    {
        MessageDispatcher[] dispatchers = _messagingModule.GetMessageDispatchers(message).ToArray();
        AssertMessageDispatchers(message, dispatchers);
        
        if (dispatchers.Empty())
        {
            return;
        }
        
        var msgLog = CreateLogMessage(message, dispatchers);

        // Execute all dispatchers and return the task for the caller to await.
        try
        {
            await InvokeMessageDispatchers(dispatchers, message, cancellationToken).ConfigureAwait(false);
        }
        catch (MessageDispatchException ex)
        {
            AddErrorsToLog(msgLog, ex);
            throw;
        }
        finally
        {
            await _messageLogger.LogAsync(msgLog);
        }
    }
    
    private static void AssertMessageDispatchers(IMessage message, MessageDispatcher[] dispatchers)
    {
        if (dispatchers.Length != 1 && message is not IDomainEvent)
        {
            var dispatcherDetails = GetDispatchLogDetails(dispatchers);
                
            throw new PublisherException(
                $"Message of type: {message.GetType()} must have one and only one consumer.", "dispatchers", 
                dispatcherDetails);
        }
    }

    private async Task InvokeMessageDispatchers(MessageDispatcher[] dispatchers, IMessage message,
        CancellationToken cancellationToken)
    {
        LogMessageDispatchers(dispatchers, message);

        TaskListItem<MessageDispatcher>[]? taskList = null;
        try
        {
            // Execute all of matching dispatchers and await the list of associated tasks.
            taskList = dispatchers.Invoke(message, InvokeDispatcher, cancellationToken);
            await taskList.WhenAll();

            LogMessageResult(message);
        }
        catch (Exception ex)
        {
            if (taskList != null)
            {
                var dispatchErrors = taskList.GetExceptions(GetDispatchException);
                if (dispatchErrors.Any())
                {
                    throw new MessageDispatchException(
                        "An exception was received when dispatching a message to one or more handlers.",
                        ex,
                        dispatchErrors);
                }
            }

            throw new MessageDispatchException(
                "An exception was received when dispatching a message.", ex);
        }
    }

    private Task InvokeDispatcher(MessageDispatcher dispatcher, IMessage message, 
        CancellationToken cancellationToken)
    {
        // Since this s root component, use service-locator to obtain reference to message consumer.
        var consumer = _services.GetService(dispatcher.ConsumerType);
        if (consumer == null)
        {
            throw new InvalidOperationException(
                $"Message consumer of type: {dispatcher.ConsumerType} not registered.");
        }
        
        return dispatcher.Dispatch(message, consumer, cancellationToken);
    }
        
    // ----------------------------- [Logging] -----------------------------

    private MessageLog CreateLogMessage(IMessage message, MessageDispatcher[] dispatchers)
    {
        var msgLog = new MessageLog(LogContextType.PublishedMessage, message);
        msgLog.SentHint("publish-in-process");
        AddDispatchersToLog(msgLog, dispatchers);

        return msgLog;
    }
        
    private void AddDispatchersToLog(MessageLog msgLog, MessageDispatcher[] dispatchers)
    {
        if (! _messageLogger.IsLoggingEnabled) return;
            
        msgLog.AddLogDetail("In-Process", "Message dispatchers for message.");
        foreach (MessageDispatcher dispatcher in dispatchers)
        {
            msgLog.AddLogDetail("Handler Class", dispatcher.ConsumerType.FullName!);
            msgLog.AddLogDetail("Handler Method", dispatcher.MessageHandlerMethod.Name);
        }
    }
        
    private void LogMessageDispatchers(MessageDispatcher[] dispatchers, IMessage message)
    {
        _logger.LogDetails(LogLevel.Debug, "Dispatching Message {MessageType}",
            dispatchers.Select(d => new
            {
                MessageType = d.MessageType.FullName,
                ConsumerType = d.ConsumerType.FullName,
                HandlerMethod = d.MessageHandlerMethod.Name,
                IsAsync = d.IsTask,
                d.IncludeDerivedTypes
            }), 
            message.GetType());
    }

    private void LogMessageResult(IMessage message)
    {
        if (message is IMessageWithResult commandResult)
        {
            var log = LogMessage.For(LogLevel.Debug, "Message {MessageType} Result", message.GetType())
                .WithProperties(
                    LogProperty.ForName("Result", commandResult.MessageResult ?? "No-Result")
                );

            _logger.Log(log);
        }
    }

    private static object GetDispatchLogDetails(IEnumerable<MessageDispatcher> dispatchers)
    {
        return dispatchers.Select(d => new {
                MessageType = d.MessageType.FullName,
                Consumer = d.ConsumerType.FullName,
                Method = d.MessageHandlerMethod.Name
            })
            .ToArray();
    }
        

    // ----------------------------- [Exception Handling] -----------------------------
        
    private static MessageDispatchException GetDispatchException(TaskListItem<MessageDispatcher> taskItem) =>
        new("Error Dispatching Message to In-Process Handler.", 
            taskItem.Invoker, 
            taskItem.Task.Exception);
        

    private void AddErrorsToLog(MessageLog msgLog, MessageDispatchException ex)
    {
        if (_messageLogger.IsLoggingEnabled)
        {
            msgLog.AddLogError("In-Process Error", ex.Details.ToIndentedJson());
        }
    }
}
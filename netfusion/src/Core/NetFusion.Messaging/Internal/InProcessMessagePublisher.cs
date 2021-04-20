using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Logging;
using NetFusion.Common.Extensions;
using NetFusion.Common.Extensions.Tasks;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.Logging;
using NetFusion.Messaging.Plugin;
using NetFusion.Messaging.Types.Contracts;

namespace NetFusion.Messaging.Internal
{
    /// <summary>
    /// This is the default message publisher that dispatches messages locally
    /// to message handlers contained within the current application process.
    /// </summary>
    public class InProcessMessagePublisher : MessagePublisher
    {
        private readonly ILogger<InProcessMessagePublisher> _logger;
        private readonly IServiceProvider _services;
        private readonly IMessageDispatchModule _messagingModule;
        private readonly IMessageLogger _messageLogger;

        public InProcessMessagePublisher(
            ILogger<InProcessMessagePublisher> logger,
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
        public override IntegrationTypes IntegrationType => IntegrationTypes.Internal;

        public override async Task PublishMessageAsync(IMessage message, CancellationToken cancellationToken)
        {
            MessageDispatchInfo[] dispatchers = GetMessageDispatchers(message);
            if (! dispatchers.Any())
            {
                return;
            }

            var msgLog = CreateLogMessage(message, dispatchers);

            // Execute all handlers and return the task for the caller to await.
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

        // Determine the dispatchers associated with the message. 
        private MessageDispatchInfo[] GetMessageDispatchers(IMessage message)
        {
            return _messagingModule.InProcessDispatchers
                .WhereHandlerForMessage(message.GetType())
                .Where(dispatchInfo => dispatchInfo.IsMatch(message))
                .ToArray();
        }

        private async Task InvokeMessageDispatchers(MessageDispatchInfo[] dispatchers, IMessage message,
            CancellationToken cancellationToken)
        {
            LogMessageDispatchers(dispatchers, message);

            TaskListItem<MessageDispatchInfo>[] taskList = null;
            try
            {
                AssertMessageDispatchers(message, dispatchers);

                // Execute all of matching dispatchers and await the list of associated tasks.
                taskList = dispatchers.Invoke(message, InvokeDispatcher, cancellationToken);
                await taskList.WhenAll();
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
        
        private static void AssertMessageDispatchers(IMessage message, MessageDispatchInfo[] dispatchers)
        {
            // There are no constraints on the number of handlers for domain-events.
            if (message is not ICommand command)
            {
                return;
            }

            if (dispatchers.Length > 1)
            {
                var dispatcherDetails = GetDispatchLogDetails(dispatchers);

                throw new PublisherException(
                    $"More than one message consumer handler was found for command message type: {command.GetType()}. " +
                     "A command message type can have only one in-process message consumer handler.", 
                     "DispatcherDetails", dispatcherDetails);
            }
        }

        private Task InvokeDispatcher(MessageDispatchInfo dispatcher, IMessage message, 
            CancellationToken cancellationToken)
        {
            // Since this root component, use service-locator obtain reference to message consumer.
            var consumer = (IMessageConsumer)_services.GetRequiredService(dispatcher.ConsumerType);
            return dispatcher.Dispatch(message, consumer, cancellationToken);
        }
        
        // ----------------------------- [Logging] -----------------------------

        private MessageLog CreateLogMessage(IMessage message, MessageDispatchInfo[] dispatchers)
        {
            var msgLog = new MessageLog(message, LogContextType.PublishedMessage);
            msgLog.SentHint("publish-in-process");
            AddDispatchersToLog(msgLog, dispatchers);

            return msgLog;
        }
        
        private void AddDispatchersToLog(MessageLog msgLog, MessageDispatchInfo[] dispatchers)
        {
            if (! _messageLogger.IsLoggingEnabled) return;
            
            msgLog.AddLogDetail("In-Process", "Message dispatchers for message.");
            foreach (MessageDispatchInfo dispatcher in dispatchers)
            {
                msgLog.AddLogDetail("Handler Class", dispatcher.ConsumerType.FullName);
                msgLog.AddLogDetail("Handler Method", dispatcher.MessageHandlerMethod.Name);
            }
        }
        
        private void LogMessageDispatchers(MessageDispatchInfo[] dispatchers, IMessage message)
        {
            _logger.LogDetails(LogLevel.Debug, "Dispatching Message {MessageType}",
                dispatchers.Select(d => new
                {
                    MessageType = d.MessageType.FullName,
                    ConsumerTpe = d.ConsumerType.FullName,
                    HandlerMethod = d.MessageHandlerMethod.Name,
                    d.IsAsync,
                    HasRules = d.DispatchRules.Any(),
                    d.IncludeDerivedTypes
                }), 
                message.GetType());
        }

        private static object GetDispatchLogDetails(IEnumerable<MessageDispatchInfo> dispatchers)
        {
            return dispatchers.Select(d => new {
                    d.MessageType.FullName,
                    Consumer = d.ConsumerType.FullName,
                    Method = d.MessageHandlerMethod.Name
                })
                .ToArray();
        }
        

        // ----------------------------- [Exception Handling] -----------------------------
        
        private static MessageDispatchException GetDispatchException(TaskListItem<MessageDispatchInfo> taskItem)
        {
            return new("Error Dispatching Message to In-Process Handler.", 
                taskItem.Invoker, 
                taskItem.Task.Exception);
        }

        private void AddErrorsToLog(MessageLog msgLog, MessageDispatchException ex)
        {
            if (_messageLogger.IsLoggingEnabled)
            {
                msgLog.AddLogError("In-Process Error", ex.Details.ToIndentedJson());
            }
        }
    }
}
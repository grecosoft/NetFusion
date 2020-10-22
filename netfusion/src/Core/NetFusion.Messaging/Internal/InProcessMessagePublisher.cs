using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Bootstrap.Logging;
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
        private readonly ILogger _logger;
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
        // in-process before doing so for broker based events.
        public override IntegrationTypes IntegrationType => IntegrationTypes.Internal;

        public override async Task PublishMessageAsync(IMessage message, CancellationToken cancellationToken)
        {
            // Determine the dispatchers associated with the message.
            MessageDispatchInfo[] dispatchers = _messagingModule.InProcessDispatchers
                .WhereHandlerForMessage(message.GetType())
                .ToArray();

            if (! dispatchers.Any())
            {
                return;
            }
            
            var msgLog = new MessageLog(message, LogContextType.PublishedMessage);
            msgLog.SentHint("publish-inprocess");
            
            LogMessageDispatchInfo(message, dispatchers);
            AddDispatchersToLog(msgLog, dispatchers);
            
            // Execute all handlers and return the task for the caller to await.
            try
            {
                await InvokeMessageDispatchers(message, dispatchers, cancellationToken).ConfigureAwait(false);
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
        
        private async Task InvokeMessageDispatchers(IMessage message,
            IEnumerable<MessageDispatchInfo> dispatchers,
            CancellationToken cancellationToken)
        {
            TaskListItem<MessageDispatchInfo>[] taskList = null;

            try
            {
                // Filter the list of dispatchers to only those that apply.
                MessageDispatchInfo[] matchingDispatchers = dispatchers
                    .Where(dispatchInfo => dispatchInfo.IsMatch(message))
                    .ToArray();
                
                AssertMessageDispatchers(message, matchingDispatchers);

                // Execute all of matching dispatchers and await the list of associated tasks.
                taskList = matchingDispatchers.Invoke(message, InvokeDispatcher, cancellationToken);
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
            if (! (message is ICommand command))
            {
                return;
            }

            if (dispatchers.Length > 1)
            {
                var dispatcherDetails = GetDispatchLogDetails(dispatchers);

                throw new PublisherException(
                    $"More than one message consumer handler was found for command message type: {command.GetType()}.  " +
                     "A command message type can have only one in-process message consumer handler.", 
                     "DispatcherDetails", dispatcherDetails);
            }
        }

        private Task InvokeDispatcher(MessageDispatchInfo dispatcher, IMessage message, 
            CancellationToken cancellationToken)
        {
            if (!_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogDebug(
                    $"Dispatching message Type: {message.GetType()} to Consumer: { dispatcher.ConsumerType } " +
                    $"Method: { dispatcher.MessageHandlerMethod.Name} ");
            }
            
            // Since this root component, use service-locator obtain reference to message consumer.
            var consumer = (IMessageConsumer)_services.GetRequiredService(dispatcher.ConsumerType);
            return dispatcher.Dispatch(message, consumer, cancellationToken);
        }

        private static MessageDispatchException GetDispatchException(TaskListItem<MessageDispatchInfo> taskItem)
        {
            var sourceEx = taskItem.Task.Exception?.InnerException;

            if (sourceEx is MessageDispatchException dispatchEx)
            {
                return dispatchEx;
            }

            return new MessageDispatchException("Error calling message consumer.", 
                taskItem.Invoker, sourceEx);
            
        }

        private void LogMessageDispatchInfo(IMessage message, IEnumerable<MessageDispatchInfo> dispatchers)
        {
            if (! _logger.IsEnabled(LogLevel.Trace)) return;
            
            var dispatcherDetails = GetDispatchLogDetails(dispatchers);

            _logger.LogTraceDetails($"Message Published: {message.GetType()}",
                new
                {
                    Dispatchers = dispatcherDetails,
                    Message = message
                });
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

        private void AddErrorsToLog(MessageLog msgLog, MessageDispatchException ex)
        {
            if (_messageLogger.IsLoggingEnabled)
            {
                msgLog.AddLogError("In-Process Error", ex.Details.ToIndentedJson());
            }
        }
    }
}

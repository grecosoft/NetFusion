using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Scripting;
using NetFusion.Bootstrap.Logging;
using NetFusion.Common.Extensions.Tasks;
using NetFusion.Messaging.Exceptions;
using NetFusion.Messaging.Plugin;
using NetFusion.Messaging.Types;

namespace NetFusion.Messaging.Core
{
    /// <summary>
    /// This is the default message publisher that dispatches messages locally
    /// to message handlers contained within the current application process.
    /// </summary>
    public class InProcessMessagePublisher : MessagePublisher
    {
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;
        private readonly IMessageDispatchModule _messagingModule;
        private readonly IEntityScriptingService _scriptingSrv;

        public InProcessMessagePublisher(
            IServiceProvider services,
            ILogger<InProcessMessagePublisher> logger,
            IMessageDispatchModule eventingModule,
            IEntityScriptingService scriptingSrv)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _messagingModule = eventingModule ?? throw new ArgumentNullException(nameof(eventingModule));
            _scriptingSrv = scriptingSrv ?? throw new ArgumentNullException(nameof(scriptingSrv));
        }

        // Not used by the implementation, but other plug-ins can use the integration type to apply
        // a subset of the publishers.  i.e. In a unit-of-work, you might want to deliver domain-events
        // in-process before doing so for broker based events.
        public override IntegrationTypes IntegrationType => IntegrationTypes.Internal;

        public override async Task PublishMessageAsync(IMessage message, CancellationToken cancellationToken)
        {
            // Determine the dispatchers associated with the message.
            IEnumerable<MessageDispatchInfo> dispatchers = _messagingModule.InProcessDispatchers
                .WhereHandlerForMessage(message.GetType())
                .ToArray();
            
            if (! dispatchers.Any())
            {
                return;
            }

            LogMessageDispatchInfo(message, dispatchers);

            // Execute all handlers and return the task for the caller to await.
            await InvokeMessageDispatchersAsync(message, dispatchers, cancellationToken).ConfigureAwait(false);
        }

        private async Task InvokeMessageDispatchersAsync(IMessage message,
            IEnumerable<MessageDispatchInfo> dispatchers,
            CancellationToken cancellationToken)
        {
            TaskListItem<MessageDispatchInfo>[] taskList = null;

            try
            {
                MessageDispatchInfo[] matchingDispatchers = await GetMatchingDispatchers(dispatchers, message);
                AssertMessageDispatchers(message, matchingDispatchers);

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

        private async Task<MessageDispatchInfo[]> GetMatchingDispatchers(IEnumerable<MessageDispatchInfo> dispatchers, IMessage message)
        {
            List<MessageDispatchInfo> matchingDispatchers = new List<MessageDispatchInfo>();

            foreach (MessageDispatchInfo dispatchInfo in dispatchers)
            {
                if (await PassesDispatchCriteria(dispatchInfo, message))
                {
                    matchingDispatchers.Add(dispatchInfo);
                }
            }
            return matchingDispatchers.ToArray();
        }

        // Determines message dispatchers that apply to the message being published.  This is optional and
        // specified by decorating the message handler method with attributes.
        private async Task<bool> PassesDispatchCriteria(MessageDispatchInfo dispatchInfo, IMessage message)
        {
            ScriptPredicate predicate = dispatchInfo.Predicate;

            // Run a dynamic script against the message and check the result of the specified predicate property.
            // This allow storing rule predicates external from the code.
            if (predicate != null)
            {
                return await _scriptingSrv.SatisfiesPredicateAsync(message, predicate);
            }

            // Check static rules to determine if message meets criteria.
            return dispatchInfo.IsMatch(message);
        }

        private static void AssertMessageDispatchers(IMessage message, MessageDispatchInfo[] dispatchers)
        {
            // There are no constrains on the number of handlers for domain-events.
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

        private Task InvokeDispatcher(MessageDispatchInfo dispatcher, IMessage message, CancellationToken cancellationToken)
        {
            if (!_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogDebug(
                    $"Dispatching message Type: {message.GetType()} to Consumer: { dispatcher.ConsumerType } " +
                    $"Method: { dispatcher.MessageHandlerMethod.Name} ");
            }
            
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

            _logger.LogTraceDetails(MessagingLogEvents.MessagingDispatch, $"Message Published: {message.GetType()}",
                new
                {
                    Message = message,
                    Dispatchers = dispatcherDetails
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
    }
}

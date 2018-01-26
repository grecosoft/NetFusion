using Autofac;
using Microsoft.Extensions.Logging;
using NetFusion.Base.Scripting;
using NetFusion.Bootstrap.Logging;
using NetFusion.Common.Extensions.Tasks;
using NetFusion.Messaging.Modules;
using NetFusion.Messaging.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.Messaging.Core
{
    /// <summary>
    /// This is the default message publisher that dispatches messages locally
    /// to message handlers contained within the current application process.
    /// </summary>
    public class InProcessMessagePublisher : MessagePublisher
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly ILogger _logger;
        private readonly IMessageDispatchModule _messagingModule;
        private readonly IEntityScriptingService _scriptingSrv;

        public InProcessMessagePublisher(
            ILifetimeScope liftimeScope,
            ILogger<InProcessMessagePublisher> logger,
            IMessageDispatchModule eventingModule,
            IEntityScriptingService scriptingSrv)
        {
            _lifetimeScope = liftimeScope;
            _logger = logger;
            _messagingModule = eventingModule;
            _scriptingSrv = scriptingSrv;
        }

        public override IntegrationTypes IntegrationType => IntegrationTypes.Internal;

        public override Task PublishMessageAsync(IMessage message, CancellationToken cancellationToken)
        {
            // Determine the dispatchers associated with the message.
            IEnumerable<MessageDispatchInfo> dispatchers = _messagingModule.InProcessDispatchers
                .WhereHandlerForMessage(message.GetType())
                .ToList();

            LogMessageDespatchInfo(message, dispatchers);

            // Execute all handlers and return the task for the caller to await.
            return InvokeMessageDispatchersAsync(message, dispatchers, cancellationToken);
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

        private async Task<bool> PassesDispatchCriteria(MessageDispatchInfo dispatchInfo, IMessage message)
        {
            ScriptPredicate predicate = dispatchInfo.Predicate;

            if (predicate != null)
            {
                return await _scriptingSrv.SatisfiesPredicateAsync(message, predicate);
            }

            return dispatchInfo.IsMatch(message);
        }

        private void AssertMessageDispatchers(IMessage message, IEnumerable<MessageDispatchInfo> dispatchers)
        {
            var command = message as ICommand;
            if (command == null)
            {
                return;
            }

            if (dispatchers.Count() > 1)
            {
                var dispatcherDetails = GetDispatchLogDetails(dispatchers);

                throw new PublisherException(
                    $"More than one message consumer handler was found for command message type: {command.GetType()}." +
                    $"A command message type can have only one in-process message consumer handler.", dispatcherDetails);
            }

            if (!dispatchers.Any())
            {
                throw new PublisherException(
                    $"No message consumer handler was found for the command message of type: {command.GetType()}." +
                    $"Make sure there is a class implementing: {typeof(IMessageConsumer)} with a method decorated with " +
                    $"the attribute of type: {typeof(InProcessHandlerAttribute)} for the corresponding command type.");
            }
        }

        private Task InvokeDispatcher(MessageDispatchInfo dispatcher, IMessage message, CancellationToken cancellationToken)
        {
            var consumer = (IMessageConsumer)_lifetimeScope.Resolve(dispatcher.ConsumerType);
            return dispatcher.Dispatch(message, consumer, cancellationToken);
        }

        private MessageDispatchException GetDispatchException(TaskListItem<MessageDispatchInfo> taskItem)
        {
            var sourceEx = taskItem.Task.Exception.InnerException;

            if (sourceEx is MessageDispatchException dispatchEx)
            {
                return dispatchEx;
            }

            return new MessageDispatchException("Error calling message consumer.", 
                taskItem.Invoker, sourceEx);
            
        }

        private void LogMessageDespatchInfo(IMessage message, IEnumerable<MessageDispatchInfo> dispatchers)
        {
            if (!_logger.IsEnabled(LogLevel.Debug))
            {
                return;
            }

            var dispatcherDetails = GetDispatchLogDetails(dispatchers);

            _logger.LogTraceDetails(MessagingLogEvents.MESSAGING_DISPATCH, $"Message Published: {message.GetType()}",
                new
                {
                    Message = message,
                    Dispatchers = dispatcherDetails
                });
        }

        private object GetDispatchLogDetails(IEnumerable<MessageDispatchInfo> dispatchers)
        {
            return dispatchers.Select(d => new {
                d.MessageType.FullName,
                Consumer = d.ConsumerType.FullName,
                Method = d.MessageHandlerMethod.Name
            })
            .ToList();
        }
    }
}

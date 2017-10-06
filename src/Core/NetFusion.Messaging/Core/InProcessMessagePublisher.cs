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
        private readonly IMessagingModule _messagingModule;
        private readonly IEntityScriptingService _scriptingSrv;

        public InProcessMessagePublisher(
            ILifetimeScope liftimeScope,
            ILogger<InProcessMessagePublisher> logger,
            IMessagingModule eventingModule,
            IEntityScriptingService scriptingSrv)
        {
            _lifetimeScope = liftimeScope;
            _logger = logger;
            _messagingModule = eventingModule;
            _scriptingSrv = scriptingSrv;
        }

        public override Task PublishMessageAsync(IMessage message, CancellationToken cancellationToken)
        {
            // Determine the dispatchers associated with the message.
            IEnumerable<MessageDispatchInfo> dispatchers = _messagingModule.InProcessDispatchers
                .WhereHandlerForMessage(message.GetType())
                .ToList();

            LogMessageDespatchInfo(message, dispatchers);

            // Execute all handlers and return the future result to the caller to await.
            return InvokeMessageDispatchersAsync(message, dispatchers, cancellationToken);
        }

        private async Task InvokeMessageDispatchersAsync(IMessage message,
            IEnumerable<MessageDispatchInfo> dispatchers,
            CancellationToken cancellationToken)
        {
            FutureResult<MessageDispatchInfo>[] futureResults = null;

            try
            {
                MessageDispatchInfo[] matchingDispatchers = await GetMatchingDispatchers(dispatchers, message);
                AssertMessageDispatchers(message, matchingDispatchers);

                futureResults = matchingDispatchers.Invoke(message, InvokeDispatcher, cancellationToken);
                await futureResults.WhenAll();
            }
            catch (Exception ex)
            {
                if (futureResults != null)
                {
                    var dispatchErrors = futureResults.GetExceptions(GetDispatchException);
                    if (dispatchErrors.Any())
                    {
                        throw new MessageDispatchException(
                            "An exception was received when dispatching a message to one or more handlers.",
                            dispatchErrors);
                    }

                    throw new MessageDispatchException(
                        "An exception was received when dispatching a message.", ex);
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
                return await _scriptingSrv.SatisfiesPredicate(message, predicate);
            }

            return dispatchInfo.IsMatch(message);
        }

        private void AssertMessageDispatchers(IMessage message, IEnumerable<MessageDispatchInfo> dispatchers)
        {
            if (message is ICommand commandMessage && dispatchers.Count() > 1)
            {
                throw new InvalidOperationException(
                    $"More than one message consumer handler was found for message type: {commandMessage.GetType()}" +
                    $"A command message type can have only one in-process message consumer handler.");
            }
        }

        private Task InvokeDispatcher(MessageDispatchInfo dispatcher, IMessage message, CancellationToken cancellationToken)
        {
            var consumer = (IMessageConsumer)_lifetimeScope.Resolve(dispatcher.ConsumerType);
            return dispatcher.Dispatch(message, consumer, cancellationToken);
        }

        private MessageDispatchException GetDispatchException(FutureResult<MessageDispatchInfo> futureResult)
        {
            var sourceEx = futureResult.Task.Exception.InnerException;

            if (sourceEx is MessageDispatchException dispatchEx)
            {
                return dispatchEx;
            }

            return new MessageDispatchException("Error calling message consumer.", 
                futureResult.Invoker, sourceEx);
            
        }

        private void LogMessageDespatchInfo(IMessage message, IEnumerable<MessageDispatchInfo> dispatchers)
        {
            if (!_logger.IsEnabled(LogLevel.Trace))
            {
                return;
            }

            var dispatcherDetails = dispatchers.Select(d => new {
                d.MessageType,
                Consumer = d.ConsumerType.Name,
                Method = d.MessageHandlerMethod.Name    
            })
            .ToList();

            _logger.LogTraceDetails(MessagingLogEvents.MESSAGING_DISPATCH, $"Message Published: {message.GetType()}",
                new
                {
                    Message = message,
                    Dispatchers = dispatcherDetails
                });
        }
    }
}

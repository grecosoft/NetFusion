﻿using Autofac;
using NetFusion.Bootstrap.Logging;
using NetFusion.Domain.Scripting;
using NetFusion.Messaging.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NetFusion.Messaging.Core
{
    /// <summary>
    /// This is the default message publisher that dispatches messages locally
    /// to message handlers.  When following DDD concepts, messages are typically
    /// published in-process and handled synchronously and/or asynchronously.  
    /// 
    /// However, this can be extended by other plug-ins publishers such as the 
    /// RabbitMQ plug-in.
    /// </summary>
    public class InProcessMessagePublisher : MessagePublisher
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly IContainerLogger _logger;
        private readonly IMessagingModule _messagingModule;
        private readonly IEntityScriptingService _scriptingSrv;

        public InProcessMessagePublisher(
            ILifetimeScope liftimeScope,
            IContainerLogger logger,
            IMessagingModule eventingModule,
            IEntityScriptingService scriptingSrv)
        {
            _lifetimeScope = liftimeScope;
            _logger = logger.ForPluginContext<InProcessMessagePublisher>();
            _messagingModule = eventingModule;
            _scriptingSrv = scriptingSrv;
        }

        public async override Task PublishMessageAsync(IMessage message)
        {
            // Determine the dispatchers associated with the message.
            var dispatchers = _messagingModule.InProcessDispatchers
                .WhereHandlerForMessage(message.GetType())
                .ToList();

            LogMessageDespatchInfo(message, dispatchers);

            // Execute all handlers and return the future result to the caller to await.
            await InvokeMessageDispatchersAsync(message, dispatchers);
        }

        private async Task InvokeMessageDispatchersAsync(IMessage message,
            IEnumerable<MessageDispatchInfo> dispatchers)
        {
            IEnumerable<DispatchTask> futureResults = null;
            List<MessageDispatchInfo> matchingDispatchers = new List<MessageDispatchInfo>();

            try
            {
                foreach(MessageDispatchInfo dispatchInfo in dispatchers)
                {
                    if (await MatchesDispatchCriteria(dispatchInfo, message))
                    {
                        matchingDispatchers.Add(dispatchInfo);
                    }
                } 

                futureResults = InvokeMessageDispatchers(message, matchingDispatchers);
                await Task.WhenAll(futureResults.Select(fr => fr.Task));
            }
            catch (Exception ex)
            {
                if (futureResults != null)
                {
                    var dispatchErrors = GetDispatcherErrors(futureResults);
                    if (dispatchErrors.Any())
                    {
                        throw new MessageDispatchException(
                            "An exception was received when dispatching a message to " +
                            "one or more asynchronous handlers.",
                            message,
                            dispatchErrors);
                    }
                }

                throw new MessageDispatchException(
                    "An exception was received when dispatching a message.", 
                    message, ex);
            }
        }

        private async Task<bool> MatchesDispatchCriteria(MessageDispatchInfo dispatchInfo, IMessage message)
        {
            ScriptPredicate predicate = dispatchInfo.Predicate;

            if (predicate != null)
            {
                return await _scriptingSrv.SatifiesPredicate(message, predicate);
            }

            return dispatchInfo.IsMatch(message);
        }

        private IEnumerable<DispatchTask> InvokeMessageDispatchers(IMessage message,
           IEnumerable<MessageDispatchInfo> dispatchers)
        {
            var futureResults = new List<DispatchTask>();

            foreach (MessageDispatchInfo dispatcher in dispatchers)
            {
                var consumer = (IMessageConsumer)_lifetimeScope.Resolve(dispatcher.ConsumerType);
                var futureResult = dispatcher.Dispatch(message, consumer);

                futureResults.Add(new DispatchTask(futureResult, dispatcher));
            }
            return futureResults;
        }

        private IEnumerable<MessageDispatchException> GetDispatcherErrors(
            IEnumerable<DispatchTask> dispatchedTasks)
        {
            var dispatchErrors = new List<MessageDispatchException>();
            var dispatchTaskErrors = dispatchedTasks.Where(dt => dt.Task.Exception != null);

            foreach (DispatchTask dispatchedTask in dispatchTaskErrors)
            {
                var sourceEx = dispatchedTask.Task.Exception.InnerException;
                var invokeEx = sourceEx as TargetInvocationException;

                if (invokeEx != null)
                {
                    dispatchErrors.Add(new MessageDispatchException(
                        "Error calling message consumer.", dispatchedTask.Dispatch, invokeEx.InnerException));
                }
                else
                {
                    dispatchErrors.Add(new MessageDispatchException(
                        "Error calling message consumer", dispatchedTask.Dispatch, sourceEx));
                }
            }

            return dispatchErrors;
        }

        private void LogMessageDespatchInfo(IMessage message, IList<MessageDispatchInfo> dispatchers)
        {
            var dispatcherDetails = dispatchers.Select(d => new {
                d.MessageType,
                Consumer = d.ConsumerType.Name,
                Method = d.MessageHandlerMethod.Name    
            })
            .ToList();

            _logger.Debug($"Message Published: {message.GetType()}",
                new
                {
                    Message = message,
                    Dispatchers = dispatcherDetails
                });
        }
    }
}

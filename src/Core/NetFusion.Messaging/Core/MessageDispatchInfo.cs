using NetFusion.Common;
using NetFusion.Domain.Messaging;
using NetFusion.Domain.Messaging.Rules;
using NetFusion.Domain.Scripting;
using NetFusion.Messaging.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace NetFusion.Messaging.Core
{
    /// <summary>
    /// Contains information used to invoke message handlers for a given message type at runtime.
    /// This information is gathered by the plug-in module during the bootstrap process.  Other
    /// plug-ins requiring the publishing of messages (i.e. NetFusion.RabbitMQ) can also access 
    /// this information.  Other plug-ins can use metadata attributes specific to their plug-in 
    /// to further filter the consumer handlers that should be invoked.
    /// </summary>
    public class MessageDispatchInfo
    {
        /// <summary>
        /// The type of the message.
        /// </summary>
        /// <returns>Runtime type of the message.</returns>
        public Type MessageType { get; set; }

        /// <summary>
        /// The component containing the method that can handle the message. All types
        /// implementing the IMessageConsumer are scanned for message hander methods.
        /// </summary>
        /// <returns>Message consumer runtime type.</returns>
        public Type ConsumerType { get; set; }

        /// <summary>
        /// Indicates if the message handler should be called for derived message types.
        /// This is determined by checking the message handler method's message parameter
        /// for the IncludeDerivedMessages attribute.
        /// </summary>
        /// <returns>
        /// Returns True if method should be called for derived message types.
        /// By default, this is false. 
        /// </returns>
        public bool IncludeDerivedTypes { get; set; }

        /// <summary>
        /// The consumer's message handler method to be called at runtime when the
        /// message is published.
        /// </summary>
        /// <returns>Method handler runtime method information.</returns>
        public MethodInfo MessageHandlerMethod { get; set; }

        /// <summary>
        /// Indicates that the handler is an asynchronous method.
        /// </summary>
        public bool IsAsync { get; set; }

        /// <summary>
        /// Indicates that the handler is an asynchronous method returning a value.
        /// </summary>
        public bool IsAsyncWithResult { get; set; }

        /// <summary>
        /// Indicates if an asynchronous message handler can be canceled.
        /// </summary>
        public bool IsCancellable { get; set; }

        /// <summary>
        /// Indicates that the handler is marked with the InProcessHandler attribute.
        /// This indicates that the handler will be used by the InProcessMessagePublisher
        /// and will not be published to another process using a messaging technology.
        /// </summary>
        public bool IsInProcessHandler { get; set; }

        /// <summary>
        /// Delegate used to invoke the message handler.  This is created from the
        /// reflected information.
        /// </summary>
        public MulticastDelegate Invoker { get; set; }

        /// <summary>
        /// The type of the dispatch rules associated with the message handler.
        /// These are simple predicates based on the properties of the message
        /// that determines if the message handler should be invoked.
        /// </summary>
        public Type[] DispatchRuleTypes { get; set; }

        /// <summary>
        /// Rule instances associated with the message handler.  The message
        /// handler will only be called if the message meets the rule criteria.
        /// </summary>
        public IMessageDispatchRule[] DispatchRules { get; set; }

        /// <summary>
        /// Determines if all or any of the rules must evaluate to true for the
        /// message handler to be called.
        /// </summary>
        public RuleApplyTypes RuleApplyType { get; set; }

        /// <summary>
        /// Identifies an externally stored script that is executed against the
        /// associated message handler to determine if the message matches the
        /// criteria.  If the message has matching criteria, the handler is called.
        /// </summary>
        public ScriptPredicate Predicate { get; set; }

        /// <summary>
        /// Determines if the message handler applies based on the assigned dispatcher
        /// rules and the dispatcher rule type.  
        /// </summary>
        /// <param name="message">The message</param>
        /// <returns>Returns True if the event handler should be called.</returns>
        public bool IsMatch(IMessage message)
        {
            Check.NotNull(message, nameof(message));

            if (!DispatchRuleTypes.Any()) return true;

            if (RuleApplyType == RuleApplyTypes.All)
            {
                return DispatchRules.All(r => r.IsMatch(message));
            }

            return DispatchRules.Any(r => r.IsMatch(message));
        }

        /// <summary>
        /// Dispatches a message to the specified consumer.  The implementation
        /// normalizes the calling for synchronous and asynchronous message handlers.
        /// This allows the method handler to be re-factored to one or the other 
        /// without having to change any of the calling code.  This also decouples
        /// the publisher from the consumer.  The publisher should not be concerned 
        /// or determine how the message is handled.
        /// </summary>
        /// <param name="message">The message to be dispatched.</param>
        /// <param name="consumer">Instance of the consumer to have message dispatched.</param>
        /// <param name="cancellationToken">The optional cancellation token passed to the message handler.</param>
        /// <returns>The response as a future result.</returns>
        public async Task<object> Dispatch(IMessage message, IMessageConsumer consumer, CancellationToken cancellationToken)
        {
            Check.NotNull(message, nameof(message));
            Check.NotNull(consumer, nameof(consumer));

            var futureResult = new TaskCompletionSource<object>();

            try
            {
                if (IsAsync)
                {
                    Task asyncResult = null;

                    var invokeParams = new List<object>{ consumer, message };
                    if (IsCancellable)
                    {
                        invokeParams.Add(cancellationToken);
                    }

                    asyncResult = (Task)Invoker.DynamicInvoke(invokeParams.ToArray());
                    await asyncResult;

                    object result = ProcessResult(message, asyncResult);
                    futureResult.SetResult(result);
                }
                else
                {
                    object syncResult = Invoker.DynamicInvoke(consumer, message);
                    object result = ProcessResult(message, syncResult);
                    futureResult.SetResult(result);
                }
            }
            catch (Exception ex)
            {
                var invokeEx = ex as TargetInvocationException;
                var sourceEx = ex;

                if (invokeEx != null)
                {
                    sourceEx = invokeEx.InnerException;
                }

                var dispatchEx = new MessageDispatchException("Exception Dispatching Message.", 
                    this, 
                    sourceEx);      
                          
                futureResult.SetException(dispatchEx);
            }

            return await futureResult.Task;
        }

        private object ProcessResult(IMessage message, object result)
        {
            object resultValue = result;
            var command = message as ICommand;

            if (command != null && result != null && IsAsyncWithResult)
            {
                dynamic resultTask = result;
                resultValue = resultTask.Result;
            }

            if (command != null && resultValue != null)
            {
                command.SetResult(resultValue);
            }

            return resultValue; 
        }
    }
}

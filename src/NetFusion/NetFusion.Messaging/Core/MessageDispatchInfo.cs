using NetFusion.Messaging.Rules;
using System;
using System.Linq;
using System.Reflection;

namespace NetFusion.Messaging.Core
{
    /// <summary>
    /// Contains information used to invoke message handlers for a given message at runtime.
    /// This information is gathered by the plug-in module during the bootstrap process.
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
        /// This is determined if a message handler method's message parameter is 
        /// decorated with the IncludeDerivedMessages attribute.
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
        /// Indicates that the handler is an asynchronous method.  This is used
        /// when dispatching message handlers when an message is published.
        /// </summary>
        public bool IsAsync { get; set; }

        /// <summary>
        /// Indicates that the handler is marked with the InProcessHandler attribute.
        /// This indicates that the handler will be used by the InProcessMessagePublisher
        /// and will not be published to another process using a messaging technology.
        /// </summary>
        public bool IsInProcessHandler { get; set; }

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
        /// Delegate used to invoke the message handler.  This is created from the
        /// reflected information and provides near statically compiled execution
        /// performance.
        /// </summary>
        public MulticastDelegate Invoker { get; set; }

        /// <summary>
        /// Determines if the message handler applies based on the assigned dispatcher
        /// rules and the dispatcher rule type.  
        /// </summary>
        /// <param name="message">The message</param>
        /// <returns>Returns True if the event handler should be called.</returns>
        public bool IsMatch(IMessage message)
        {
            if (!this.DispatchRuleTypes.Any()) return true;

            if (this.RuleApplyType == RuleApplyTypes.All)
            {
                return this.DispatchRules.All(r => r.IsMatch(message));
            }

            return this.DispatchRules.Any(r => r.IsMatch(message));
        }
    }
}

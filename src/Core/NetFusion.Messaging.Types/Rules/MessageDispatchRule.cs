using System;

namespace NetFusion.Messaging.Types.Rules
{
    /// <summary>
    /// Base class for specifying a rule to determine if a consumer's handler should be 
    /// invoked for a given message.
    /// </summary> 
    /// <typeparam name="TMessage">The type or base type of the
    /// message associated with the rule.</typeparam>
    public abstract class MessageDispatchRule<TMessage> : IMessageDispatchRule
        where TMessage : IMessage
    {
        Type IMessageDispatchRule.MessageType => typeof(TMessage);

        bool IMessageDispatchRule.IsMatch(IMessage message)
        {
            return IsMatch((TMessage)message);
        }

        /// <summary>
        /// Implemented by a derived dispatch rule to determine if the message should be handled.
        /// </summary>
        /// <param name="message">The published message.</param>
        /// <returns>True if the hander should be invoked.  Otherwise, False.</returns>
        protected abstract bool IsMatch(TMessage message);
    }
}

using System;

namespace NetFusion.Messaging.Rules
{
    /// <summary>
    /// Base class for specifying a rule to determine if a consumer's
    /// handler should be invoked for a given message.
    /// </summary>
    /// <typeparam name="TEvent">The type or base type of the
    /// message associated with the rule.</typeparam>
    public abstract class MessageDispatchRule<TEvent> : IMessageDispatchRule
        where TEvent : IMessage
    {
        Type IMessageDispatchRule.EventType => typeof(TEvent);

        bool IMessageDispatchRule.IsMatch(IMessage message)
        {
            return IsMatch((TEvent)message);
        }

        /// <summary>
        /// Implemented by a derived dispatch rule to determine if the
        /// message should be handled.
        /// </summary>
        /// <param name="message">The published message.</param>
        /// <returns>True if the hander should be invoked.  Otherwise, False.</returns>
        protected abstract bool IsMatch(TEvent message);
    }
}

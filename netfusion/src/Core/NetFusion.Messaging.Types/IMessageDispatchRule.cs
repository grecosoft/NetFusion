using NetFusion.Base.Plugins;
using System;

namespace NetFusion.Messaging.Types
{
    /// <summary>
    /// Implemented by a class that determines if a given consumer's message 
    /// handler should be invoked for a published message.
    /// </summary>
    public interface IMessageDispatchRule : IKnownPluginType
    {
        /// <summary>
        /// The type of the message associated with the rule.
        /// </summary>
        Type MessageType { get; }

        /// <summary>
        /// Determine if the message meets the criteria needed to call the event handler.
        /// </summary>
        /// <param name="message">The message to test.</param>
        /// <returns>True if the handler should be called.  Otherwise, false.</returns>
        bool IsMatch(IMessage message);  
    }
}

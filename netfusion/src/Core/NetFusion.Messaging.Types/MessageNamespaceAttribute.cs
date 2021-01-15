using System;

namespace NetFusion.Messaging.Types
{
    /// <summary>
    /// Attribute used to associate an external namespace with the messaged.
    /// The namespace value can be used by a message receiver to determine
    /// how the message should be processed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class MessageNamespaceAttribute : Attribute
    {
        public string MessageNamespace { get; }

        /// <summary>
        /// Associates an external namespace with a message.
        /// </summary>
        /// <param name="messageNamespace">External namespace value communicated to message receivers.</param>
        public MessageNamespaceAttribute(string messageNamespace)
        {
            if (string.IsNullOrWhiteSpace(messageNamespace))
                throw new ArgumentException("Message Namespace not specified.", nameof(messageNamespace));
            
            MessageNamespace = messageNamespace;
        }
    }
}
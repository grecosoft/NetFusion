using System;

namespace NetFusion.Azure.Messaging.Publisher.Internal
{
    /// <summary>
    /// Interface represeting an item (i.e. Queue/Topic) that can be created within \
    /// an Azure namespace.
    /// </summary>
    public interface INamespaceItem
    {
        /// <summary>
        /// The namespace one which the object exists.
        /// </summary>
        string Namespace { get; }
        
        /// <summary>
        /// The name of the object assocated with the namespace (i.e. Queue/Topic).
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// The Message type associated with the namespace named item.  When a message
        /// is published of this type, it is sent to the named resource defined on the
        /// namespace.
        /// </summary>
        Type MessageType { get; }
        
        /// <summary>
        /// The serialized content type of the message.
        /// </summary>
        string ContentType { get; }
        
        /// <summary>
        /// The encoding of the serialized message.
        /// </summary>
        string ContentEncoding { get; }
    }
}
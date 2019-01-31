namespace NetFusion.AMQP.Publisher.Internal
{
    using System;

    /// <summary>
    /// Interface representing an item (i.e. Queue/Topic) that can be defined
    /// on the host.
    /// </summary>
    public interface IHostItem
    {
        /// <summary>
        /// The host name on which the object exists.
        /// </summary>
        string HostName { get; }
        
        /// <summary>
        /// The name of the item defined on the host (i.e. Queue/Topic).
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// The Message type associated with the host named item.  When a message
        /// is published of this type, it is sent to the named resource defined on
        /// host.
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
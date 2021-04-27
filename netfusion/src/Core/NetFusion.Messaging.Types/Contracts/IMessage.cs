using System.Collections.Generic;

namespace NetFusion.Messaging.Types.Contracts
{
    /// <summary>
    /// Interface representing communication between a publisher and consumer.
    /// The message can also be attributed with simple key/value pairs.
    /// </summary>
    public interface IMessage 
    {
        /// <summary>
        /// Attributes associated with a message.  These attributes are usually
        /// not used by a business application but by messaging implementations
        /// to pass settings between the publisher and subscriber.
        /// </summary>
        IDictionary<string, string> Attributes { get; set; }
    }
}

using System.Collections.Generic;
using NetFusion.Azure.Messaging.Publisher.Internal;
using NetFusion.Base.Plugins;

namespace NetFusion.Azure.Messaging.Publisher
{
    /// <summary>
    /// Discovered when the PublisherModule is initialized.  A namespace registery is responsible
    /// for providing the metadata about the namespace items (i.e. Queue/Topic) that are defined
    /// on that namespace.  This metadata is cached and used to create the AMQP objects when
    /// sending and receiving messages. 
    /// </summary>
    public interface INamespaceRegistry : IKnownPluginType
    {
        /// <summary>
        /// Called during the bootstrap process and returns the metadata for the objects
        /// defined on the namespace.
        /// </summary>
        /// <returns>List of namespace item metadata.</returns>
        IEnumerable<INamespaceItem> GetItems();
    }
}
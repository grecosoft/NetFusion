using System.Collections.Generic;
using NetFusion.AMQP.Publisher.Internal;
using NetFusion.Base.Plugins;

namespace NetFusion.AMQP.Publisher
{
    /// <summary>
    /// Discovered when the PublisherModule is initialized.  A host registry is responsible
    /// for providing the metadata about defined host items (i.e. Queue/Topic).  This metadata
    /// is cached and used to create the AMQP objects when sending and receiving messages. 
    /// </summary>
    public interface IHostRegistry : IKnownPluginType
    {
        /// <summary>
        /// Called during the bootstrap process and returns the metadata for the
        /// objects defined on the host.
        /// </summary>
        /// <returns>List of host item metadata.</returns>
        IEnumerable<IHostItem> GetItems();
    }
}
using System.Collections.Generic;
using NetFusion.Base.Plugins;
using NetFusion.RabbitMQ.Metadata;

namespace NetFusion.RabbitMQ.Publisher
{
    /// <summary>
    /// Interface implemented by application used to define
    /// the exchanges/queues to which messages can be dispatched.
    /// </summary>
    public interface IExchangeRegistry : IKnownPluginType
    {
        /// <summary>
        /// Called during the bootstrap process to obtain a list of
        /// exchange metadata describing the exchanges/queues that
        /// should be created.
        /// </summary>
        /// <returns>List of exchange metadata definitions.</returns>
        IEnumerable<ExchangeMeta> GetDefinitions();
    }
}
using System.Collections.Generic;
using NetFusion.Base.Plugins;

namespace NetFusion.RabbitMQ.Publisher
{
    /// <summary>
    /// Defines the contact for a component responsible for returning exchange
    /// definitions to which published messages should be delivered.
    /// </summary>
    public interface IExchangeRegistry : IKnownPluginType
    {
        /// <summary>
        /// The list of configured exchange definitions specifying the exchanges
        /// to which a given type of message should be delivered when published.
        /// </summary>
        /// <returns>List of exchange defintions.</returns>
        IEnumerable<ExchangeDefinition> GetDefinitions();
    }
}
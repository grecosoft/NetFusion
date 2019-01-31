using System.Threading.Tasks;
using Amqp;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.AMQP.Modules
{
    using NetFusion.AMQP.Publisher.Internal;

    /// <summary>
    /// Module that manages a set of connections/sessions to the configured
    /// AMQP hosts.  When the host stops and is disposed, the created
    /// instances are disposed by the module.
    /// </summary>
    public interface IConnectionModule : IPluginModuleService
    {
        /// <summary>
        /// Returns the session associated with a specific host.
        /// </summary>
        /// <param name="hostName">The host name.</param>
        /// <returns>The session used to communicate with the host.</returns>
        Task<Session> GetSession(string hostName);
        
        /// <summary>
        /// For a message publisher, sets the AMQP SenderLink instance
        /// used to send messages to the defined host item.
        /// </summary>
        /// <param name="session">The session on which the sender-link will be created.</param>
        /// <param name="senderHostItem">Reference representing the host item to which
        /// messages can be sent.</param>
        void SetSenderLink(Session session, ISenderHostItem senderHostItem);
    }
}
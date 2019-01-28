using System.Threading.Tasks;
using Amqp;
using NetFusion.Azure.Messaging.Publisher.Internal;
using NetFusion.Azure.Messaging.Settings;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.Azure.Messaging.Modules
{
    /// <summary>
    /// Module that manages a set of connections/sessions to the configured
    /// Azure namespaces.  When the host stops and is disposed, the created
    /// instances are disposed by the module.
    /// </summary>
    public interface IConnectionModule : IPluginModuleService
    {
        /// <summary>
        /// Returns the session associated with a specific namespace.
        /// </summary>
        /// <param name="namespaceName">The Azure namespace.</param>
        /// <returns>The session used to communicate with Azure.</returns>
        Task<Session> GetSession(string namespaceName);
        
        /// <summary>
        /// For a message publisher, sets the AMQP SenderLink instance
        /// used to send messages to the defined namespace object.
        /// </summary>
        /// <param name="session">The session on which the sender-link will be created.</param>
        /// <param name="nsSender">Reference representing the namespace object.</param>
        void SetSenderLink(Session session, ILinkedItem nsSender);

        /// <summary>
        /// Returns the application settings for a named namespace.
        /// </summary>
        /// <param name="namespaceName">The name of the namespace.</param>
        /// <returns>The namespace settings or an exception if not found.</returns>
        NamespaceSettings GetNamespaceSettings(string namespaceName);
    }
}
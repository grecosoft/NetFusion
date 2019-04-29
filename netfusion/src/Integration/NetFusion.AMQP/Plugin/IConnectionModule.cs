using System;
using System.Threading.Tasks;
using Amqp;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.AMQP.Plugin
{
    /// <summary>
    /// Module that manages a set of connections/sessions to the configured
    /// AMQP hosts.  When the host stops and is disposed, the created
    /// instances are disposed by the module.
    /// </summary>
    public interface IConnectionModule : IPluginModuleService
    {
        /// <summary>
        /// Return a new session, associated with the specific host connection,
        /// used to process received messages.
        /// </summary>
        /// <param name="hostName">The host name specified within the application's settings.</param>
        /// <returns>The session used to communicate with the host.</returns>
        Session CreateReceiverSession(string hostName);
        
        /// <summary>
        /// Returns the session, associated with the specified host connection,
        /// used to send messages. 
        /// </summary>
        /// <param name="hostName">The host name specified within the application's settings.</param>
        /// <returns></returns>
        Task<Session> CreateSenderSession(string hostName);

        /// <summary>
        /// Allows a handler to be set that is invoked when a receiver connection is closed.
        /// </summary>
        /// <param name="handler">Delegate passed the name of the host that was closed.</param>
        void SetReceiverConnectionCloseHandler(Action<string> handler);

        void SimulateClose();
    }
}
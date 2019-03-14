using Amqp;
using NetFusion.Bootstrap.Plugins;

namespace NetFusion.AMQP.Modules
{
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
        Session CreateReceiverSession(string hostName);
        
        Session GetSenderSession(string hostName);
    }
}
using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Client.Resources;

namespace NetFusion.Rest.Config.Modules
{
    /// <summary>
    /// Exposes services defined by the plug-in module used by service components.
    /// </summary>
    public interface IClientFactoryModule : IPluginModuleService
    {
        /// <summary>
        /// Given a client name specified within the application host's configuration, 
        /// determines the associated base address.
        /// </summary>
        /// <param name="clientName">The name of the client specified within the host's configuration.</param>
        /// <returns>The corresponding base address.</returns>
        string GetBaseAddressForName(string clientName);

        /// <summary>
        /// Returns the entry-point resource associated with the configured client name.
        /// </summary>
        /// <param name="clientName">The name of the client specified with the host's configuration.</param>
        /// <returns>The entry-point resource.</returns>
        HalEntryPointResource GetEntryPointResource(string clientName);
    }
}

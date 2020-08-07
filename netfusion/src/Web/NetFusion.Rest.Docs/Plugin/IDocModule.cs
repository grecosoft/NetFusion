using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Docs.Entities;
using NetFusion.Rest.Docs.Plugin.Configs;

namespace NetFusion.Rest.Docs.Plugin
{
    /// <summary>
    /// Plugin module responsible for configuring and registering
    /// services required to provide REST Api documentation.
    /// </summary>
    public interface IDocModule : IPluginModuleService
    {
        /// <summary>
        /// Reference to the REST Documentation configuration settings.
        /// </summary>
        RestDocConfig RestDocConfig { get; }
        
        /// <summary>
        /// Reference to HAL specific comments.
        /// </summary>
        HalComments HalComments { get; }
    }
}
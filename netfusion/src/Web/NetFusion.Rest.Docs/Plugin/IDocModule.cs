using NetFusion.Bootstrap.Plugins;
using NetFusion.Rest.Docs.Plugin.Configs;

namespace NetFusion.Rest.Docs.Plugin
{
    /// <summary>
    /// Plugin service providing access to REST Documentation.
    /// </summary>
    public interface IDocModule : IPluginModuleService
    {
        /// <summary>
        /// Reference to the REST Documentation configuration settings.
        /// </summary>
        RestDocConfig RestDocConfig { get; }
    }
}
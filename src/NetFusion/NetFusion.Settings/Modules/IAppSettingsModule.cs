using NetFusion.Bootstrap.Plugins;
using NetFusion.Settings.Configs;

namespace NetFusion.Settings.Modules
{
    /// <summary>
    /// Services exposed by the application settings module.
    /// </summary>
    public interface IAppSettingsModule : IPluginModuleService
    {
        /// <summary>
        /// The overall general application configurations.
        /// </summary>
        NetFusionConfig AppConfig { get; }
    }
}

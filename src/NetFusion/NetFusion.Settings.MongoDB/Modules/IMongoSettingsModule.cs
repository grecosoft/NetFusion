using NetFusion.Bootstrap.Plugins;
using NetFusion.Settings.Mongo.Configs;

namespace NetFusion.Settings.Mongo.Modules
{
    /// <summary>
    /// Module interface providing access to the configuration
    /// that should be used to load application settings from
    /// a collection.
    /// </summary>
    public interface IMongoSettingsModule : IPluginModuleService
    {
        /// <summary>
        /// Reference to the loaded configuration.
        /// </summary>
        MongoAppSettingsConfig MongoAppSettingsConfig { get; }
    }
}

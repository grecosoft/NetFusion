using NetFusion.Bootstrap.Plugins;
using NetFusion.Common.Extensions;
using NetFusion.MongoDB.Modules;
using NetFusion.Settings.MongoDB.Configs;

namespace NetFusion.Settings.MongoDB.Modules
{
    /// <summary>
    /// Determines the settings that should be used to load application settings from MongoDB.  
    /// The settings specified within the application's configuration file are used to override 
    /// any default MongoAppSettingsConfig property values.  If no application configuration 
    /// settings are specified, then a default instance of the MongoAppSettingsConfig is used.
    /// </summary>
    public class MongoSettingsModule : PluginModule, 
        IMongoSettingsModule
    {
        // IMongoSettingsModule
        public MongoAppSettingsConfig MongoAppSettingsConfig { get; private set; }

        public override void Initialize()
        {
            this.MongoAppSettingsConfig = this.Context.Plugin.GetConfig<MongoAppSettingsConfig>();
            SetHostAppConfigFileSettings(this.MongoAppSettingsConfig);
        }

        // The host can specify the MongoDb collection in which application settings are stored.
        // The following locates the MongoDb mapping for the base AppSettings entity and updates
        // associated collection.
        public override void Configure()
        {
            var mappingModule = this.Context.GetPluginModule<IMongoMappingModule>();
            var entityMapping = mappingModule.GetEntityMap<AppSettings>();
            entityMapping.CollectionName = this.MongoAppSettingsConfig.CollectionName;
        }

        public void SetHostAppConfigFileSettings(MongoAppSettingsConfig settingsConfig)
        {
            if (!this.Context.Plugin.IsConfigSet<MongoAppSettingsConfigSection>()) return;
            
            var configSection = this.Context.Plugin.GetConfig<MongoAppSettingsConfigSection>();
            var mongoAppConfig = configSection.MongoAppSettingsConfig;

            if (mongoAppConfig != null)
            {
                // Override any default settings with the values specified within the application's
                // configuration file.
                if (!mongoAppConfig.MongoUrl.IsNullOrWhiteSpace())
                {
                    settingsConfig.MongoUrl = mongoAppConfig.MongoUrl;
                }

                if (!mongoAppConfig.DatabaseName.IsNullOrWhiteSpace())
                {
                    settingsConfig.DatabaseName = mongoAppConfig.DatabaseName;
                }

                if (!mongoAppConfig.CollectionName.IsNullOrWhiteSpace())
                {
                    settingsConfig.CollectionName = mongoAppConfig.CollectionName;
                }
            }
        }
    }
 }

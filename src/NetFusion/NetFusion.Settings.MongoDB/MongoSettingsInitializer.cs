using MongoDB.Driver;
using NetFusion.Common.Extensions;
using NetFusion.MongoDB.Modules;
using NetFusion.Settings.MongoDB.Configs;
using NetFusion.Settings.MongoDB.Modules;
using NetFusion.Settings.Strategies;
using System;
using System.Linq;

namespace NetFusion.Settings.MongoDB
{
    /// <summary>
    /// Generic implementation of an application settings initializer that loads
    /// an application setting from MongoDb.  
    /// </summary>
    /// <typeparam name="TSettings"></typeparam>
    public class MongoSettingsInitializer<TSettings> : AppSettingsInitializer<TSettings>
        where TSettings : IAppSettings
    {
        private readonly IMongoMappingModule _mongoMappingModule;
        private readonly IMongoSettingsModule _mongoSettingModule;

        public MongoSettingsInitializer(
            IMongoMappingModule mongoMappingModule,
            IMongoSettingsModule mongoSettingsModule)
        {
            _mongoMappingModule = mongoMappingModule;
            _mongoSettingModule = mongoSettingsModule;
        }

        protected override IAppSettings OnConfigure(TSettings settings)
        {
            // Check to see if setting is registered as a MongoDb Know setting type.
            string typeDiscriminator = _mongoMappingModule.GetEntityDiscriminator(typeof(AppSettings), typeof(TSettings));
            if (typeDiscriminator == null)
            {
                return null;
            }

            var settingsColl = GetSettingsCollection((MongoAppSettingsConfig)_mongoSettingModule.MongoAppSettingsConfig);
            var filter = BuildAppSettingsFilter(settings, typeDiscriminator, Environment.MachineName);
           
            // Check if there is a machine specific settings configured.
            var foundSettings = settingsColl.Find(filter).ToListAsync().Result;
            if (foundSettings.Empty())
            {
                // Check if there is a non-machine specific setting.
                filter = BuildAppSettingsFilter(settings, typeDiscriminator);
                foundSettings = settingsColl.Find(filter).ToListAsync().Result;
            }

            if (foundSettings.Count() > 1)
            {
                throw new InvalidOperationException(
                    $"More one setting configuration was found for the setting type: {typeof(TSettings)} " +
                    $"for the environment: {settings.Environment}");
            }

            return foundSettings.FirstOrDefault();
        }

        private IMongoCollection<AppSettings> GetSettingsCollection(MongoAppSettingsConfig settingsConfig)
        {
            var appSettingsConfig = _mongoSettingModule.MongoAppSettingsConfig;
            var client = new MongoClient(appSettingsConfig.MongoUrl);
            var settingDb = client.GetDatabase(appSettingsConfig.DatabaseName);
            return settingDb.GetCollection<AppSettings>(appSettingsConfig.CollectionName);
        }

        private FilterDefinition<AppSettings> BuildAppSettingsFilter(
            IAppSettings settings,
            string typeDiscriminator,
            string machineName = null)
        {
            var builder = Builders<AppSettings>.Filter;

            var filter = builder.Eq(s => s.ApplicationId, settings.ApplicationId);
            filter = filter & builder.Eq(s => s.Environment, settings.Environment);

            if (machineName != null)
            {
                filter = filter & builder.Eq(s => s.MachineName, machineName.ToLower());
            }

            return filter & builder.Eq("_t", typeDiscriminator);
        }
    }
}

using MongoDB.Driver;
using NetFusion.Bootstrap.Exceptions;
using NetFusion.MongoDB.Modules;
using NetFusion.Settings.MongoDB.Configs;
using NetFusion.Settings.MongoDB.Modules;
using NetFusion.Settings.Strategies;
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

            IMongoCollection<AppSettings> settingsColl = GetSettingsCollection(_mongoSettingModule.MongoAppSettingsConfig);

            // The following are the order in which the MongoDB collection should be searched 
            // for settings.  The first found match is used.
            var machineEnvFilter = BuildAppSettingsFilter(typeDiscriminator,
                settings.ApplicationId,
                settings.MachineName,
                settings.Environment);

           var machineFilter = BuildAppSettingsFilter(typeDiscriminator,
                settings.ApplicationId,
                settings.MachineName);

            var envFilter = BuildAppSettingsFilter(typeDiscriminator,
                settings.ApplicationId,
                envType: settings.Environment);

            var appFilter = BuildAppSettingsFilter(typeDiscriminator, 
                settings.ApplicationId);

            return SearchSettingsCollection(settingsColl, machineEnvFilter)
                ?? SearchSettingsCollection(settingsColl, machineFilter)
                ?? SearchSettingsCollection(settingsColl, envFilter)
                ?? SearchSettingsCollection(settingsColl, appFilter);
        }

        private IMongoCollection<AppSettings> GetSettingsCollection(MongoAppSettingsConfig appSettingsConfig)
        {
            var client = new MongoClient(appSettingsConfig.MongoUrl);
            var settingDb = client.GetDatabase(appSettingsConfig.DatabaseName);
            return settingDb.GetCollection<AppSettings>(appSettingsConfig.CollectionName);
        }

        private FilterDefinition<AppSettings> BuildAppSettingsFilter(string typeDiscriminator, 
            string applicationId,
            string machineName = null,
            EnvironmentTypes? envType = null)
        {
            var builder = Builders<AppSettings>.Filter;
            var filter = builder.Eq(s => s.ApplicationId, applicationId);

            if (machineName != null)
            {
                filter = filter & builder.Eq(s => s.MachineName, machineName.ToLower());
            }

            if (envType != null)
            {
                filter = filter & builder.Eq(s => s.Environment, envType.Value);
            }

            return filter & builder.Eq("_t", typeDiscriminator);
        }

        private IAppSettings SearchSettingsCollection(IMongoCollection<AppSettings> collection,
            FilterDefinition<AppSettings> filter)
        {
            var foundSettings = collection.Find(filter).ToListAsync().Result;

            if (foundSettings.Count() > 1)
            {
                throw new ContainerException(
                    $"More one setting configuration was found for the setting type: {typeof(TSettings)}");
            }

            return foundSettings.FirstOrDefault();
        }
    }
}

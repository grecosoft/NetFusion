using NetFusion.MongoDB;
using NetFusion.Settings;
using MongoDB.Driver;
using RefArch.Domain.Samples.Settings;
using RefArch.Infrastructure.Samples.MongoDB;
using System.Threading.Tasks;
using RefArch.Domain.Samples.MongoDb;

namespace RefArch.Infrastructure.Samples.Settings
{
    public class SettingsInitService : ISettingsInitService
    {
        private readonly IMongoDbClient<NetFusionDB> _netFusionDbClient;

        public SettingsInitService(
            IMongoDbClient<NetFusionDB> netFusionDbClient)
        {
            _netFusionDbClient = netFusionDbClient;
        }

        public async Task<MongoInitializedSettings> InitMongoDbStoredSettings()
        {
            var settingsColl = _netFusionDbClient.GetCollection<AppSettings>();

            var settings = await settingsColl.OfType<MongoInitializedSettings>()
                .Find(_ => true)
                .FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new MongoInitializedSettings
                {
                    ApplicationId = "570bd2be397e063470b2be9e",
                    Environment = EnvironmentTypes.Development,
                    MachineName = null,
                    Value1 = 1000,
                    Value2 = 2000
                };

                settingsColl.InsertOne(settings);
            }
            return settings;
        }
    }
}
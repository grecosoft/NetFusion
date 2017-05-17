using InfrastructureTests.MongoDB.Mocks;
using NetFusion.MongoDB;
using NetFusion.Settings;
using NetFusion.Test.Plugins;

namespace InfrastructureTests.MongoDB
{
    public static class MongoDbTestExtensions
    {
        public static TestTypeResolver UseDefaultSettingsConfig(this TestTypeResolver resolver)
        {
            resolver.AddPlugin<MockAppHostPlugin>();

            resolver.AddPlugin<MockCorePlugin>()
                 .UseSettingsPlugin()
                 .UseMongoDbPlugin();
                

            return resolver;
        }

        public static void SetupValidMongoConsumingPlugin(this TestTypeResolver resolver)
        {
            resolver.AddPlugin<MockAppComponentPlugin>()
                .AddPluginType<MockMongoModule>()
                .AddPluginType<MockMongoDb>()               // Database settings
                .AddPluginType<MockEntity>()                // Database entity
                .AddPluginType<MockDerivedEntity>()
                .AddPluginType<MockEntityClassMap>()        // Entity mapping
                .AddPluginType<MockDerivedEntityClassMap>();
        }
    }
}

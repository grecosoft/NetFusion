using InfrastructureTests.MongoDB.Mocks;
using NetFusion.MongoDB;
using NetFusion.Settings;
using NetFusion.Test.Plugins;

namespace InfrastructureTests.MongoDB
{
    public static class TestFixtureExtensions
    {
        public static TestTypeResolver WithMongoDbConfiguredHost(this TestTypeResolver resolver)
        {
            resolver.AddPlugin<MockAppHostPlugin>()
                .UseSettingsPlugin()
                .UseMongoDbPlugin();

            return resolver;
        }

        public static void SetupMongoConsumingPlugin(this TestTypeResolver resolver)
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

using NetFusion.Bootstrap.Testing;
using NetFusion.MongoDB.Modules;

namespace NetFusion.MongoDB.Testing
{
    public static class HostTypeResolverExtensions
    {
        public static void AddMongoDbPlugin(this TestTypeResolver resolver)
        {
            resolver.AddPlugin<MockCorePlugin>()
                .AddPluginType<MappingModule>()
                .AddPluginType<MongoModule>();
        }
    }
}

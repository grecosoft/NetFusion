using NetFusion.Bootstrap.Testing;
using NetFusion.MongoDB.Modules;

namespace NetFusion.MongoDB.Testing
{
    public static class HostTypeResolverExtensions
    {
        public static void AddMongoDbPlugin(this HostTypeResolver resolver)
        {
            resolver.AddPlugin<MockCorePlugin>()
                .AddPluginType<MappingModule>()
                .AddPluginType<MongoModule>();
        }
    }
}

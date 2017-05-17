using Autofac;
using NetFusion.Bootstrap.Extensions;
using NetFusion.MongoDB;
using NetFusion.MongoDB.Modules;

namespace InfrastructureTests.MongoDB.Mocks
{
    public class MockMongoModule : MongoModule
    {
        public override void RegisterComponents(ContainerBuilder builder)
        {
            // Override the base register method so a mock derived version of the
            // MongoDbClient can be used for testing.
            builder.RegisterGeneric(typeof(MockMongoDbClient<>))
                .As(typeof(IMongoDbClient<>))
                .NotifyOnActivating()
                .SingleInstance();
        }
    }
}

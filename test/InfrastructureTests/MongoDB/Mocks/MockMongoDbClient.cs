using NetFusion.MongoDB.Configs;
using NetFusion.MongoDB.Core;
using NetFusion.MongoDB.Modules;

namespace InfrastructureTests.MongoDB.Mocks
{
    public class MockMongoDbClient<TSettings> : MongoDbClient<TSettings>
        where TSettings : MongoSettings
    {
        public bool IsActivated { get; private set; }

        public MockMongoDbClient(TSettings dbSettings, IMongoMappingModule mappingModule) 
            : base(dbSettings, mappingModule)
        {
        }

        public override void OnActivated()
        {
            this.IsActivated = true;
        }
    }
}

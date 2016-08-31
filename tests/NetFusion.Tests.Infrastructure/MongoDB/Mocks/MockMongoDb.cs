using NetFusion.MongoDB.Configs;

namespace NetFusion.Tests.MongoDB.Mocks
{
    public class MockMongoDb : MongoSettings
    {
        public MockMongoDb()
        {
            this.IsInitializationRequired = false;
            this.MongoUrl = "TestMongoUrl";
            this.UserName = "TestUserName";
        }
    }
}

using NetFusion.MongoDB.Configs;

namespace InfrastructureTests.MongoDB.Mocks
{
    public class MockMongoDb : MongoSettings
    {
        public MockMongoDb()
        {
            this.MongoUrl = "TestMongoUrl";
            this.UserName = "TestUserName";
        }
    }
}

using Autofac;
using FluentAssertions;
using InfrastructureTests.MongoDB.Mocks;
using NetFusion.MongoDB;
using NetFusion.Test.Container;
using Xunit;

namespace InfrastructureTests.MongoDB
{
    public class MongoClientTests
    {
        /// <summary>
        /// A given MongoDB database is identified by a derived MongoSettings class.
        /// A client to the database is resolved by specifying the IMongoDbClient
        /// with the settings class as the generic parameters.
        /// </summary>
        [Fact(DisplayName = "Database Client can be Resolved for Database")]
        public void DatabaseClientCanBeResolved_ForDatabase()
        {
             ContainerFixture.Test(fixture => { fixture
                .Arrange
                    .Resolver(r => {
                        r.WithMongoDbConfiguredHost();
                        r.SetupMongoConsumingPlugin();
                    })
                .Act.OnContainer(c => c.Build())
                .Assert.Container(c =>
                {
                    var client = (MockMongoDbClient<MockMongoDb>)c.Services.ResolveOptional<IMongoDbClient<MockMongoDb>>();
                    client.Should().NotBeNull();

                    client.DbSettings.Should().BeOfType<MockMongoDb>();
                    client.DbSettings.Should().NotBeNull();
                    client.DbSettings.MongoUrl.Should().NotBeNull();
                });
            });
        }

        /// <summary>
        /// The connection is established to the server when the MongoDB client is activated. 
        /// </summary>
        [Fact(DisplayName = "Database Client activated when Resolved")]
        public void DatabaseClientActivated_WhenResolved()
        {
            ContainerFixture.Test(fixture => { fixture
                .Arrange
                    .Resolver(r => {
                        r.WithMongoDbConfiguredHost();
                        r.SetupMongoConsumingPlugin();
                    })
                .Act.OnContainer(c => c.Build())
                .Assert.Container(c =>
                {
                    var client = (MockMongoDbClient<MockMongoDb>)c.Services.ResolveOptional<IMongoDbClient<MockMongoDb>>();
                    client.IsActivated.Should().BeTrue();
                });
            });
        }
    }
}

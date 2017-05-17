using Autofac;
using FluentAssertions;
using InfrastructureTests.MongoDB.Mocks;
using NetFusion.Bootstrap.Container;
using NetFusion.MongoDB;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
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
        [Fact(DisplayName = nameof(DatabaseClientResolved_ForDatabase))]
        public void DatabaseClientResolved_ForDatabase()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.UseDefaultSettingsConfig();
                    config.SetupValidMongoConsumingPlugin();
                })
                .Test(
                    c => c.Build(),
                    (IAppContainer c) =>
                    {
                        var client = (MockMongoDbClient<MockMongoDb>)c.Services.ResolveOptional<IMongoDbClient<MockMongoDb>>();
                        client.Should().NotBeNull();

                        client.DbSettings.Should().BeOfType<MockMongoDb>();
                        client.DbSettings.Should().NotBeNull();
                        client.DbSettings.MongoUrl.Should().NotBeNull();
                    });
        }

        /// <summary>
        /// The connection is established to the server when the MongoDB client is
        /// activated.  This allows for any exceptions to be thrown for the activation
        /// method and not from within the constructor.
        /// </summary>
        [Fact(DisplayName = nameof(DatabaseClientActivated_WhenResolved))]
        public void DatabaseClientActivated_WhenResolved()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.UseDefaultSettingsConfig();
                    config.SetupValidMongoConsumingPlugin();
                })
                .Test(
                    c => c.Build(),
                    (IAppContainer c) =>
                    {
                        var client = (MockMongoDbClient<MockMongoDb>)c.Services.ResolveOptional<IMongoDbClient<MockMongoDb>>();
                        client.IsActivated.Should().BeTrue();
                    });
        }
    }
}

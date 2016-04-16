using Autofac;
using FluentAssertions;
using NetFusion.Bootstrap.Container;
using NetFusion.Bootstrap.Testing;
using NetFusion.MongoDB;
using NetFusion.Tests.Core.Bootstrap;
using NetFusion.Tests.MongoDB.Mocks;
using Xunit;

namespace NetFusion.Tests.MongoDB
{
    public class MongoClientTests
    {
        /// <summary>
        /// A given MongoDB database is identified by a derived MongoSettings class.
        /// A client to the database is resolved by specifying the IMongoDbClient
        /// with the settings class as the generic parameters.
        /// </summary>
        [Fact]
        public void CanResolveEntityContextForDatabaseSettings()
        {
            ContainerSetup
                .Arrange((HostTypeResolver config) =>
                {
                    config.SetupHostApp();
                    config.SetupMogoDbPlugIn();
                    config.SetupValidMongoConsumingPlugin();
                })
                .Act(c =>
                {
                    c.Build();
                })
                .Assert((AppContainer c) =>
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
        [Fact]
        public void MongoClientIsActivated()
        {
            ContainerSetup
                .Arrange((HostTypeResolver config) =>
                {
                    config.SetupHostApp();
                    config.SetupMogoDbPlugIn();
                    config.SetupValidMongoConsumingPlugin();
                })
                .Act(c =>
                {
                    c.Build();
                })
                .Assert((AppContainer c) =>
                {
                    var client = (MockMongoDbClient<MockMongoDb>)c.Services.ResolveOptional<IMongoDbClient<MockMongoDb>>();
                    client.IsActivated.Should().BeTrue();
                });
        }
    }
}

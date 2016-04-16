using FluentAssertions;
using MongoDB.Bson.Serialization;
using NetFusion.Bootstrap.Testing;
using NetFusion.MongoDB.Modules;
using NetFusion.Tests.Core.Bootstrap;
using NetFusion.Tests.MongoDB.Mocks;
using System.Linq;
using Xunit;

namespace NetFusion.Tests.MongoDB
{
    public class MongoMappingTests
    {
        /// <summary>
        /// All MongoDB entity class mappings are discovered by the module.
        /// </summary>
        [Fact]
        public void AllEntityClassMapsDiscovered()
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
                .Assert((MappingModule module) =>
                {
                    module.Mappings.Should().HaveCount(1);
                    module.Mappings.First().Should().BeOfType<MockEntityClassMap>();
                });
        }

        /// <summary>
        /// The discovered MongoDB class mappings are added to the driver.
        /// </summary>
        [Fact]
        public void DiscoveredEntityClassMapsAddedToMongo()
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
                    c.Build().Start();
                })
                .Assert((MappingModule module) =>
                {
                    var classMap = BsonClassMap.GetRegisteredClassMaps().FirstOrDefault();
                    classMap.Should().NotBeNull();
                    classMap.Should().BeOfType<MockEntityClassMap>();
                });
        }

        /// <summary>
        /// Class maps can obtained by using services exposed from the module.
        /// </summary>
        [Fact]
        public void CanLookupClassMapping()
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
                .Assert((MappingModule module) =>
                {
                    var entityMap = module.GetEntityMap(typeof(MockEntity));
                    entityMap.Should().NotBeNull();
                });
        }
    }
}

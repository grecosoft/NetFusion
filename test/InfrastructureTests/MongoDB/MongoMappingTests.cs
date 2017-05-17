using FluentAssertions;
using InfrastructureTests.MongoDB.Mocks;
using MongoDB.Bson.Serialization;
using NetFusion.MongoDB.Modules;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using System.Linq;
using Xunit;

namespace InfrastructureTests.MongoDB
{
    public class MongoMappingTests
    {
        /// <summary>
        /// All MongoDB entity class mappings are discovered by the module.
        /// </summary>
        [Fact(DisplayName = nameof(EntityClassMaps_DiscoveredByModule))]
        public void EntityClassMaps_DiscoveredByModule()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.UseDefaultSettingsConfig();
                    config.SetupValidMongoConsumingPlugin();
                })
                .Test(
                    c => c.Build(),
                    (MappingModule module) =>
                    {
                        module.Mappings.Should().HaveCount(2);
                        module.Mappings.OfType<MockEntityClassMap>().Should().HaveCount(1);
                        module.Mappings.OfType<MockDerivedEntityClassMap>().Should().HaveCount(1);
                    });
        }

        /// <summary>
        /// The discovered MongoDB class mappings are added to the driver.
        /// </summary>
        [Fact(DisplayName = nameof(DiscoveredEntityClassMaps_AddedToMongo))]
        public void DiscoveredEntityClassMaps_AddedToMongo()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.UseDefaultSettingsConfig();
                    config.SetupValidMongoConsumingPlugin();
                })
                .Test(
                    c => c.Build().Start(),
                    (MappingModule module) =>
                    {
                        var classMap = BsonClassMap.GetRegisteredClassMaps().FirstOrDefault();
                        classMap.Should().NotBeNull();
                        classMap.Should().BeOfType<MockEntityClassMap>();
                    });
        }

        /// <summary>
        /// Class maps can obtained by using services exposed from the module.
        /// </summary>
        [Fact(DisplayName = nameof(CanLookup_ClassMapping))]
        public void CanLookup_ClassMapping()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.UseDefaultSettingsConfig();
                    config.SetupValidMongoConsumingPlugin();
                })
                .Test(
                    c => c.Build(),
                    (MappingModule module) =>
                    {
                        var entityMap = module.GetEntityMap(typeof(MockEntity));
                        entityMap.Should().NotBeNull();
                    });
        }

        [Fact(DisplayName = nameof(CanLookup_KnowTypeClassDescriminator))]
        public void CanLookup_KnowTypeClassDescriminator()
        {
            ContainerSetup
                .Arrange((TestTypeResolver config) =>
                {
                    config.UseDefaultSettingsConfig();
                    config.SetupValidMongoConsumingPlugin();
                })
                .Test(
                    c => c.Build(),
                    (MappingModule module) =>
                    {
                        var descriminator  = module.GetEntityDiscriminator(typeof(MockEntity), typeof(MockDerivedEntity));
                        descriminator.Should().Be("expected_descriminator_name");
                    });
        }
    }
}

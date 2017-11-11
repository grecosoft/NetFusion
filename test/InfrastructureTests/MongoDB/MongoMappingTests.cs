using FluentAssertions;
using InfrastructureTests.MongoDB.Mocks;
using MongoDB.Bson.Serialization;
using NetFusion.MongoDB.Modules;
using NetFusion.Test.Container;
using System.Linq;
using Xunit;

namespace InfrastructureTests.MongoDB
{
    public class MongoMappingTests
    {
        /// <summary>
        /// All MongoDB entity class mappings are discovered by the module.
        /// </summary>
        [Fact(DisplayName = "Entity Class Maps discovered by Module")]
        public void EntityClassMaps_DiscoveredByModule()
        {
            ContainerFixture.Test(fixture => { fixture
                .Arrange
                    .Resolver(r => {
                        r.WithMongoDbConfiguredHost();
                        r.SetupMongoConsumingPlugin();
                    })
                .Act.OnContainer(c => c.Build())
                .Assert.PluginModule<MappingModule>(m =>
                {
                    m.Mappings.Should().HaveCount(2);
                    m.Mappings.OfType<MockEntityClassMap>().Should().HaveCount(1);
                    m.Mappings.OfType<MockDerivedEntityClassMap>().Should().HaveCount(1);
                });
            });
        }

        /// <summary>
        /// The discovered MongoDB class mappings are added to the driver.
        /// </summary>
        [Fact(DisplayName = "Discovered Entity Class Maps added to MongoDb")]
        public void DiscoveredEntityClassMaps_AddedToMongoDb()
        {
            ContainerFixture.Test(fixture => { fixture
                .Arrange
                    .Resolver(r => {
                        r.WithMongoDbConfiguredHost();
                        r.SetupMongoConsumingPlugin();
                    })
                .Act.OnContainer(c => c.Build().Start())
                .Assert.Container(_ =>
                {                    
                    var classMap = BsonClassMap.GetRegisteredClassMaps().FirstOrDefault();
                    classMap.Should().NotBeNull();
                    classMap.Should().BeOfType<MockEntityClassMap>();
                });
            });
        }

        /// <summary>
        /// Class maps can obtained by using services exposed from the module.
        /// </summary>
        [Fact(DisplayName = "Can lookup Class Mapping using Module")]
        public void CanLookup_ClassMapping_UsingModule()
        {
            ContainerFixture.Test(fixture => { fixture
                .Arrange
                    .Resolver(r => {
                        r.WithMongoDbConfiguredHost();
                        r.SetupMongoConsumingPlugin();
                    })
                .Act.OnContainer(c => c.Build())
                .Assert.PluginModule<MappingModule>(m =>
                {
                    var entityMap = m.GetEntityMap(typeof(MockEntity));
                    entityMap.Should().NotBeNull();
                });
            });
        }

        [Fact(DisplayName = nameof(CanLookup_KnowTypeClassDescriminator))]
        public void CanLookup_KnowTypeClassDescriminator()
        {
            ContainerFixture.Test(fixture => { fixture
                .Arrange
                    .Resolver(r => {
                        r.WithMongoDbConfiguredHost();
                        r.SetupMongoConsumingPlugin();
                    })
                .Act.OnContainer(c => c.Build())
                .Assert.PluginModule<MappingModule>(m =>
                {
                    var descriminator = m.GetEntityDiscriminator(typeof(MockEntity), typeof(MockDerivedEntity));
                    descriminator.Should().Be("expected_descriminator_name");
                });
            });
        }
    }
}

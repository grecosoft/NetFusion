using Microsoft.Extensions.DependencyInjection;
using NetFusion.Services.Mapping;
using NetFusion.Services.Mapping.Core;
using NetFusion.Services.Mapping.Plugin;
using NetFusion.Services.Mapping.Plugin.Modules;
using NetFusion.Services.UnitTests.Mapping.Entities;
using NetFusion.Services.UnitTests.Mapping.Strategies;

namespace NetFusion.Services.UnitTests.Mapping;

public class MappingModuleTests
{
    /// <summary>
    /// IObjectMapper service can be injected into any component registered
    /// within the container and used to map one type to another.
    /// </summary>
    [Fact]
    public void WhenBootstrapped_ObjectMapper_RegisteredAsScopedService()
    {
        var testPlugin = new MockHostPlugin();

        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange
                .Container(c =>
                {
                    c.RegisterPlugins(testPlugin);
                    c.RegisterPlugin<MappingPlugin>();
                })
                .Assert.ServiceCollection(c =>
                {
                    c.HasRegistration<IObjectMapper, ObjectMapper>(ServiceLifetime.Singleton)
                        .Should().BeTrue("Object mapper not registered as service");
                });
        });    
    }
    
    /// <summary>
    /// When the plugin bootstraps, all IMappingStrategy classes are found
    /// and stored as metadata used at runtime to map objects between types.
    /// </summary>
    [Fact]
    public void WhenBootstrapped_MappingStrategies_MetadataBuilt()
    {
        var testPlugin = new MockHostPlugin();
        testPlugin.AddPluginType<TestMapStrategyOneToTwo>();
        testPlugin.AddPluginType<TestMapStrategyThreeToTwo>();

        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange
                .Container(c =>
                {
                    c.RegisterPlugins(testPlugin);
                    c.RegisterPlugin<MappingPlugin>();
                })
                .Assert.PluginModule((MappingModule m) =>
                {
                    m.SourceTypeMappings.Should().HaveCount(2);
                    
                    Assert.True(m.SourceTypeMappings.Contains(typeof(TestMapTypeOne)));
                    Assert.True(m.SourceTypeMappings.Contains(typeof(TestMapTypeThree)));
                });
        });
    }
    
    /// <summary>
    /// When the mapping plugin bootstraps, the details of all the possible mappings
    /// for a given source type to target types are determined.
    /// </summary>
    [Fact]
    public void WhenBootstrapped_StrategyMetadata_BuiltCorrectly()
    {
        var testPlugin = new MockHostPlugin();
        testPlugin.AddPluginType<TestMapStrategyOneToTwo>();

        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange
                .Container(c =>
                {
                    c.RegisterPlugins(testPlugin);
                    c.RegisterPlugin<MappingPlugin>();
                })
                .Assert.PluginModule((MappingModule m) =>
                {
                    m.SourceTypeMappings.Should().HaveCount(1, "Only single mapping specified");
                    
                    Assert.True(m.SourceTypeMappings.Contains(typeof(TestMapTypeOne)), 
                        "Possible strategies keyed by source type");

                    var targetMaps = m.SourceTypeMappings[typeof(TestMapTypeOne)].ToArray();
                    targetMaps.Should().HaveCount(1, "Source type should have one target mapping");

                    var targetMap = targetMaps.First();
                    
                    targetMap.StrategyType.Should().Be(typeof(TestMapStrategyOneToTwo),
                        "Mapping strategy specified to map between source and target types");

                    targetMap.SourceType.Should().Be(typeof(TestMapTypeOne));
                    targetMap.TargetType.Should().Be(typeof(TestMapTypeTwo));
                });
        });
    }
    
    /// <summary>
    /// When the plugin bootstraps, all IMappingStrategyFactory classes are found
    /// and the IMappingStrategy objects instances cached.
    /// </summary>
    [Fact]
    public void WhenBootstrapped_MappingStrategyFactories_ProvideStrategies()
    {
        var testPlugin = new MockHostPlugin();
        testPlugin.AddPluginType<TestMappingStrategyFactory>();

        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange
                .Container(c =>
                {
                    c.RegisterPlugins(testPlugin);
                    c.RegisterPlugin<MappingPlugin>();
                })
                .Assert.PluginModule((MappingModule m) =>
                {
                    m.SourceTypeMappings.Should().HaveCount(1);
                    Assert.True(m.SourceTypeMappings.Contains(typeof(TestMapTypeOne)));

                    var targetMappings = m.SourceTypeMappings[typeof(TestMapTypeOne)].ToArray();
                    targetMappings.Should().HaveCount(1);
                    targetMappings.First().TargetType.Should().Be(typeof(TestMapTypeTwo));
                });
        });
    }
    
    /// <summary>
    /// Strategy classes not created by a IMappingStrategyFactory are registered
    /// with the dependency-injection container and can have other services injected.
    /// </summary>
    [Fact]
    public void WhenBootstrapped_MappingStrategies_RegisteredAsScopedServices()
    {
        var testPlugin = new MockHostPlugin();
        testPlugin.AddPluginType<TestMapStrategyOneToTwo>();

        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange
                .Container(c =>
                {
                    c.RegisterPlugins(testPlugin);
                    c.RegisterPlugin<MappingPlugin>();
                })
                .Assert.ServiceCollection(c =>
                {
                    c.HasRegistration<TestMapStrategyOneToTwo>(ServiceLifetime.Scoped)
                        .Should().BeTrue("Strategy not registered as a service");
                });
        });
    }
    
    /// <summary>
    /// The IMappingStrategy instances provided by IMappingStrategyFactory defined
    /// classes are cached and used for all executions and not registered within
    /// the dependency-injection container.
    /// </summary>
    [Fact]
    public void WhenBootstrapped_FactoryBuiltStrategies_SingletonNotRegistered()
    {
        var testPlugin = new MockHostPlugin();
        testPlugin.AddPluginType<TestMappingStrategyFactory>();

        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange
                .Container(c =>
                {
                    c.RegisterPlugins(testPlugin);
                    c.RegisterPlugin<MappingPlugin>();
                })
                .Assert.PluginModule((MappingModule m) =>
                {
                    m.SourceTypeMappings[typeof(TestMapTypeOne)].Single().StrategyInstance
                        .Should().NotBeNull("Factory provided strategies are singletons");
                });
        });
    }
    
    /// <summary>
    /// A source type can have multiple strategies mapping to different target types.
    /// For example, a business entity could be mapped to a corresponding domain-event
    /// or view model.
    /// </summary>
    [Fact]
    public void WhenBootstrapped_SourceType_CanHaveMultipleTargetTypes()
    {
        var testPlugin = new MockHostPlugin();
        testPlugin.AddPluginType<TestMapStrategyOneToTwo>();
        testPlugin.AddPluginType<TestMapStrategyOneToThree>();

        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange
                .Container(c =>
                {
                    c.RegisterPlugins(testPlugin);
                    c.RegisterPlugin<MappingPlugin>();
                })
                .Assert.PluginModule((MappingModule m) =>
                {
                    m.SourceTypeMappings.Should().HaveCount(1);
                    Assert.True(m.SourceTypeMappings.Contains(typeof(TestMapTypeOne)));

                    var targetMappings = m.SourceTypeMappings[typeof(TestMapTypeOne)].ToArray();
                    targetMappings.Should().HaveCount(2, "Source type has two possible target types");
                    
                    targetMappings.Where(tm => tm.TargetType == typeof(TestMapTypeTwo)).Should().HaveCount(1);
                    targetMappings.Where(tm => tm.TargetType == typeof(TestMapTypeThree)).Should().HaveCount(1);
                });
        });
    }
    
    /// <summary>
    /// The IObjectMapper service is used to map one object type to another.
    /// </summary>
    [Fact]
    public void SourceType_WithDefinedMapperForTarget_CanBeConvertedToTargetType()
    {
        var testPlugin = new MockHostPlugin();
        testPlugin.AddPluginType<TestMapStrategyOneToTwo>();

        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange
                .Container(c =>
                {
                    c.RegisterPlugins(testPlugin);
                    c.RegisterPlugin<MappingPlugin>();
                })
                .Act.OnService((IObjectMapper mapper) =>
                {
                    var entity = new TestMapTypeOne
                    {
                        Values = new []{ 100, 20, 300, 400 }
                    };

                    return mapper.Map<TestMapTypeTwo>(entity);
                })
                .Assert.ServiceResult((TestMapTypeTwo result) =>
                {
                    result.Min.Should().Be(20);
                    result.Max.Should().Be(400);
                    result.Sum.Should().Be(820);
                });
        });
    }
    
    [Fact]
    public void SourceType_WithUnDefinedMapperForTarget_RaisesException()
    {
        var testPlugin = new MockHostPlugin();
        testPlugin.AddPluginType<TestMapStrategyOneToTwo>();

        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange
                .Container(c =>
                {
                    c.RegisterPlugins(testPlugin);
                    c.RegisterPlugin<MappingPlugin>();
                })
                .Act.RecordException()
                .OnService((IObjectMapper mapper) =>
                {
                    var entity = new TestMapTypeOne
                    {
                        Values = new []{ 100, 20, 300, 400 }
                    };

                    return mapper.Map<string>(entity);
                })
                .Assert.Exception((MappingException ex) =>
                {
                    ex.ExceptionId.Should().Be("MAPPING_NOT_FOUND");
                });
        });
    }

    [Fact]
    public void SourceType_WithMultipleMapperForTarget_RaisesException()
    {
        var testPlugin = new MockHostPlugin();
        testPlugin.AddPluginType<TestMapStrategyOneToTwo>();
        testPlugin.AddPluginType<TestMapStrategyOneToThree>();

        ContainerFixture.Test(fixture =>
        {
            fixture.Arrange
                .Container(c =>
                {
                    c.RegisterPlugins(testPlugin);
                    c.RegisterPlugin<MappingPlugin>();
                })
                .Act.RecordException()
                .OnService((IObjectMapper mapper) =>
                {
                    var entity = new TestMapTypeOne
                    {
                        Values = [100, 20, 300, 400]
                    };

                    // Since there are multiple target types and all types are assignable 
                    // to object, two possible mappings will be found.
                    return mapper.Map<object>(entity);
                })
                .Assert.Exception((MappingException ex) =>
                {
                    ex.ExceptionId.Should().Be("MULTIPLE_MAPPINGS_FOUND");
                });
        });
    }
}
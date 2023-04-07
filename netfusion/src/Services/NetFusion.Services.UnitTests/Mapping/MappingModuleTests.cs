using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Core.TestFixtures.Extensions;
using NetFusion.Core.TestFixtures.Plugins;
using NetFusion.Services.Mapping;
using NetFusion.Services.Mapping.Core;
using NetFusion.Services.Mapping.Plugin;
using NetFusion.Services.Mapping.Plugin.Modules;

namespace NetFusion.Services.UnitTests.Mapping;

public class MappingModuleTests
{
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
                    c.AssertHasRegistration<IObjectMapper, ObjectMapper>(ServiceLifetime.Scoped);
                });
        });    
    }
    
    [Fact]
    public void WhenBootstrapped_MappingStrategies_MetadataBuilt()
    {
        var testPlugin = new MockHostPlugin();
        testPlugin.AddPluginType<TestMappingStrategyTwo>();
        testPlugin.AddPluginType<TestMappingStrategyOne>();

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
    
    [Fact]
    public void WhenBootstrapped_SourceType_CanHaveMultipleTargetTypes()
    {
        var testPlugin = new MockHostPlugin();
        testPlugin.AddPluginType<TestMappingStrategyOne>();
        testPlugin.AddPluginType<TestMappingStrategyThree>();

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
                    targetMappings.Where(m => m.TargetType == typeof(TestMapTypeTwo)).Should().HaveCount(1);
                    targetMappings.Where(m => m.TargetType == typeof(TestMapTypeThree)).Should().HaveCount(1);
                });
        });
    }
    
    [Fact]
    public void WhenBootstrapped_MappingStrategies_RegisteredAsScopedServices()
    {
        var testPlugin = new MockHostPlugin();
        testPlugin.AddPluginType<TestMappingStrategyTwo>();
        testPlugin.AddPluginType<TestMappingStrategyOne>();

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
                    c.AssertHasRegistration<TestMappingStrategyTwo>(ServiceLifetime.Scoped);
                    c.AssertHasRegistration<TestMappingStrategyOne>(ServiceLifetime.Scoped);
                });
        });
    }

    [Fact]
    public void WhenBootstrapped_StrategyMetadata_BuiltCorrectly()
    {
        var testPlugin = new MockHostPlugin();
        testPlugin.AddPluginType<TestMappingStrategyOne>();

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

                    var targetMap = m.SourceTypeMappings[typeof(TestMapTypeOne)].ToArray();
                    
                    targetMap.Should().HaveCount(1, "Source type should have one target mapping");
                    targetMap.First().StrategyType.Should().Be(typeof(TestMappingStrategyOne),
                        "Mapping strategy specified to map between source and target types");
                });
        });
    }
    
    [Fact]
    public void SourceType_WithDefinedMapperForTarget_CanBeConvertedToTargetType()
    {
        var testPlugin = new MockHostPlugin();
        testPlugin.AddPluginType<TestMappingStrategyOne>();

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
        testPlugin.AddPluginType<TestMappingStrategyOne>();

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
        testPlugin.AddPluginType<TestMappingStrategyOne>();
        testPlugin.AddPluginType<TestMappingStrategyThree>();

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

                    return mapper.Map<object>(entity);
                })
                .Assert.Exception((MappingException ex) =>
                {
                    ex.ExceptionId.Should().Be("MULTIPLE_MAPPINGS_FOUND");
                });
        });
    }
    
    public class TestMappingStrategyFactory : IMappingStrategyFactory
    {
        public IEnumerable<IMappingStrategy> GetStrategies()
        {
            yield return DelegateMap.Map((TestMapTypeOne s) => new TestMapTypeTwo
            {
                Sum = s.Values.Sum(),
                Min =  s.Values.Min(),
                Max = s.Values.Max()
            });
        }
    }

    public class TestMapTypeOne
    {
        public int[] Values { get; set; } = Array.Empty<int>();
    }

    public class TestMapTypeTwo
    {
        public int Sum { get; set; }
        public int Max { get; set; }
        public int Min { get; set; }
    }

    public class TestMapTypeThree
    {
        public int MaxAllowedValue { get; set; }
        public int[] Values { get; set; } = Array.Empty<int>();
    }
    
    public class TestMappingStrategyOne : MappingStrategy<TestMapTypeOne, TestMapTypeTwo>
    {
        protected override TestMapTypeTwo SourceToTarget(TestMapTypeOne source)
        {
            return new TestMapTypeTwo
            {
                Max = source.Values.Max(),
                Min = source.Values.Min(),
                Sum = source.Values.Sum()
            };
        }
    }

    public class TestMappingStrategyThree : MappingStrategy<TestMapTypeOne, TestMapTypeThree>
    {
        protected override TestMapTypeThree SourceToTarget(TestMapTypeOne source)
        {
            var maxVal = source.Values.Max();
            
            return new TestMapTypeThree
            {
                MaxAllowedValue = maxVal,
                Values = source.Values.Where(v => v <= maxVal).ToArray()
            };
        }
    }

    public class TestMappingStrategyTwo : MappingStrategy<TestMapTypeThree, TestMapTypeOne>
    {
        protected override TestMapTypeOne SourceToTarget(TestMapTypeThree source)
        {
            return new TestMapTypeOne
            {
                Values = source.Values.Where(v => 
                    v <= source.MaxAllowedValue).ToArray()
            };
        }
    }
}
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Core.TestFixtures.Plugins;
using NetFusion.Services.Mapping;
using NetFusion.Services.Mapping.Core;
using NetFusion.Services.Mapping.Plugin;
using NetFusion.Services.Mapping.Plugin.Modules;

namespace NetFusion.Services.UnitTests.Mapping;

public class MappingModuleTests
{
    /// <summary>
    /// All IMappingStrategy types are found when the module bootstraps.  However, unlike the strategies
    /// returned from a factory, they are not created and cached.  The mapping strategy instance is created
    /// from the service collection when needed by the current request scope.
    /// </summary>
    [Fact]
    public void AllMappingStrategies_AreFound()
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
                    Assert.Equal(1, m.SourceTypeMappings.Count);

                    // .. Get the list of possible target types for the source type:
                    Assert.True(m.SourceTypeMappings.Contains(typeof(TestMapTypeThree)));
                    var targetMappings = m.SourceTypeMappings[typeof(TestMapTypeThree)].ToArray();

                    Assert.NotNull(targetMappings);
                    Assert.Single(targetMappings);

                    // ... Specific mapping strategies are not created and cached but are
                    // ... instantiated from the service collection.
                    var targetMapping = targetMappings.First();
                    Assert.Null(targetMapping.StrategyInstance);

                    Assert.True(targetMapping.SourceType == typeof(TestMapTypeThree));
                    Assert.True(targetMapping.TargetType == typeof(TestMapTypeOne));
                });
        });
    }

    [Fact]
    public void AllMappingStrategies_RegisteredAsServices()
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
                .Assert.ServiceCollection(c =>
                {
                    var registration = c.FirstOrDefault(s => 
                        s.ImplementationType == typeof(TestMappingStrategyOne));

                    Assert.NotNull(registration);
                    Assert.Equal(ServiceLifetime.Scoped, registration.Lifetime);
                });
        });    
    }

    [Fact]
    public void ObjectMapper_RegisteredAsService()
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
                    var registration = c.FirstOrDefault(s => s.ServiceType == typeof(IObjectMapper));

                    Assert.NotNull(registration);

                    Assert.Equal(typeof(ObjectMapper), registration.ImplementationType);
                    Assert.Equal(ServiceLifetime.Scoped, registration.Lifetime);
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

    public class TestMappingStrategyOne : MappingStrategy<TestMapTypeThree, TestMapTypeOne>
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
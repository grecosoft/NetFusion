using NetFusion.Services.Mapping;
using NetFusion.Services.Mapping.Plugin;
using NetFusion.Services.UnitTests.Mapping.Entities;
using NetFusion.Services.UnitTests.Mapping.Strategies;

namespace NetFusion.Services.UnitTests.Mapping;

public class ObjectMappingTests
{
    /// <summary>
    /// The ObjectMapper will first search for a mapping strategy for the
    /// source type where the specified target type has an exact match.
    /// matching
    /// </summary>
    [Fact]
    public void WhenMappingSourceType_ExactMatchingTargetType_StrategyUsed()
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
                .Act.OnService((IObjectMapper mapper) =>
                {
                    var testSrcObj = new TestMapTypeOne
                    {
                        Values = [30, 5, 88, 33, 83]
                    };
                    
                    return mapper.Map<TestMapTypeTwo>(testSrcObj);
                })
                .Assert.ServiceResult((TestMapTypeTwo result) =>
                {
                    Assert.NotNull(result);
                    Assert.Equal(5, result.Min);
                    Assert.Equal(88, result.Max);
                    Assert.Equal(239, result.Sum);
                });
        });
    }
    
    /// <summary>
    /// The ObjectMapper also supports mapping a source type to a derived target type.
    /// This allows for the case where the target type is specified for a given source
    /// type but the calling code does not need to know the exact derived type and can
    /// be generically written using the base type regardless of source type.
    /// </summary>
    [Fact]
    public void CanMapSourceTo_DerivedTarget()
    {
        var testPlugin = new MockHostPlugin();
        testPlugin.AddPluginType<TestDerivedStrategyFactory>();

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
                    var testSrcObjs = new object[]
                    {
                        new Customer("Mark", "Twain", 7),
                        new Car("VW", "GTI", "Silver", 2004)
                    };
                    
                    return testSrcObjs.Select(mapper.Map<Summary>).ToArray();
                })
                .Assert.ServiceResult((Summary[] results) =>
                {
                    results.Should().HaveCount(2);
                    
                    var customerSummary = results.OfType<CustomerSummary>().First();
                    var carSummary = results.OfType<CarSummary>().First();

                    customerSummary.Description.Should().Be("Mark-Twain");
                    carSummary.Description.Should().Be("VW-GTI");
                });
        });
    }

    /// <summary>
    /// A IMappingStrategy provided by a IStrategyFactory is used to map object types the same as one
    /// provided directly.  While the IMappingStrategy instances returned from a factory are cached
    /// and can't have services injected, they provide a compact form allowing multiple related mapping
    /// to be specified within one file without have to define specific strategy types.
    /// </summary>
    [Fact]
    public void CanMapSourceToTarget_WithFactorySpecifiedStrategy()
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
                .Act.OnService((IObjectMapper mapper) =>
                {
                    var testSrcObj = new TestMapTypeOne
                    {
                        Values = [30, 5, 88, 60, 65, 33, 83]
                    };
                    
                    return mapper.Map<TestMapTypeTwo>(testSrcObj);
                })
                .Assert.ServiceResult((TestMapTypeTwo result) =>
                {
                    result.Min.Should().Be(5);
                    result.Max.Should().Be(88);
                    result.Sum.Should().Be(364);
                });
        });
    }
}
﻿using Microsoft.Extensions.DependencyInjection;
using NetFusion.Core.TestFixtures.Container;
using NetFusion.Core.TestFixtures.Plugins;
using NetFusion.Services.Mapping;
using NetFusion.Services.Mapping.Plugin;
using NetFusion.Services.UnitTests.Mapping.Entities;

namespace NetFusion.Services.UnitTests.Mapping;

public class ObjectMappingTests
{
    [Fact]
    public void NoMappingStrategyFound_ExceptionIsRaised()
    {
        var testPlugin = new MockHostPlugin();

        var testSrcObj = new TestMapTypeOne
        {
            Values = new[] { 30, 5, 88, 33, 83 }
        };

        ContainerFixture.Test(fixture =>
        {
            var testResult = fixture.Arrange
                .Container(c =>
                {
                    c.RegisterPlugins(testPlugin);
                    c.RegisterPlugin<MappingPlugin>();
                })
                .Act.RecordException().OnServices(s =>
                {
                    var mapper = s.GetRequiredService<IObjectMapper>();
                    mapper.Map<TestInvalidMapType>(testSrcObj);
                });

            testResult.Assert.Exception<MappingException>(ex =>
            {
                Assert.Contains("Mapping strategy not found.", ex.Message);
            });
        });
    }
        
    // The ObjectMapper will first search for a mapping strategy based on the source type matching
    // the target type exactly.  If found, the mapping strategy is executed.
    [Fact]
    public void StrategyUsedTo_MapSourceToTarget()
    {
        var testPlugin = new MockHostPlugin();
        testPlugin.AddPluginType<TestMapStrategyOneToTwo>();
        testPlugin.AddPluginType<TestMapStrategyTwoToOne>();
            
        var testSrcObj = new TestMapTypeOne
        {
            Values = new[] { 30, 5, 88, 33, 83 }
        };
        TestMapTypeTwo? testTgtObj = null;

        ContainerFixture.Test(fixture =>
        {
            var testResult = fixture.Arrange
                .Container(c =>
                {
                    c.RegisterPlugins(testPlugin);
                    c.RegisterPlugin<MappingPlugin>();
                })
                .Act.OnServices(s =>
                {
                    var mapper = s.GetRequiredService<IObjectMapper>();
                    testTgtObj = mapper.Map<TestMapTypeTwo>(testSrcObj);
                });

            testResult.Assert.State(() =>
            {
                Assert.NotNull(testTgtObj);
                Assert.Equal(5, testTgtObj.Min);
                Assert.Equal(88, testTgtObj.Max);
                Assert.Equal(239, testTgtObj.Sum);
            });
        });
    }

    // When mapping from a target type to a source type, a mapping strategy where the
    // source type is the target type and the target type is the source type will be 
    // used.  If not found, a reverse lookup is completed.
    [Fact]
    public void StrategyUsedTo_MapTargetToSource()
    {
        var testPlugin = new MockHostPlugin();
        testPlugin.AddPluginType<TestMapStrategyOneToTwo>();
        testPlugin.AddPluginType<TestMapStrategyTwoToOne>();

        var testSrcObj = new TestMapTypeTwo
        {
            Min = 5,
            Max = 60,
            Sum = 65
        };
            
        TestMapTypeOne? testTgtObj = null;

        ContainerFixture.Test(fixture =>
        {
            var testResult = fixture.Arrange
                .Container(c =>
                {
                    c.RegisterPlugins(testPlugin);
                    c.RegisterPlugin<MappingPlugin>();
                })
                .Act.OnServices(s =>
                {
                    var mapper = s.GetRequiredService<IObjectMapper>();
                    testTgtObj = mapper.Map<TestMapTypeOne>(testSrcObj);
                });

            testResult.Assert.State(() =>
            {
                Assert.NotNull(testTgtObj);
                Assert.Equal(3, testTgtObj.Values.Length);
                Assert.Contains(5, testTgtObj.Values);
                Assert.Contains(60, testTgtObj.Values);
                Assert.Contains(65, testTgtObj.Values);
            });
        });
    }

    // The ObjectMapper also supports mapping a source type to a derived target type.
    // This allows for the case where the target type is specified for a given source
    // type but the calling code does not need to know the exact derived type and can
    // be generically written using the base type regardless of source type.
    [Fact(DisplayName = "Can map Source to Derived Target")]
    public void CanMapSourceTo_DerivedTarget()
    {
        var testPlugin = new MockHostPlugin();
        testPlugin.AddPluginType<TestDerivedStrategyFactory>();

        var testSrcObjs = new object[]
        {
            new Customer("Tom", "Green", 7),
            new Car("VW", "Passat", "Silver", 2004)
        };
            
        Summary[] testTgtObjs = Array.Empty<Summary>();

        ContainerFixture.Test(fixture =>
        {
            var testResult = fixture.Arrange
                .Container(c =>
                {
                    c.RegisterPlugins(testPlugin);
                    c.RegisterPlugin<MappingPlugin>();
                })
                .Act.OnServices(s =>
                {
                    var mapper = s.GetRequiredService<IObjectMapper>();
                    testTgtObjs = testSrcObjs.Select(src => mapper.Map<Summary>(src)).ToArray();
                });

            testResult.Assert.State(() =>
            {
                Assert.NotNull(testSrcObjs);
                Assert.Equal(2, testSrcObjs.Length);

                var custSummary = testTgtObjs.OfType<CustomerSummary>().First();
                var carSummary = testTgtObjs.OfType<CarSummary>().First();
                    
                Assert.Equal("Tom-Green", custSummary.Description);
                Assert.Equal("VW-Passat", carSummary.Description);
            });
        });
    }

    [Fact]
    public void CanMapSourceToTarget_WithFactorySpecifiedStrategy()
    {
        var testPlugin = new MockHostPlugin();
        testPlugin.AddPluginType<TestMappingStrategyFactory>();

        var testSrcObj = new TestMapTypeThree
        {
            MaxAllowedValue = 70,
            Values = new[] { 30, 5, 88, 60, 65, 33, 83 }
        };
            
        TestMapTypeTwo? testTgtObj = null;

        ContainerFixture.Test(fixture =>
        {
            var testResult = fixture.Arrange
                .Container(c =>
                {
                    c.RegisterPlugins(testPlugin);
                    c.RegisterPlugin<MappingPlugin>();
                })
                .Act.OnServices(s =>
                {
                    var mapper = s.GetRequiredService<IObjectMapper>();
                    testTgtObj = mapper.Map<TestMapTypeTwo>(testSrcObj);
                });

            testResult.Assert.State(() =>
            {
                Assert.NotNull(testTgtObj);
                Assert.Equal(5, testTgtObj.Min);
                Assert.Equal(65, testTgtObj.Max);
                Assert.Equal(193, testTgtObj.Sum);
            });
        });
    }
    
    public class TestMapStrategyTwoToOne : MappingStrategy<TestMapTypeTwo, TestMapTypeOne>
    {
        protected override TestMapTypeOne SourceToTarget(TestMapTypeTwo target) =>
            new()
            {
                Values = new[] { target.Min, target.Max, target.Sum }
            };
    }
        
    public class TestMapStrategyOneToTwo : MappingStrategy<TestMapTypeOne, TestMapTypeTwo>
    {
        protected override TestMapTypeTwo SourceToTarget(TestMapTypeOne source)
        {
            return new TestMapTypeTwo
            {
                Sum = source.Values.Sum(),
                Min = source.Values.Min(),
                Max = source.Values.Max()
            };
        }
    }
        
    public class TestDerivedStrategyFactory : IMappingStrategyFactory
    {
        public IEnumerable<IMappingStrategy> GetStrategies()
        {
            yield return DelegateMap.Map((Customer s) => new CustomerSummary
            {
                Description = $"{s.FirstName}-{s.LastName}",
                FirstName = s.FirstName,
                LastName = s.LastName
            });
                
            yield return DelegateMap.Map((Car s) =>
                
                new CarSummary {
                    Description = $"{s.Make}-{s.Model}",
                    Make = s.Make,
                    Model = s.Model
                });
        }
    }
        
    public class TestMappingStrategyFactory : IMappingStrategyFactory
    {
        public IEnumerable<IMappingStrategy> GetStrategies()
        {
            yield return DelegateMap.Map((TestMapTypeThree s) => new TestMapTypeTwo
            {
                Sum = s.Values.Where(v => v <= s.MaxAllowedValue).Sum(),
                Min =  s.Values.Where(v => v <= s.MaxAllowedValue).Min(),
                Max = s.Values.Where(v => v <= s.MaxAllowedValue).Max()
            });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Common.Extensions.Collections;
using NetFusion.Mapping;
using NetFusion.Test.Container;
using NetFusion.Test.Plugins;
using Xunit;

namespace CommonTests.Mapping
{
    public class ObjectMappingTests
    {
        // The ObjectMapper implementation will search the lookup registry for the mapping strategy that best
        // matches the target type that the source type is to be mapped.  If no matching strategy is found,
        // an exception is raised.
        [Fact]
        public void NoMappingStrategyFound_ExceptionIsRaised()
        {
            var typesUnderTest = new Type[] { };

            var testSrcObj = new TestMapTypeOne
            {
                Values = new[] { 30, 5, 88, 33, 83 }
            };
            
            TestInvalidMapType testTgtObj = null;

            ContainerFixture.Test(fixture =>
            {
                var testResult = fixture.Arrange
                    .Resolver(r => r.WithMappingConfiguredHost(typesUnderTest))
                    .Act.OnServices(s =>
                    {
                        var mapper = s.GetService<IObjectMapper>();
                        testTgtObj = mapper.Map<TestInvalidMapType>(testSrcObj);
                    });

                testResult.Assert.Exception<InvalidOperationException>(ex =>
                {
                    Assert.True(ex.Message.Contains("Mapping strategy not found."));
                });
            });
        }
        
        // The ObjectMapper will first search for a mapping strategy based on the source type matching
        // the target type exactly.  If found, the mapping strategy is executed.
        [Fact]
        public void StrategyUsedTo_MapSourceToTarget()
        {
            var typesUnderTest = new[] { typeof(TestMapStrategyOneToTwo) };

            var testSrcObj = new TestMapTypeOne
            {
                Values = new[] { 30, 5, 88, 33, 83 }
            };
            TestMapTypeTwo testTgtObj = null;

            ContainerFixture.Test(fixture =>
            {
                var testResult = fixture.Arrange
                    .Resolver(r => r.WithMappingConfiguredHost(typesUnderTest))
                    .Act.OnServices(s =>
                    {
                        var mapper = s.GetService<IObjectMapper>();
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
            var typesUnderTest = new[] { typeof(TestMapStrategyOneToTwo) };

            var testSrcObj = new TestMapTypeTwo
            {
                Min = 5,
                Max = 60,
                Sum = 65
            };
            
            TestMapTypeOne testTgtObj = null;

            ContainerFixture.Test(fixture =>
            {
                var testResult = fixture.Arrange
                    .Resolver(r => r.WithMappingConfiguredHost(typesUnderTest))
                    .Act.OnServices(s =>
                    {
                        var mapper = s.GetService<IObjectMapper>();
                        testTgtObj = mapper.Map<TestMapTypeOne>(testSrcObj);
                    });

                testResult.Assert.State(() =>
                {
                    Assert.NotNull(testTgtObj);
                    Assert.Equal(3, testTgtObj.Values.Length);
                    Assert.True(testTgtObj.Values.Contains(5));
                    Assert.True(testTgtObj.Values.Contains(60));
                    Assert.True(testTgtObj.Values.Contains(65));
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
            var typesUnderTest = new[] { typeof(TestDerivedStrategyFactory) };

            var testSrcObjs = new object[]
            {
                new Customer { FirstName = "Tom", LastName = "Green", Age = 7 },
                new Car { Make = "VW", Model = "Passat", Color = "Silver", Year = 2014 }
            };
            
            Summary[] testTgtObjs = null;

            ContainerFixture.Test(fixture =>
            {
                var testResult = fixture.Arrange
                    .Resolver(r => r.WithMappingConfiguredHost(typesUnderTest))
                    .Act.OnServices(s =>
                    {
                        var mapper = s.GetService<IObjectMapper>();
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
            var typesUnderTest = new[] { typeof(TestMappingStrategyFactory) };

            var testSrcObj = new TestMapTypeThree
            {
                MaxAllowedValue = 70,
                Values = new[] { 30, 5, 88, 60, 65, 33, 83 }
            };
            
            TestMapTypeTwo testTgtObj = null;

            ContainerFixture.Test(fixture =>
            {
                var testResult = fixture.Arrange
                    .Resolver(r => r.WithMappingConfiguredHost(typesUnderTest))
                    .Act.OnServices(s =>
                    {
                        var mapper = s.GetService<IObjectMapper>();
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
        
        public class TestMapTypeOne
        {
            public int[] Values { get; set; }
        }

        public class TestMapTypeTwo
        {
            public int Sum { get; set; }
            public int Max { get; set; }
            public int Min { get; set; }
        }

        public class TestInvalidMapType
        {
            
        }
        
        public class TestMapTypeThree
        {
            public int MaxAllowedValue { get; set; }
            public int[] Values { get; set; }
        }

        public class Car
        {
            public string Make { get; set; }
            public string Model { get; set; }
            public string Color { get; set; }
            public int Year { get; set; }
        }

        public class Customer
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public int Age { get; set; }
        }

        public abstract class Summary
        {
            public string Description { get; set; }
        }

        public class CarSummary : Summary
        {
            public string Make { get; set; }
            public string Model { get; set; }
        }

        public class CustomerSummary : Summary
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
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

            protected override TestMapTypeOne TargetToSource(TestMapTypeTwo target)
            {
                return new TestMapTypeOne
                {
                    Values = new int[] { target.Min, target.Max, target.Sum }
                };
            }
        }
        
        public class TestDerivedStrategyFactory : IMappingStrategyFactory
        {
            public IEnumerable<IMappingStrategy> GetStrategies()
            {
                yield return new MappingDelegate<Customer, CustomerSummary>(
                    source => new CustomerSummary {
                        Description = $"{source.FirstName}-{source.LastName}",
                        FirstName = source.FirstName,
                        LastName = source.LastName
                    });
                
                yield return new MappingDelegate<Car, CarSummary>(
                    source => new CarSummary {
                        Description = $"{source.Make}-{source.Model}",
                        Make = source.Make,
                        Model = source.Model
                    });
            }
        }
        
        public class TestMappingStrategyFactory : IMappingStrategyFactory
        {
            public IEnumerable<IMappingStrategy> GetStrategies()
            {
                yield return new MappingDelegate<TestMapTypeThree, TestMapTypeTwo>(
                    source => new TestMapTypeTwo {
                        Sum = source.Values.Where(v => v <= source.MaxAllowedValue).Sum(),
                        Min =  source.Values.Where(v => v <= source.MaxAllowedValue).Min(),
                        Max = source.Values.Where(v => v <= source.MaxAllowedValue).Max()
                    });
            }
        }

    }
    
    public static class TestFixtureExtensions
    {
        public static TestTypeResolver WithMappingConfiguredHost(this TestTypeResolver resolver, 
            params Type[] pluginTypes)
        {
            // Configure Core Plugin with messaging and the 
            // unit -of-work module.
            resolver.AddPlugin<MockCorePlugin>()
                .UseMappingPlugin();

            // Add host plugin with the plugin-types to be used
            // for the unit-test.
            resolver.AddPlugin<MockAppHostPlugin>()
                .AddPluginType(pluginTypes);

            return resolver;
        }
    }
}
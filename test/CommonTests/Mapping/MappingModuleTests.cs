using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NetFusion.Mapping;
using NetFusion.Mapping.Core;
using NetFusion.Mapping.Modules;
using NetFusion.Test.Modules;
using Xunit;

namespace CommonTests.Mapping
{
    public class MappingModuleTests
    {
        /// <summary>
        /// One or more IMappingStrategyFactory types can be defined.  When the MappingModule boostraps,
        /// it finds all the factories and executes their GetStrategies method to get the created mapping
        /// strategy instances.  These instances are cached and are stored within the Sevice Container.
        /// If a mapping strategy needs to inject a dependency, then a specific strategy should be defined
        /// which will be found and added to the service container (see next 2 unit tests);
        /// </summary>
        [Fact]
        public void AllMappingStrategyFactories_AreFound()
        {
            // Arrange:
            var module = ModuleTestFixture.SetupModule<MappingModule>(
                typeof(TestMappingStrategyFactory));
            
            // Act:
            module.Configure();
            
            // Assert:
            Assert.NotNull(module.StrategyFactories);
            Assert.Single(module.StrategyFactories);           
            Assert.Equal(1, module.SourceTypeMappings.Count);
            
            // .. Get the list of possible target types for the source type:
            Assert.True(module.SourceTypeMappings.Contains(typeof(TestMapTypeOne)));
            var targetMappings = module.SourceTypeMappings[typeof(TestMapTypeOne)].ToArray();
            
            Assert.NotNull(targetMappings);           
            Assert.Single(targetMappings);

            // ... Any strategies returned from a factory are cached:
            var targetMapping = targetMappings.First();
            Assert.NotNull(targetMapping.StrategyInstance);
            
            Assert.True(targetMapping.SourceType == typeof(TestMapTypeOne));
            Assert.True(targetMapping.TargetType == typeof(TestMapTypeTwo));
        }

        /// <summary>
        /// All IMappingStrategy types are found when the module bootstraps.  However, unlike the strategies
        /// returned from a factory, they are not created and cached.  The mapping strategy instance is created
        /// from the service collection when needed by the current request scope.
        /// </summary>
        [Fact]
        public void AllMappingStrategies_AreFound()
        {
            // Arrange:
            var module = ModuleTestFixture.SetupModule<MappingModule>(
                typeof(TestMappingStragegyOne));
            
            // Act:
            module.Configure();
            
            // Assert:
            Assert.Equal(1, module.SourceTypeMappings.Count);
            
            // .. Get the list of possible target types for the source type:
            Assert.True(module.SourceTypeMappings.Contains(typeof(TestMapTypeThree)));
            var targetMappings = module.SourceTypeMappings[typeof(TestMapTypeThree)].ToArray();
            
            Assert.NotNull(targetMappings);           
            Assert.Single(targetMappings);

            // ... Specific mapping strategies are not created and cached but are
            // ... instantiated from the service collection.
            var targetMapping = targetMappings.First();
            Assert.Null(targetMapping.StrategyInstance);
            
            Assert.True(targetMapping.SourceType == typeof(TestMapTypeThree));
            Assert.True(targetMapping.TargetType == typeof(TestMapTypeOne));
        }

        [Fact]
        public void AllMappingStrategies_RegisteredAsServices()
        {
            // Arrange:
            var module = ModuleTestFixture.SetupModule<MappingModule>(
                typeof(TestMappingStragegyOne));

            var services = new ServiceCollection();
            
            // Act:
            module.Configure();
            module.RegisterServices(services);
            
            // Assert:
            Assert.Single(services);
            Assert.Equal(ServiceLifetime.Scoped, services.First().Lifetime);
        }

        [Fact]
        public void ObjectMapper_RegisteredAsService()
        {
            // Arrange:
            var module = ModuleTestFixture.SetupModule<MappingModule>();
            var services = new ServiceCollection();
            
            // Act:
            module.RegisterDefaultServices(services);
            
            // Assert:
            Assert.Single(services);
            Assert.Equal(typeof(IObjectMapper), services.First().ServiceType);
            Assert.Equal(typeof(ObjectMapper), services.First().ImplementationType);
            Assert.Equal(ServiceLifetime.Scoped, services.First().Lifetime);
        }

        public class TestMappingStrategyFactory : IMappingStrategyFactory
        {
            public IEnumerable<IMappingStrategy> GetStrategies()
            {
                yield return new MappingDelegate<TestMapTypeOne, TestMapTypeTwo>(
                    source => new TestMapTypeTwo {
                        Sum = source.Values.Sum(),
                        Min =  source.Values.Min(),
                        Max = source.Values.Max()
                    });
            }
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

        public class TestMapTypeThree
        {
            public int MaxAllowedValue { get; set; }
            public int[] Values { get; set; }
        }

        public class TestMappingStragegyOne : MappingStrategy<TestMapTypeThree, TestMapTypeOne>
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
}
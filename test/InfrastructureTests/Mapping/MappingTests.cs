using FluentAssertions;
using NetFusion.Domain.Entities;
using NetFusion.Domain.Entities.Core;
using NetFusion.Domain.Patterns.Behaviors.Mapping;
using NetFusion.Mapping;
using NetFusion.Mapping.Core;
using System;
using UtilitiesTests.Mapping.Setup;
using Xunit;

namespace InfrastructureTests.Mapping
{
    public class MappingTests
    {
        // A behavior implementing the IMappingBehavior contract can be registered with the domain entity factory.
        // The behavior delegates to the IObjectMapper instance to map the domain entity to the specified target
        // type.  An object can also be mapped by injecting the IMappingBehavior into a component where it can be
        // directly referenced.  Having it configured as a domain entity behavior simplifies the calling code by
        // keeping the behavior closer to the domain entity being mapped and improves the readability of the code.
        [Fact(DisplayName = "Mapping behavior can be Supported by Domain Entity")]
        public void MappingBehaviorCanBe_SupportedByDomainEntity()
        {
            var targetMaps = new TargetMap[] { };

            var domainEntity = CreateTestEntity<SourceDomainEntity>(targetMaps);
            var behavior = domainEntity.Behaviors.Get<IMappingBehavior>();

            behavior.supported.Should().BeTrue();
            behavior.instance.Should().NotBeNull();
        }

        // The ObjectMapper implementation will search the lookup registry for the mapping strategy that best
        // matches the target type that the source type is to be mapped.  If no matching strategy is found,
        // an exception is raised.
        [Fact(DisplayName = "No Mappings Strategy found Exception raised")]
        public void NoMappingStrategyFound_ExceptionIsRaised()
        {
            var targetMaps = new TargetMap[] { };

            var domainEntity = CreateTestEntity<SourceDomainEntity>(targetMaps);

            domainEntity.ValueOne = "V1";
            domainEntity.ValueTwo = "V2";

            Assert.Throws<InvalidOperationException>(() => domainEntity.MapTo<TargetModel>())
                .Message.Should().Contain("Mapping strategy not found.");
        }

        // The ObjectMapper will first search for a mapping strategy based on the source type matching
        // the target type exactly.  If found, the mapping strategy is executed.
        [Fact(DisplayName = "Strategy used to Map Source to Target")]
        public void StrategyUsedTo_MapSourceToTarget()
        {
            var targetMaps = new TargetMap[] {
                new TargetMap
                {
                    SourceType = typeof(SourceDomainEntity),
                    TargetType = typeof(TargetModel),
                    StrategyType = typeof(ExampleMapStrategy)
                }
            };

            var domainEntity = CreateTestEntity<SourceDomainEntity>(targetMaps);

            domainEntity.ValueOne = "V1";
            domainEntity.ValueTwo = "V2";

            var model = domainEntity.MapTo<TargetModel>();
            Assert.Equal(model.ValueOne, domainEntity.ValueOne + "-SourceToTarget");
            Assert.Equal(model.ValueTwo, domainEntity.ValueTwo + "-SourceToTarget");
        }

        // When mapping from a target type to a source type, a mapping strategy where the
        // source type is the target type and the target type is the source type will be 
        // used.  If not found, a reverse lookup is completed.
        [Fact(DisplayName = "Strategy used to Map Target to Source")]
        public void StrategyUsedTo_MapTargetToSource()
        {
            var targetMaps = new TargetMap[] {
                new TargetMap
                {
                    SourceType = typeof(SourceDomainEntity),
                    TargetType = typeof(TargetModel),
                    StrategyType = typeof(ExampleMapStrategy)
                }
            };

            IObjectMapper mapper = ObjectMapperConfig.CreateObjectMapper(targetMaps);
            TargetModel model = new TargetModel
            {
                ValueOne = "MV1",
                ValueTwo = "MV2",
            };

            var entity = mapper.Map<SourceDomainEntity>(model);
            Assert.Equal(entity.ValueOne, model.ValueOne + "-TargetToSource");
            Assert.Equal(entity.ValueTwo, model.ValueTwo + "-TargetToSource");
        }

        // The ObjectMapper also supports mapping a source type to a derived target type.
        // This allows for the case where the target type is specified for a given source
        // type but the calling code does not need to know the exact derived type and can
        // be generically written using the base type regardless of source type.
        [Fact(DisplayName = "Can map Source to Derived Target")]
        public void CanMapSourceTo_DerivedTarget()
        {
            var targetMaps = new TargetMap[] {
                new TargetMap
                {
                    SourceType = typeof(SourceDomainEntity),
                    TargetType = typeof(TargetDerivedModel),
                    StrategyType = typeof(ExampleDerivedMapStrategy)
                }
            };

            var domainEntity = CreateTestEntity<SourceDomainEntity>(targetMaps);

            domainEntity.ValueOne = "V1";
            domainEntity.ValueTwo = "V2";

            var model = domainEntity.MapTo<TargetModel>();
            var derivedModel = model as TargetDerivedModel;

            derivedModel.Should().NotBeNull();
            Assert.Equal("V1V2", derivedModel.ValueThree);
        }

        // --------------------------MOCK DOMAIN ENTITIES----------------------------


        // Creates an source domain entity for testing with the mapping behavior registered.
        private SourceDomainEntity CreateTestEntity<T>(TargetMap[] targetMaps)
            where T : IBehaviorDelegator
        {
            var factory = new DomainEntityFactory(new MockResolver(targetMaps));

            factory.BehaviorsFor<T>(
                e => e.Add<IMappingBehavior, MappingBehavior>());

            return factory.Create<SourceDomainEntity>();
        }

        public class SourceDomainEntity : IBehaviorDelegator
        {
            public IBehaviorDelegatee Behaviors { get; private set; }

            void IBehaviorDelegator.SetDelegatee(IBehaviorDelegatee behaviors)
            {
                Behaviors = behaviors;
            }

            public string ValueOne { get; set; }
            public string ValueTwo { get; set; }
        }

        public class TargetModel
        {
            public string ValueOne { get; set; }
            public string ValueTwo { get; set; }
        }

        public class ExampleMapStrategy : MappingStrategy<SourceDomainEntity, TargetModel>
        {
            protected override TargetModel SourceToTarget(SourceDomainEntity source)
            {
                return new TargetModel
                {
                    ValueOne = source.ValueOne + "-SourceToTarget",
                    ValueTwo = source.ValueTwo + "-SourceToTarget"
                };
            }

            protected override SourceDomainEntity TargetToSource(TargetModel target)
            {
                return new SourceDomainEntity
                {
                    ValueOne = target.ValueOne + "-TargetToSource",
                    ValueTwo = target.ValueTwo + "-TargetToSource"
                };
            }
        }

        public class TargetDerivedModel : TargetModel
        {
            public string ValueThree { get; set; }
        }

        public class ExampleDerivedMapStrategy : MappingStrategy<SourceDomainEntity, TargetDerivedModel>
        {
            protected override TargetDerivedModel SourceToTarget(SourceDomainEntity source)
            {
                return new TargetDerivedModel
                {
                    ValueThree = source.ValueOne + source.ValueTwo
                };
            }
        }
    }

}

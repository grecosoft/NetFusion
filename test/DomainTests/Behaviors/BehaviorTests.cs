using DomainTests.Behaviors.Mocks;
using FluentAssertions;
using NetFusion.Domain.Entities.Core;
using Xunit;

namespace DomainTests.Behaviors
{
    public class BehaviorTests
    {
        /// <summary>
        /// Application code can query an entity to determine if it supports
        /// a specific behavior.
        /// </summary>
        [Fact (DisplayName = "Behaviors: Entity Can be Tested for Supported Behavior")]
        public void EntityCanBeTestedFor_SupportedBehavior()
        {
            var factory = new DomainEntityFactory(new MockResolver());
            factory.AddBehavior<IMockBehavior, MockBehaviorDefaultConstructor>();

            var aggregate = factory.Create<Aggregates>();
            aggregate.Behaviors.Supports<IMockBehavior>().Should().BeTrue();

            var behavior = aggregate.Behaviors.Get<IMockBehavior>();
            behavior.supported.Should().BeTrue();
            behavior.instance.Should().NotBeNull();
        }

        /// <summary>
        /// When a factory behavior is register with the factory and not for a 
        /// specific entity type, it applies to all created entities.  These
        /// type of behaviors should contain cross-cut type behaviors.
        /// </summary>
        [Fact ( DisplayName = "Behaviors: Factory Behavior Applies to All Created Entities")]
        public void FactoryBehavior_Applies_ToAllCreatedEntities()
        {
            var factory = new DomainEntityFactory(new MockResolver());
            factory.AddBehavior<IMockBehavior, MockBehaviorDefaultConstructor>();;

            var aggregate1 = factory.Create<Aggregates>();
            var aggregate2 = factory.Create<AggregateTwo>();

            aggregate1.Behaviors.Supports<IMockBehavior>().Should().BeTrue();
            aggregate2.Behaviors.Supports<IMockBehavior>().Should().BeTrue();
        }

        /// <summary>
        /// If a behavior is registered at the factory and entity levels, using the 
        /// same behavior contract, the create entity will have the implementation
        /// specified specifically for its class.
        /// </summary>
        [Fact (DisplayName = "Behaviors: Entity Specific Behavior Overrides Factory Behavior")]
        public void EntitySpecificBehavior_Overrides_FactoryBehavior()
        {
            var factory = new DomainEntityFactory(new MockResolver());
            factory.AddBehavior<IMockBehavior, MockBehaviorDefaultConstructor>();

            factory.BehaviorsFor<Aggregates>(entity => {
                entity.Add<IMockBehavior, MockBehaviorEntityConstructor>();
            });

            var aggregate1 = factory.Create<Aggregates>();
            var aggregate2 = factory.Create<AggregateTwo>();

            var behaviors1 = aggregate1.Behaviors.Get<IMockBehavior>();
            var behaviors2 = aggregate2.Behaviors.Get<IMockBehavior>();

            behaviors1.supported.Should().BeTrue();
            behaviors2.supported.Should().BeTrue();

            behaviors1.instance.Should().BeOfType<MockBehaviorEntityConstructor>();
            behaviors2.instance.Should().BeOfType<MockBehaviorDefaultConstructor>();
        }

        /// <summary>
        /// When application code requests an entity's behavior, it is created
        /// upon the first request and cached.
        /// </summary>
        [Fact (DisplayName = "Behaviors: Behavior Created Once Per Entity")]
        public void BehaviorCreated_Once_PerEntity()
        {
            var factory = new DomainEntityFactory(new MockResolver());
            factory.AddBehavior<IMockBehavior, MockBehaviorDefaultConstructor>(); 

            var aggregate1 = factory.Create<Aggregates>();
            var firstAccess = aggregate1.Behaviors.GetRequired<IMockBehavior>();
            var secondAccess = aggregate1.Behaviors.GetRequired<IMockBehavior>();

            firstAccess.Should().BeSameAs(secondAccess);
        }

        /// <summary>
        /// A behavior can inject container registered services by having
        /// public properties typed to the services being utilized.
        /// </summary>
        [Fact (DisplayName = "Behaviors: Behavior Service Properties are Resolved")]
        public void BehaviorServiceProperties_AreResolved()
        {
            var mockResolver = new MockResolver();
            var factory = new DomainEntityFactory(mockResolver);
            factory.AddBehavior<IMockBehavior, MockBehaviorDefaultConstructor>(); 

            factory.Create<Aggregates>().Behaviors.Get<IMockBehavior>();
            factory.Create<AggregateTwo>().Behaviors.Get<IMockBehavior>();

            mockResolver.ResolveCount.Should().Be(2);
        }
    }
}

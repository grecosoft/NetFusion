using DomainTests.Behaviors.Mocks;
using FluentAssertions;
using NetFusion.Domain.Entities.Core;
using System;
using Xunit;

namespace DomainTests.Behaviors
{
    public class BehaviorRegistrationTests
    {
        /// <summary>
        /// Behavior constructor can only have one constructor with a type assignable
        /// to IBehaviorDelegator.  If not, an exception is thrown.
        /// </summary>
        [Fact (DisplayName = "Behaviors: Behavior Cannot Have Constructors For Multiple Entities")]
        public void Behavior_CannotHave_Constructors_ForMultipleEntities()
        {
            var factory = new DomainEntityFactory(new MockResolver());

            Assert.Throws<InvalidOperationException>(() =>
            {
                factory.AddBehavior<IMockBehavior, MockInvalidBehavior>();
            }).Message.Contains("does not have a valid constructor");
        }

        /// <summary>
        /// If a behavior does not need access to its associated entity, 
        /// a default constructor can be defined.
        /// </summary>
        [Fact (DisplayName = "Behaviors: Behavior Can Have Default Constructor")]
        public void Behavior_CanHave_DefaultConstructor()
        {
            var factory = new DomainEntityFactory(new MockResolver());
            factory.AddBehavior<IMockBehavior, MockBehaviorDefaultConstructor>();

            var aggregate = factory.Create<Aggregates>();
            var behavior = aggregate.Behaviors.Get<IMockBehavior>();

            behavior.supported.Should().BeTrue();
            behavior.instance.IsDefaultConstructorUsed.Should().BeTrue();
        }

        /// <summary>
        /// If a behavior has needs access to its associated entity, it can define
        /// a constructor with the parameter of the exact entity type.
        /// </summary>
        [Fact (DisplayName = "Behaviors: Behavior Can Access Entity via Constructor.")]
        public void Behavior_CanAccess_EntityViaConstructor()
        {
            var factory = new DomainEntityFactory(new MockResolver());
            factory.AddBehavior<IMockBehavior, MockBehaviorEntityConstructor>();

            var aggregate = factory.Create<Aggregates>();
            var behavior = aggregate.Behaviors.Get<IMockBehavior>();

            behavior.supported.Should().BeTrue();
            behavior.instance.AssociatedAggregate.Should().NotBeNull();
        }

        /// <summary>
        /// If a behavior having the same behavior contract type is registered more than
        /// for a specific entity type, an exception is thrown.
        /// </summary>
        [Fact (DisplayName = "Behaviors: Behavior Can be Registered Only Once for Entity")]
        public void BehaviorCanBeRegisterd_OnlyOnce_ForEntity()
        {
            var factory = new DomainEntityFactory(new MockResolver());

            Assert.Throws<InvalidOperationException>(() =>
            {
                factory.BehaviorsFor<Aggregates>(behaviors => {
                    behaviors.Add<IMockBehavior, MockInvalidBehavior>();
                    behaviors.Add<IMockBehavior, MockInvalidBehavior>();
                });
            }).Message.Contains("is already registered");
        }

        /// <summary>
        /// If a behavior is registered at the factory level more than once,
        /// an exception is thrown.
        /// </summary>
        [Fact (DisplayName = "Behaviors: Behavior Can be Registered Only Once For Factory")]
        public void BehaviorCanBeRegistered_OnlyOnce_ForFactory()
        {
            var factory = new DomainEntityFactory(new MockResolver());

            Assert.Throws<InvalidOperationException>(() =>
            {
                factory.AddBehavior<IMockBehavior, MockInvalidBehavior>();
                factory.AddBehavior<IMockBehavior, MockInvalidBehavior>();
            }).Message.Contains("is already registered");
        }
    } 

}

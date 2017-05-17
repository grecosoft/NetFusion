using System;
using FluentAssertions;
using NetFusion.Domain.Entities;
using NetFusion.Domain.Entities.Core;
using Xunit;

namespace DomainTests
{
    /// <summary>
    /// Tests for domain entity behaviors.  Domain entity behaviors are simple classes that can be associated
    /// with a domain entity types and are used to encapsulate a set of related domain logic.  This helps
    /// the domain entity from becoming bloated overtime and business needs change.  Domain entities created
    /// using the IDomainEntityFactory automatically have their associated behaviors registered.  A behavior
    /// is created on first access and stored for the live of the domain entity.  The IDomainEntityFactory
    /// implementation will inject any properties corresponding to services stored in the dependency-injection
    /// container.  The injected services should be simple ones used to obtain information needed by the
    /// business logic.  Application services and services that make heave use of database class should not
    /// be injected.  Only services that return cached information should be used.
    /// </summary>
    public class DomainEntityBehaviorTests
    {
        [Fact (DisplayName = nameof(GlobalBehavior_AppliesToAllDomainEntities))]
        public void GlobalBehavior_AppliesToAllDomainEntities()
        {
            var factory = new DomainEntityFactory(new MockResolver());
            factory.AddBehavior<IMockDomainBehavior, MockGlobalDomainBehavior>();

            var domainEntity = factory.Create<MockDomainEntity>();
            var behavior = domainEntity.Entity.GetBehavior<IMockDomainBehavior>();

            behavior.Should().NotBeNull();
            behavior.supported.Should().BeTrue();
            behavior.instance.Should().NotBeNull();
            behavior.instance.Should().BeOfType<MockGlobalDomainBehavior>();
        }

        [Fact(DisplayName = nameof(DomainEntityBehavior_Overrides_GlobalBehavior))]
        public void DomainEntityBehavior_Overrides_GlobalBehavior()
        {
            var factory = new DomainEntityFactory(new MockResolver());
            factory.AddBehavior<IMockDomainBehavior, MockGlobalDomainBehavior>();

            // Register behavior that is specific to the domain entity type.
            factory.BehaviorsFor<MockDomainEntity>(
                entity => entity.Supports<IMockDomainBehavior, MockDomainBehavior>());

            var domainEntity = factory.Create<MockDomainEntity>();
            var behavior = domainEntity.Entity.GetBehavior<IMockDomainBehavior>();

            behavior.Should().NotBeNull();
            behavior.supported.Should().BeTrue();
            behavior.instance.Should().NotBeNull();
            behavior.instance.Should().BeOfType<MockDomainBehavior>();
        }

        [Fact(DisplayName = nameof(DomainEntity_MayNotSupportBehavior))]
        public void DomainEntity_MayNotSupportBehavior()
        {
            var factory = new DomainEntityFactory(new MockResolver());
           
            var domainEntity = factory.Create<MockDomainEntity>();
            var behavior = domainEntity.Entity.GetBehavior<IMockNotSupportedBehavior>();

            behavior.Should().NotBeNull();
            behavior.supported.Should().BeFalse();
            behavior.instance.Should().BeNull();
        }

        [Fact(DisplayName = nameof(BehaviorCreatedOnce_PerDomainEntity))]
        public void BehaviorCreatedOnce_PerDomainEntity()
        {
            var factory = new DomainEntityFactory(new MockResolver());
            factory.AddBehavior<IMockDomainBehavior, MockGlobalDomainBehavior>();

            var domainEntity = factory.Create<MockDomainEntity>();
            var behavior = domainEntity.Entity.GetBehavior<IMockDomainBehavior>();
            var behavior2 = domainEntity.Entity.GetBehavior<IMockDomainBehavior>();

            behavior.instance.Should().BeSameAs(behavior2.instance);

        }

        [Fact(DisplayName = nameof(BehaviorServices_AreResolved))]
        public void BehaviorServices_AreResolved()
        {
            var resolver = new MockResolver();
            var factory = new DomainEntityFactory(resolver);
            factory.AddBehavior<IMockDomainBehavior, MockGlobalDomainBehavior>();

            resolver.ResolveCount.Should().Be(0);
            var domainEntity = factory.Create<MockDomainEntity>();
            var behavior = domainEntity.Entity.GetBehavior<IMockDomainBehavior>();

            resolver.ResolveCount.Should().Be(1);
        }

        [Fact(DisplayName = nameof(DomainEntity_AssocatedWithBehavior))]
        public void DomainEntity_AssocatedWithBehavior()
        {
            var resolver = new MockResolver();
            var factory = new DomainEntityFactory(resolver);
            factory.AddBehavior<IMockDomainBehavior, MockGlobalDomainBehavior>();

            var domainEntity = factory.Create<MockDomainEntity>();
            var behavior = domainEntity.Entity.GetBehavior<IMockDomainBehavior>();
            domainEntity.Should().BeSameAs(behavior.instance.DomainEntity);
        }

        [Fact(DisplayName = nameof(DomainEntity_WithAccessor_AssocatedWithBehavior))]
        public void DomainEntity_WithAccessor_AssocatedWithBehavior()
        {

        }

        [Fact(DisplayName = nameof(ExceptionIf_EntityBehavior_RegisteredMoreThanOnce))]
        public void ExceptionIf_EntityBehavior_RegisteredMoreThanOnce()
        {

        }

        [Fact(DisplayName = nameof(ExceptionIf_GlobalBehavior_RegisteredMoreThanOnce))]
        public void ExceptionIf_GlobalBehavior_RegisteredMoreThanOnce()
        {

        }

        private class MockDomainEntity : IEntityDelegator
        {
            public IEntity Entity { get; private set; }

            public void SetEntity(IEntity entity)
            {
                this.Entity = entity;
            }
        }

        private interface IMockDomainBehavior : IDomainBehavior
        {
            MockDomainEntity DomainEntity { get; }
        }

        private interface IMockNotSupportedBehavior : IDomainBehavior
        {

        }

        private class MockGlobalDomainBehavior : IMockDomainBehavior
        {
            public MockDomainEntity DomainEntity { get; }

            public MockGlobalDomainBehavior(MockDomainEntity domainEntity)
            {
                this.DomainEntity = domainEntity;
            }
        }

        private class MockDomainBehavior : IMockDomainBehavior
        {

            public MockDomainEntity DomainEntity => throw new NotImplementedException();
        }

        private class MockNotSupportedDomainBehavior : IMockNotSupportedBehavior
        {

        }
    }
}

using FluentAssertions;
using NetFusion.Common.Extensions.Collection;
using NetFusion.Domain.Entities;
using NetFusion.Domain.Entities.Core;
using NetFusion.Domain.Entities.Registration;
using NetFusion.Utilities.Validation;
using NetFusion.Utilities.Validation.Behaviors;
using NetFusion.Utilities.Validation.Core;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace UtilitiesTests
{
    public class ValidationTests
    {
        [Fact (DisplayName = nameof(ValidationBehaviorCanBe_SupportedByDomainEntity))]
        public void ValidationBehaviorCanBe_SupportedByDomainEntity()
        {
            var domainEntity = CreateTestEntity<MockValidatableDomainEntity>();
            var behavior = domainEntity.Entity.GetBehavior<IValidationBehavior>();

            behavior.supported.Should().BeTrue();
            behavior.instance.Should().NotBeNull();
        }

        [Fact(DisplayName = nameof(DomainEntityValidated_UsingMsValidationAttributes_ByDefault))]
        public void DomainEntityValidated_UsingMsValidationAttributes_ByDefault()
        {
            var domainEntity = CreateTestEntity<MockValidatableDomainEntity>();
            domainEntity.ValueOne = new string('X', 50);
            domainEntity.ValueTwo = 200;

            var valResult = domainEntity.Validate();
            valResult.ValidationType.Should().Be(ValidationTypes.Error);

            // There should be one domain entity validation with two validations items
            // the above two specified properties are out of range.
            valResult.ObjectValidations.Should().HaveCount(1);
            var entityValidation = valResult.ObjectValidations.First();

            entityValidation.Should().NotBeNull();
            entityValidation.Validations.Should().HaveCount(2);

            // The entity validation has reference to invalid domain entity.
            entityValidation.Object.Should().BeSameAs(domainEntity);

            // Validate the messages for each validation message.
            entityValidation.Validations[0].Message.Should().Be("ValueOneValidationMessage");
            entityValidation.Validations[1].Message.Should().Be("ValueTwoValidationMessage");

            // The validation result has reference to the root domain entity that was validated.
            valResult.RootObject.Should().BeSameAs(domainEntity);
        }

        [Fact(DisplayName = nameof(ValidationBehaviorNotSupported_NotSpecifiedResult))]
        public void ValidationBehaviorNotSupported_NotSpecifiedResult()
        {
            var domainEntity = CreateTestEntity<MockDomainEntity>();
            var valResult = domainEntity.Validate();

            valResult.ValidationType.Should().Be(ValidationTypes.NotSpecified);
            valResult.RootObject.Should().BeSameAs(domainEntity);
            valResult.ObjectValidations.Should().BeEmpty();
        }

        [Fact(DisplayName = nameof(DomainEntityCalled_DuringValidation))]
        public void DomainEntityCalled_DuringValidation()
        {
            var domainEntity = CreateTestEntity<MockValidatableDomainEntity>();
            domainEntity.ValueThree = 200;

            var valResult = domainEntity.Validate();

            valResult.ValidationType.Should().Be(ValidationTypes.Error);
            valResult.ObjectValidations.First()
                .Validations.First()
                .Message.Should().Be("ValueThreeValidationMessage");
        }

        [Fact(DisplayName = nameof(IfValidatorDetermines_EntityNotValid_EntityValidationMethodNotInvoked))]
        public void IfValidatorDetermines_EntityNotValid_EntityValidationMethodNotInvoked()
        {
            var domainEntity = CreateTestEntity<MockValidatableDomainEntity>();
            domainEntity.ValueOne = new string('X', 50);
            domainEntity.ValueTwo = 200;

            var valResult = domainEntity.Validate();
            valResult.ValidationType.Should().Be(ValidationTypes.Error);
            domainEntity.EntityValidateInvoked.Should().BeFalse();
        }

        [Fact(DisplayName = nameof(DomainEntityCan_ValidateChildDomainEntity))]
        public void DomainEntityCan_ValidateChildDomainEntity()
        {
            var domainEntity = CreateTestEntity<MockValidatableDomainEntity>();
            var child = new MockChildDomainEntity();

            child.ValueFour = 5;
            domainEntity.SetChild(child);

            var valResults = domainEntity.Validate();
            valResults.ValidationType.Should().Be(ValidationTypes.Error);

            // The validation result aggregate should be the domain entity that was validated.
            valResults.RootObject.Should().BeSameAs(domainEntity);

            // Assert the child object validations.
            var childObjVal = valResults.ObjectValidations.SingleOrDefault();
            childObjVal.Should().NotBeNull();
            childObjVal.Object.Should().BeSameAs(child);

            // Assert the single child entity validation.
            var valItem = childObjVal.Validations.SingleOrDefault();
            valItem.Should().NotBeNull();
            valItem.Message.Should().Be("ValueFourValidationMessage");
        }

        [Fact(DisplayName = nameof(DomainEntityCan_ValidateChildDomainEntities))]
        public void DomainEntityCan_ValidateChildDomainEntities()
        {
            var domainEntity = CreateTestEntity<MockValidatableDomainEntity>();

            var children = new MockChildDomainEntity[] {
                new MockChildDomainEntity { ValueFour = 5 },
                new MockChildDomainEntity { ValueFour = 8 }
            };

            domainEntity.SetChildren(children);

            var valResults = domainEntity.Validate();
            valResults.ValidationType.Should().Be(ValidationTypes.Error);

            // The validation result aggregate should be the domain entity that was validated.
            valResults.RootObject.Should().BeSameAs(domainEntity);

            // Assert the child object validations.
            valResults.ObjectValidations.Should().HaveCount(2);
        }

        [Fact(DisplayName = nameof(ResultValidationLevel_IsMaxValidationLevel_ForAllInvalidations))]
        public void ResultValidationLevel_IsMaxValidationLevel_ForAllInvalidations()
        {
            var domainEntity = CreateTestEntity<MockValidatableDomainEntity>();
            var child = new MockChildDomainEntity();

            domainEntity.ValueInfoOne = 33;
            child.ValueWarningTwo = 77;
            domainEntity.SetChild(child);

            var valResult = domainEntity.Validate();
            valResult.ValidationType.Should().Be(ValidationTypes.Warning);
        }

        [Fact(DisplayName = nameof(ResultIsFlatenedList_OfAllValidatedEntities))]
        public void ResultIsFlatenedList_OfAllValidatedEntities()
        {
            var domainEntity = CreateTestEntity<MockValidatableDomainEntity>();
            var child = new MockChildDomainEntity();

            domainEntity.ValueInfoOne = 33;
            child.ValueWarningTwo = 77;
            domainEntity.SetChild(child);

            var valResult = domainEntity.Validate();
            valResult.ObjectValidations.SelectMany(ov => ov.Validations)
                .Should().HaveCount(2)
                .And.Contain(v => v.ValidationType == ValidationTypes.Info)
                .And.Contain(v => v.ValidationType == ValidationTypes.Warning);
        }

        [Fact(DisplayName = nameof(CanThrowExceptionFor_InvalidResult))]
        public void CanThrowExceptionFor_InvalidResult()
        {
            var domainEntity = CreateTestEntity<MockValidatableDomainEntity>();
            domainEntity.ValueOne = new string('X', 50);

            var valResult = domainEntity.Validate();

            Assert.ThrowsAny<ValidationResultException>(() => valResult.ThrowIfInvalid());
        }

        // --------------------------MOCK VALIDATION ENTITIES----------------------------

        private MockValidatableDomainEntity CreateTestEntity<T>() where T : IEntityDelegator
        {
            var factory = new DomainEntityFactory(new MockResolver());

            factory.BehaviorsFor<T>(
                e => e.Supports<IValidationBehavior, MockValidationBehavior>());

            return factory.Create<MockValidatableDomainEntity>();
        }

        private class MockValidatableDomainEntity : IEntityDelegator,
            IValidatableType
        {
            public IEntity Entity { get; private set; }
            private MockChildDomainEntity Child { get; set; }
            private MockChildDomainEntity[] Children { get; set; }

            [StringLength(20, MinimumLength = 10, ErrorMessage = "ValueOneValidationMessage")]
            public string ValueOne { get; set; } = new string('X', 15);

            [Range(50, 100, ErrorMessage = "ValueTwoValidationMessage")]
            public int ValueTwo { get; set; } = 75;

            public int ValueThree { get; set; } = 501;

            public int ValueInfoOne { get; set; } = 1000;

            public bool EntityValidateInvoked { get; set; } = false;

            public void SetEntity(IEntity entity)
            {
                this.Entity = entity;
            }

            public void SetChild(MockChildDomainEntity child)
            {
                this.Child = child;
            }

            public void SetChildren(MockChildDomainEntity[] children)
            {
                this.Children = children;
            }

            public void Validate(IObjectValidator validator)
            {
                validator.Validate(ValueThree > 500, "ValueThreeValidationMessage", ValidationTypes.Error);
                this.EntityValidateInvoked = true;

                if (this.Child != null)
                {
                    validator.AddChildValidator(this.Child);
                } 

                if (this.Children != null)
                {
                    Children.ForEach(c => validator.AddChildValidator(c));
                }

                validator.Validate(ValueInfoOne == 1000,
                   "ValueWarningOne", ValidationTypes.Info);
            }
        }

        private class MockChildDomainEntity : IValidatableType
        {
            [Range(95, 100, ErrorMessage = "ValueFourValidationMessage")]
            public int ValueFour { get; set; } = 98;

            public int ValueWarningTwo { get; set; } = 2000;

            public void Validate(IObjectValidator validator)
            {
                validator.Validate(ValueWarningTwo == 2000, 
                    "ValueWarningTwoMessage", ValidationTypes.Warning);
            }
        }

        private class MockDomainEntity : IEntityDelegator
        {
            public IEntity Entity { get; private set; }

            public void SetEntity(IEntity entity)
            {
                this.Entity = entity;
            }
        }

        private class MockResolver : IDomainServiceResolver
        {
            public void ResolveDomainServices(IDomainBehavior domainBehavior)
            {
              
            }
        }

        // This simulates what happens during the bootstrap process.  The host application can
        // specify using a container configuration the implementation of IObjectValidator should
        // be used.  The below returns the default object-validator instance.
        private class MockValidationBehavior : ValidationBehavior
        {
            public MockValidationBehavior(IEntityDelegator entity) : base(entity)
            {
                this.ValidationModule = new MockValidationModule();
            }
        }

        private class MockValidationModule : IValidationModule
        {
            public IObjectValidator CreateValidator(object obj)
            {
                return new ObjectValidator(obj);
            }
        }
    }
}

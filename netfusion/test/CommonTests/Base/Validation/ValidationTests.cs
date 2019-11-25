using FluentAssertions;
using NetFusion.Base.Validation;
using NetFusion.Common.Extensions.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace CommonTests.Base.Validation
{
    public class ValidationTests
    {
        [Fact(DisplayName = nameof(DomainEntityValidated_UsingMsValidationAttributes_ByDefault))]
        public void DomainEntityValidated_UsingMsValidationAttributes_ByDefault()
        {
            var domainEntity = new MockValidatableDomainEntity
            {
                ValueOne = new string('X', 50),
                ValueTwo = 200
            };

            var valResult = Validate(domainEntity);
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

        [Fact(DisplayName = nameof(DomainEntityCalled_DuringValidation))]
        public void DomainEntityCalled_DuringValidation()
        {
            var domainEntity = new MockValidatableDomainEntity
            {
                ValueThree = 200
            };

            var valResult = Validate(domainEntity);

            valResult.ValidationType.Should().Be(ValidationTypes.Error);
            valResult.ObjectValidations.First()
                .Validations.First()
                .Message.Should().Be("ValueThreeValidationMessage");
        }

        [Fact(DisplayName = nameof(IfValidatorDetermines_EntityNotValid_EntityValidationMethodNotInvoked))]
        public void IfValidatorDetermines_EntityNotValid_EntityValidationMethodNotInvoked()
        {
            var domainEntity = new MockValidatableDomainEntity
            {
                ValueOne = new string('X', 50),
                ValueTwo = 200
            };

            var valResult = Validate(domainEntity);
            valResult.ValidationType.Should().Be(ValidationTypes.Error);
            domainEntity.EntityValidateInvoked.Should().BeFalse();
        }

        [Fact(DisplayName = nameof(DomainEntityCan_ValidateChildDomainEntity))]
        public void DomainEntityCan_ValidateChildDomainEntity()
        {
            var domainEntity = new MockValidatableDomainEntity();
            var child = new MockChildDomainEntity
            {
                ValueFour = 5
            };

            domainEntity.SetChild(child);

            var valResults = Validate(domainEntity);
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
            var domainEntity = new MockValidatableDomainEntity();

            var children = new[] {
                new MockChildDomainEntity { ValueFour = 5 },
                new MockChildDomainEntity { ValueFour = 8 }
            };

            domainEntity.SetChildren(children);

            var valResults = Validate(domainEntity);
            valResults.ValidationType.Should().Be(ValidationTypes.Error);

            // The validation result aggregate should be the domain entity that was validated.
            valResults.RootObject.Should().BeSameAs(domainEntity);

            // Assert the child object validations.
            valResults.ObjectValidations.Should().HaveCount(2);
        }

        [Fact(DisplayName = nameof(ResultValidationLevel_IsMaxValidationLevel_ForAllInvalidations))]
        public void ResultValidationLevel_IsMaxValidationLevel_ForAllInvalidations()
        {
            var domainEntity = new MockValidatableDomainEntity();
            var child = new MockChildDomainEntity();

            domainEntity.ValueInfoOne = 33;
            child.ValueWarningTwo = 77;
            domainEntity.SetChild(child);

            var valResult = Validate(domainEntity);
            valResult.ValidationType.Should().Be(ValidationTypes.Warning);
        }

        [Fact(DisplayName = nameof(ResultIsFlatenedList_OfAllValidatedEntities))]
        public void ResultIsFlatenedList_OfAllValidatedEntities()
        {
            var domainEntity = new MockValidatableDomainEntity();
            var child = new MockChildDomainEntity();

            domainEntity.ValueInfoOne = 33;
            child.ValueWarningTwo = 77;
            domainEntity.SetChild(child);

            var valResult = Validate(domainEntity);
            valResult.ObjectValidations.SelectMany(ov => ov.Validations)
                .Should().HaveCount(2)
                .And.Contain(v => v.ValidationType == ValidationTypes.Info)
                .And.Contain(v => v.ValidationType == ValidationTypes.Warning);
        }

        [Fact(DisplayName = nameof(CanThrowExceptionFor_InvalidResult))]
        public void CanThrowExceptionFor_InvalidResult()
        {
            var domainEntity = new MockValidatableDomainEntity
            {
                ValueOne = new string('X', 50)
            };

            var valResult = Validate(domainEntity);
            Assert.ThrowsAny<ValidationResultException>(() => valResult.ThrowIfInvalid());
        }

        private ValidationResultSet Validate(object obj)
        {
            var validator = new ObjectValidator(obj);
            return validator.Validate();
        }

        // --------------------------MOCK VALIDATION ENTITIES----------------------------

        private class MockValidatableDomainEntity : IValidatableType
        {
            private MockChildDomainEntity Child { get; set; }
            private MockChildDomainEntity[] Children { get; set; }

            [StringLength(20, MinimumLength = 10, ErrorMessage = "ValueOneValidationMessage")]
            public string ValueOne { get; set; } = new string('X', 15);

            [Range(50, 100, ErrorMessage = "ValueTwoValidationMessage")]
            public int ValueTwo { get; set; } = 75;

            public int ValueThree { get; set; } = 501;

            public int ValueInfoOne { get; set; } = 1000;

            public bool EntityValidateInvoked { get; private set; }

            public void SetChild(MockChildDomainEntity child)
            {
                Child = child;
            }

            public void SetChildren(MockChildDomainEntity[] children)
            {
                Children = children;
            }

            public void Validate(IObjectValidator validator)
            {
                validator.Verify(ValueThree > 500, "ValueThreeValidationMessage");
                EntityValidateInvoked = true;

                if (Child != null)
                {
                    validator.AddChild(Child);
                }

                Children?.ForEach(c => validator.AddChild(c));

                validator.Verify(ValueInfoOne == 1000,
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
                validator.Verify(ValueWarningTwo == 2000,
                    "ValueWarningTwoMessage", ValidationTypes.Warning);
            }
        }
    }
}

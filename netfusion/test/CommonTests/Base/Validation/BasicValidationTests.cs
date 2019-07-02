using System.ComponentModel.DataAnnotations;
using System.Linq;
using NetFusion.Base.Validation;
using Xunit;

namespace CommonTests.Base.Validation
{
    public class BasicValidationTests
    {
        [Fact]
        public void ValidationAttributes_NotValid_ValidationMethodNotCalled()
        {
            var testObj = new ParentValidateType {PropWithAttribValidation = 500};

            var validator = new DataAnnotationsValidator(testObj);
            validator.Validate();
            
            Assert.False(testObj.WasValidationMethodInvoked);
        }

        [Fact]
        public void ValidationAttributes_Valid_ValidationMethodCalled()
        {
            var testObj = new ParentValidateType {PropWithAttribValidation = 50};

            var validator = new DataAnnotationsValidator(testObj);
            validator.Validate();
            
            Assert.True(testObj.WasValidationMethodInvoked);
        }

        [Fact]
        public void ValidationResultSet_Populated()
        {
            var testObj = new ParentValidateType {PropWithAttribValidation = 500};

            var validator = new DataAnnotationsValidator(testObj);
            var resultSet = validator.Validate();

            Assert.NotNull(resultSet.ObjectValidations);
            Assert.Single(resultSet.ObjectValidations);
            Assert.Equal(testObj, resultSet.RootObject);

            var objValItem = resultSet.ObjectValidations.First();
            Assert.Equal(testObj, objValItem.Object);
            
            Assert.NotNull(objValItem.Validations);
            Assert.Single(objValItem.Validations);

            var valItem = objValItem.Validations.First();
            
            Assert.Equal("Value out of range.", valItem.Message);
        }

        [Fact]
        public void CanAddValidation_BasedOnPredicate()
        {
            var testObj = new ParentValidateType
            {
                PropWithAttribValidation = 13, 
                AddPredicateValidation = true
            };

            var validator = new DataAnnotationsValidator(testObj);
            var resultSet = validator.Validate();
            
            Assert.True(resultSet.IsInvalid);
            
            var allValidations = resultSet.ObjectValidations
                .SelectMany(ov => ov.Validations)
                .ToArray();

            Assert.Single(allValidations);

            var valItem = allValidations.First();
            Assert.Equal("Value can't be 13.", valItem.Message);
        }

        [Fact]
        public void CanEnlistChildOjects_InResultSet_ToBeValidated()
        {
            var testObj = new ParentValidateType
            {
                PropWithAttribValidation = 50,
                EnlistChildObjectOne = new ChildValidateType
                {
                    AddPredicateValidation = true,
                    PropWithAttribValidation = 1013
                }
            };

            var validator = new DataAnnotationsValidator(testObj);
            var resultSet = validator.Validate();

            Assert.True(resultSet.IsInvalid);
            
            Assert.True(testObj.EnlistChildObjectOne.WasValidationMethodInvoked);
            
            var allValidations = resultSet.ObjectValidations
                .SelectMany(ov => ov.Validations)
                .ToArray();

            Assert.Single(allValidations);

            var valItem = allValidations.First();
            Assert.Equal("Value can't be 1013.", valItem.Message);
        }

        [Fact]
        public void RootObjectSet_ForValidationResutSet()
        {
            var testObj = new ParentValidateType
            {
                PropWithAttribValidation = 50,
                EnlistChildObjectOne = new ChildValidateType
                {
                    AddPredicateValidation = true,
                    PropWithAttribValidation = 1013
                }
            };

            var validator = new DataAnnotationsValidator(testObj);
            var resultSet = validator.Validate();
            
            Assert.Equal(testObj, resultSet.RootObject);
        }

        [Fact]
        public void ValidationSet_Reports_MaxValidation()
        {
            var testObj = new ParentValidateType
            {
                PropWithAttribValidation = 50,
                EnlistChildObjectOne = new ChildValidateType
                {
                    AddPredicateValidation = true,
                    PropWithAttribValidation = 1013,
                    PredicateValidationType = ValidationTypes.Info
                },
                EnlistChildObjectTwo = new ChildValidateType
                {
                    AddPredicateValidation = true,
                    PropWithAttribValidation = 1013,
                    PredicateValidationType = ValidationTypes.Warning
                }
            };

            var validator = new DataAnnotationsValidator(testObj);
            var resultSet = validator.Validate();
            
            Assert.Equal(ValidationTypes.Warning, resultSet.ValidationType);
        }

        [Fact]
        public void ValidationSet_OnlyErrors_ConsideredInvalid()
        {
            var testObj = new ParentValidateType
            {
                PropWithAttribValidation = 50,
                EnlistChildObjectOne = new ChildValidateType
                {
                    AddPredicateValidation = true,
                    PropWithAttribValidation = 1013,
                    PredicateValidationType = ValidationTypes.Info
                },
                EnlistChildObjectTwo = new ChildValidateType
                {
                    AddPredicateValidation = true,
                    PropWithAttribValidation = 1013,
                    PredicateValidationType = ValidationTypes.Warning
                }
            };

            var validator = new DataAnnotationsValidator(testObj);
            var resultSet = validator.Validate();
            
            Assert.True(resultSet.IsValid);
            Assert.False(resultSet.IsInvalid);
        }

        [Fact]
        public void ValidationSet_CanBeQueried_ForValidationsOfType()
        {
            var testObj = new ParentValidateType
            {
                PropWithAttribValidation = 50,
                EnlistChildObjectOne = new ChildValidateType
                {
                    AddPredicateValidation = true,
                    PropWithAttribValidation = 1013,
                    PredicateValidationType = ValidationTypes.Info
                },
                EnlistChildObjectTwo = new ChildValidateType
                {
                    AddPredicateValidation = true,
                    PropWithAttribValidation = 1013,
                    PredicateValidationType = ValidationTypes.Warning
                }
            };

            var validator = new DataAnnotationsValidator(testObj);
            var resultSet = validator.Validate();
            
            Assert.Single(resultSet.GetValidationsOfType(ValidationTypes.Info));
            Assert.Single(resultSet.GetValidationsOfType(ValidationTypes.Warning));
        }

        [Fact]
        public void ValidationSet_CanThrowException_IfInvalid()
        {
            var testObj = new ParentValidateType {PropWithAttribValidation = 500};

            var validator = new DataAnnotationsValidator(testObj);
            var resultSet = validator.Validate();

            Assert.Throws<ValidationResultException>(() => resultSet.ThrowIfInvalid());
        }

        private class ParentValidateType : IValidatableType
        {           
            [Range(5, 100, ErrorMessage = "Value out of range.")]
            public int PropWithAttribValidation { get; set; } = 5;
            
            public bool WasValidationMethodInvoked { get; private set; }
            public bool AddPredicateValidation { private get; set; }
            public ChildValidateType EnlistChildObjectOne { get; set; }
            public ChildValidateType EnlistChildObjectTwo { get; set; }
            
            public void Validate(IObjectValidator validator)
            {
                WasValidationMethodInvoked = true;

                if (AddPredicateValidation)
                {
                    validator.Verify(PropWithAttribValidation != 13, "Value can't be 13.");
                }

                if (validator.IsValid && EnlistChildObjectOne != null)
                {
                    validator.AddChild(EnlistChildObjectOne);
                }

                if (validator.IsValid && EnlistChildObjectTwo != null)
                {
                    validator.AddChild(EnlistChildObjectTwo);
                }
            }
        }

        private class ChildValidateType : IValidatableType
        {
            public bool WasValidationMethodInvoked { get; private set; }
            public bool AddPredicateValidation { private get; set; }
            public ValidationTypes PredicateValidationType = ValidationTypes.Error;

            [Range(1000, 2000, ErrorMessage = "Child Validation Message.")]
            public int PropWithAttribValidation { get; set; } = 1500;

            public void Validate(IObjectValidator validator)
            {
                WasValidationMethodInvoked = true;
                
                if (AddPredicateValidation)
                {
                    validator.Verify(PropWithAttribValidation != 1013, "Value can't be 1013.", PredicateValidationType);
                }
            }
        }
    }
}
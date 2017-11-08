//using NetFusion.Utilities.Validation.Results;
//using System.Collections.Generic;

//namespace NetFusion.Utilities.Validation
//{
//    /// <summary>
//    /// Object responsible for validating an object using a specific validation library.
//    /// </summary>
//    public interface IObjectValidator
//    {
//        /// <summary>
//        /// The validated object.
//        /// </summary>
//        object Object { get; }

//        /// <summary>
//        /// Indicates if the object is valid.  If any validation is the level Error, the object is consider invalid.
//        /// </summary>
//        bool IsValid { get; }

//        /// <summary>
//        /// The list of validation items associated with the object.
//        /// </summary>
//        IEnumerable<ValidationItem> Validations { get; }

//        /// <summary>
//        /// The children object validators associated with the object.
//        /// </summary>
//        IEnumerable<IObjectValidator> Children { get; }

//        /// <summary>
//        /// Add and returns a new validator for a child object.
//        /// </summary>
//        /// <param name="childObject">The child object to validate.</param>
//        /// <returns>The created validator that can have additional validations added.
//        /// </returns>
//        IObjectValidator AddChildValidator(object childObject);

//        /// <summary>
//        /// Asserts that the predicate is true.  If not true, a validation item is added
//        /// to the list.
//        /// </summary>
//        /// <param name="predicate">The expression to assert.</param>
//        /// <param name="message">The associated message for the assertion.</param>
//        /// <param name="level">The level of the associated message.</param>
//        /// <returns></returns>
//        bool Validate(bool predicate, string message, ValidationTypes level = ValidationTypes.Error);

//    }

//}

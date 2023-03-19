using System.Collections.Generic;

namespace NetFusion.Common.Base.Validation;

/// <summary>
/// Object responsible for validating an object using a specific validation library
/// or the simple implementation provided by NetFusion.
/// </summary>
public interface IObjectValidator
{
    /// <summary>
    /// The validated object.
    /// </summary>
    object Object { get; }

    /// <summary>
    /// Indicates if the object is valid.  If any validations are the level Error, 
    /// the object is consider invalid.
    /// </summary>
    bool IsValid { get; }

    /// <summary>
    /// The list of validation items associated with the object.
    /// </summary>
    IEnumerable<ValidationItem> Validations { get; }

    /// <summary>
    /// The children object validators associated with the object.
    /// </summary>
    IEnumerable<IObjectValidator> Children { get; }

    /// <summary>
    /// Validates the object and all if its children and returns a flattened
    /// list all objects with validations.
    /// </summary>
    /// <returns>Result set containing all objects with invalidations.</returns>
    ValidationResultSet Validate();

    /// <summary>
    /// Add and returns a new validator for a child object.
    /// </summary>
    /// <param name="childObject">The child object to validate.</param>
    /// <returns>The created validator that can have additional validations added.</returns>
    IObjectValidator AddChild(object childObject);

    /// <summary>
    /// Adds new validators for a list of child objects
    /// </summary>
    /// <param name="childObjects">Object to be validated.</param>
    void AddChildren(IEnumerable<object> childObjects);
        
    /// <summary>
    /// Adds new validators for a list of child objects.
    /// </summary>
    /// <param name="childObjects">Object(s) to be validated.</param>
    void AddChildren(params object[] childObjects);

    /// <summary>
    /// Verifies that the predicate is true.  If not true, a validation item is added to the list.
    /// </summary>
    /// <param name="predicate">The expression to assert.</param>
    /// <param name="message">The associated message for the assertion.</param>
    /// <param name="level">The level of the associated message.</param>
    /// <param name="propertyNames">The name of the properties.</param>
    /// <returns>The result of the predicate.</returns>
    bool Verify(bool predicate, string message, ValidationTypes level = ValidationTypes.Error, params string[] propertyNames);
}
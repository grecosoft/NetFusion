using System;
using System.Collections.Generic;
using System.Linq;
using NetFusion.Common.Extensions.Collections;

namespace NetFusion.Common.Base.Validation;

/// <summary>
/// Contains the result of validating an object.  The result contains a list of flattened
/// object validations.  Each object validation has the list of associated validation items.
/// </summary>
public class ValidationResultSet
{
    /// <summary>
    /// The root object that was validated.
    /// </summary>
    public object RootObject { get; private set; }

    /// <summary>
    /// List containing an entry for each object that resulted in validations.
    /// </summary>
    public ObjectValidation[] ObjectValidations { get; private set; } = Array.Empty<ObjectValidation>();

    /// <summary>
    /// The maximum validation level for all associated validations.
    /// </summary>
    public ValidationTypes ValidationType { get; private set; }

    /// <summary>
    /// Indicates that there is at least one object validations with an error validation item.
    /// </summary>
    public bool IsInvalid => ValidationType == ValidationTypes.Error;

    /// <summary>
    /// Indicates there are no objects with validation error items.
    /// </summary>
    public bool IsValid => !IsInvalid;

    /// <summary>
    /// Returns all object validations containing validation items of a specific type.
    /// </summary>
    /// <param name="validationType">The validation type.</param>
    /// <returns>List of object validations containing validation items of a matching type.</returns>
    public IEnumerable<ObjectValidation> GetValidationsOfType(ValidationTypes validationType)
    {
        return ObjectValidations.Where(ov => 
            ov.Validations.Any(v => v.ValidationType == validationType));
    }

    public ValidationResultSet(object rootObject)
    {
        RootObject = rootObject ?? throw new ArgumentNullException(nameof(rootObject));
        ValidationType = ValidationTypes.Valid;
    }

    /// <summary>
    /// Creates new instance representing a validation result.
    /// </summary>
    /// <param name="rootObject">The root object that was validated.</param>
    /// <param name="validator">The validator used to validated the object.</param>
    public ValidationResultSet(object rootObject, IObjectValidator validator)
    {
        RootObject = rootObject ?? throw new ArgumentNullException(nameof(rootObject));

        if (validator == null) throw new ArgumentNullException(nameof(validator));

        var validations = new List<ObjectValidation>();
        BuildValidationList(validations, validator);

        ObjectValidations = validations.ToArray();
        ValidationType = GetMaxValidationType(ObjectValidations);
    }

    /// <summary>
    /// Returns a validation result indicating that validation was not applied.
    /// </summary>
    /// <param name="rootObject">The root object.</param>
    /// <returns></returns>
    public static ValidationResultSet ValidResult(object rootObject)
    {
        return new ValidationResultSet(rootObject);
    }

    /// <summary>
    /// Determines if any of the object validation items are of the specified validation level.
    /// </summary>
    /// <param name="validationType">The validation level to check.</param>
    /// <returns>True of if there is a validation of the specified type.</returns>
    public bool Contains(ValidationTypes validationType)
    {
        return ObjectValidations.Any(v => v.ValidationType == validationType);
    }

    private static ValidationTypes GetMaxValidationType(ObjectValidation[] validations)
    {
        if (validations.Empty())
        {
            return ValidationTypes.Valid;
        }

        return validations.OrderByDescending(v => v.ValidationType)
            .First().ValidationType;
    }

    // Recursively processed the validations and builds a flat list of object validations. 
    private static void BuildValidationList(ICollection<ObjectValidation> validations,
        IObjectValidator validator)
    {
        var result = new ObjectValidation(
            validator.Object,
            validator.Validations.ToArray());

        if (result.Validations.Any())
        {
            validations.Add(result);
        }

        // Process child object validators.
        foreach (IObjectValidator childValidator in validator.Children)
        {
            BuildValidationList(validations, childValidator);
        }
    }

    /// <summary>
    /// Throws and exception of the result contains any error level validations.
    /// </summary>
    /// <param name="notifyClient">Can be used by consuming code to determine if the 
    /// exception is something that should be returned to the client making the call.
    /// </param>
    public void ThrowIfInvalid(bool notifyClient = false)
    {
        if (ValidationType == ValidationTypes.Error)
        {
            throw new ValidationResultException(RootObject, notifyClient, this);
        }
    }
}
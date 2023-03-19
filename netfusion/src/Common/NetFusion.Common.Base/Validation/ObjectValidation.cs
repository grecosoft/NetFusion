using System;
using System.Linq;

namespace NetFusion.Common.Base.Validation;

/// <summary>
/// Contains the validation results for a specific object.
/// </summary>
public class ObjectValidation
{
    /// <summary>
    /// The validated object.
    /// </summary>
    public object Object { get; }

    /// <summary>
    /// List of validations associated with the object.
    /// </summary>
    public ValidationItem[] Validations { get; }

    /// <summary>
    /// The maximum record validation level ordered as follows:  
    /// Error, Warning, Info.   
    /// </summary>
    public ValidationTypes ValidationType { get; }

    /// <summary>
    /// Creates an object containing validations associated with an object.
    /// </summary>
    /// <param name="obj">The validated object.</param>
    /// <param name="validations">The associated validations.</param>
    public ObjectValidation(object obj, 
        ValidationItem[] validations)
    {
        Object = obj ?? throw new ArgumentNullException(nameof(obj));
        Validations = validations ?? throw new ArgumentNullException(nameof(validations));

        ValidationType = GetMaxValidationType();
    }

    private ValidationTypes GetMaxValidationType()
    {
        if (Validations.Length == 0)
        {
            return ValidationTypes.Valid;
        }

        return Validations.OrderByDescending(v => v.ValidationType)
            .First().ValidationType;
    }
}
namespace NetFusion.Common.Base.Validation;

/// <summary>
/// Implemented by a class to indicate that it should be passed its corresponding
/// validator so validations can be added.
/// </summary>
public interface IValidatableType
{
    /// <summary>
    /// Called during the validation process to allow a class instance to apply validations.
    /// </summary>
    /// <param name="validator">The validator instance associated with the object being validated.</param>
    void Validate(IObjectValidator validator);
}
using NetFusion.Base.Validation;

namespace NetFusion.Bootstrap.Validation
{
    /// <summary>
    /// Service for validating objects using the host provided IObjectValidator instance.
    /// </summary>
    public interface IValidationService
    {
        /// <summary>
        /// Validates the object and all if its children and returns a flattened
        /// list all objects with validations.
        /// </summary>
        /// <returns>Result set containing all objects with invalidations.</returns>
        ValidationResultSet Validate(object obj);
    }
}

using NetFusion.Base.Validation;

namespace NetFusion.Bootstrap.Validation
{
    /// <summary>
    /// Service for validating objects using the host provided IObjectValidator instance.
    /// If the host does not provided a IObjectValidatory implementation, a version based
    /// on Microsoft's Data Annotations is used.
    /// </summary>
    public interface IValidationService
    {
        /// <summary>
        /// Validates the object and all if its children and returns a flattened
        /// list containing objects with validations.
        /// </summary>
        /// <returns>Result set containing all objects with invalidations.</returns>
        ValidationResultSet Validate(object obj);
    }
}

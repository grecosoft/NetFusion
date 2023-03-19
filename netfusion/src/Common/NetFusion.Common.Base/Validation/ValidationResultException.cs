using System;
using NetFusion.Common.Base.Exceptions;

namespace NetFusion.Common.Base.Validation;

/// <summary>
/// Exception that is thrown when an object is determined to be invalid.
/// </summary>
public class ValidationResultException : NetFusionException
{
    /// <summary>
    /// Indicates if the client who made the request should be notified of the
    /// validation exception.  This should only be set if the validations can
    /// can be viewed by the client.
    /// </summary>
    /// <returns>Boolean value.</returns>
    public bool NotifyClient { get; }

    /// <summary>
    /// The associated validation result set.
    /// </summary>
    /// <returns>The set of invalidations associated with the exception.</returns>
    public ValidationResultSet Result { get; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="invalidObject">The object that failed validation.</param>
    /// <param name="notifyClient">Indicates if client should be notified.</param>
    /// <param name="result">The set of invalidations associated with the exception.</param>
    public ValidationResultException(
        object invalidObject,
        bool notifyClient,
        ValidationResultSet result) : base(
        "Validation Exceptions", 
        "Validations", 
        GetValidationDetails(invalidObject, result))
    {
        Result = result ?? throw new ArgumentNullException(nameof(result));
        NotifyClient = notifyClient;
    }

    private static object GetValidationDetails(object invalidObject, ValidationResultSet result)
    {
        return new
        {
            Object = invalidObject,
            Result = result
        };
    }
}
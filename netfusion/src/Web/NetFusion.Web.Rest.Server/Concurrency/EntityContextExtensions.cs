using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetFusion.Common.Base.Entity;
using NetFusion.Web.Rest.Resources;

namespace NetFusion.Web.Rest.Server.Concurrency;

/// <summary>
/// Contains extension methods for ConcurrencyContext class used to record concurrency
/// information associated with the current request.  The extension methods interpret
/// the state of the ConcurrencyContext and maps it to its associated HTTP objects.
/// </summary>
public static class EntityContextExtensions
{
    /// <summary>
    /// Based on the state of the ConcurrencyContext associated with the current request,
    /// creates a corresponding ActionResult.
    /// </summary>
    /// <param name="context">ConcurrencyContext instance associate with current request.</param>
    /// <param name="map">Optional map delegate used to map the current entity state set
    /// when an update fails due to optimistic concurrency.</param>
    /// <returns>Action Result.</returns>
    public static ActionResult? ToResult(this EntityContext context, 
        Func<object, HalResource>? map = null)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
            
        if (! context.PreConditionSatisfied)
        {
            return new StatusCodeResult(StatusCodes.Status428PreconditionRequired);
        }
            
        // If the currency check was satisfied, returning null allows controller to structure code as:
        //  return context.ToResult() ?? ObjectResult(updatedResult);
        if (context.ConcurrencyCheckSatisfied) return null;
            
        // Determine if the application specified the current object state that failed the concurrency check
        // and return as the result or map the result into a resource if a mapping delegated was specified.
        if (context.CurrentEntityState == null) return new StatusCodeResult(StatusCodes.Status412PreconditionFailed);

        return new ObjectResult(map == null ? context.CurrentEntityState : map(context.CurrentEntityState))
        {
            StatusCode = StatusCodes.Status412PreconditionFailed
        };
    }
}
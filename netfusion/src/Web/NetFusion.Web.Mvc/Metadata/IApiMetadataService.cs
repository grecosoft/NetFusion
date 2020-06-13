using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace NetFusion.Web.Mvc.Metadata
{
    /// <summary>
    /// Returns metadata for controller and its associated routes.
    /// </summary>
    public interface IApiMetadataService
    {
        /// <summary>
        /// Returns the description metadata populated by ASP.NET WebApi corresponding
        /// to a controller action method.
        /// </summary>
        /// <param name="methodInfo">The runtime information for a controller's action
        /// method.</param>
        /// <returns>The associated description or an exception of not found.</returns>
        ApiActionMeta GetActionMeta(MethodInfo methodInfo);

        ApiActionMeta GetActionMeta<T>(string actionName, params Type[] paramTypes)
            where T : ControllerBase;
    }
}
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
        /// <param name="methodInfo">The runtime information for a controller's action method.</param>
        /// <param name="actionMeta">The metadata associated with the action.</param>
        /// <returns>True if found.  Otherwise, False.</returns>
        bool TryGetActionMeta(MethodInfo methodInfo, out ApiActionMeta actionMeta);

        ApiActionMeta GetActionMeta(MethodInfo methodInfo);
        
        ApiActionMeta GetActionMeta<T>(string actionName, params Type[] paramTypes)
            where T : ControllerBase;
    }
}
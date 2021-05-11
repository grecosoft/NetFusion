using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace NetFusion.Web.Mvc.Metadata
{
    /// <summary>
    /// Returns metadata for a controller and its associated routes.
    /// </summary>
    public interface IApiMetadataService
    {
        /// <summary>
        /// Determines if there is metadata for a given controller action relative path having
        /// the associated HTTP method.
        /// </summary>
        /// <param name="httpMethod">The http method used to invoke the action URI.</param>
        /// <param name="relativePath">The path associated with the controller action.</param>
        /// <param name="actionMeta">Reference to the controller action metadata if found.</param>
        /// <returns>True if the controller metadata is found.  Otherwise, false.</returns>
        bool TryGetActionMeta(string httpMethod, string relativePath, out ApiActionMeta actionMeta);
        
        /// <summary>
        /// Determines if there is metadata for a given controller action method. 
        /// </summary>
        /// <param name="methodInfo">The runtime method information for the controller action.</param>
        /// <param name="actionMeta">Reference to the controller action metadata if found.</param>
        /// <returns>True if the controller metadata is found.  Otherwise, false.</returns>
        bool TryGetActionMeta(MethodInfo methodInfo, out ApiActionMeta actionMeta);

        /// <summary>
        /// Returns the metadata for a given controller action method.
        /// </summary>
        /// <param name="methodInfo">The runtime method information for the controller action.</param>
        /// <returns>Reference to the metadata.  If not found, an exception is raised.</returns>
        ApiActionMeta GetActionMeta(MethodInfo methodInfo);
        
        /// <summary>
        /// Returns to the controller action metadata.
        /// </summary>
        /// <param name="actionName">The name of the controller action.</param>
        /// <param name="paramTypes">The types passed to the controller action.</param>
        /// <typeparam name="T">The controller on which the action method is defined.</typeparam>
        /// <returns>Reference to the metadata.  If not found, an exception is raised.</returns>
        ApiActionMeta GetActionMeta<T>(string actionName, params Type[] paramTypes)
            where T : ControllerBase;
    }
}
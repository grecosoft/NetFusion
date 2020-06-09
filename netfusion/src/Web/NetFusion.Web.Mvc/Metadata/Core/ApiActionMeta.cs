using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace NetFusion.Web.Mvc.Metadata.Core
{
    /// <summary>
    /// Metadata containing information for controller routes
    /// decorated with the ActionMetadataAttribute.
    /// </summary>
    public class ApiActionMeta
    {
        /// <summary>
        /// The method information associated with the controller's action.
        /// </summary>
        public MethodInfo ActionMethodInfo { get; private set; }
        
        /// <summary>
        /// The name of a controller's action.
        /// </summary>
        public string ActionName { get; private set; }
        
        /// <summary>
        /// The name of the action's associated controller.
        /// </summary>
        public string ControllerName { get; private set; }

        /// <summary>
        /// The HTTP method used to call the action's route.
        /// </summary>
		public string HttpMethod { get; }

        /// <summary>
        /// The path corresponding to the action's route.
        /// </summary>
		public string RelativePath { get; }

        /// <summary>
        /// Metadata about the route template parameters.
        /// </summary>
        public ApiParameterMeta[] Parameters { get; }

        
        public ApiActionMeta(ApiDescription description)
        {
            if (description == null) throw new ArgumentNullException(nameof(description));
            
            RelativePath = description.RelativePath;
            HttpMethod  = description.HttpMethod;
            Parameters = GetActionParameters(description.ActionDescriptor);

            SetActionDescriptions(description);
        }

        private void SetActionDescriptions(ApiDescription apiDescription)
        {
            var actionDescriptor = (ControllerActionDescriptor)apiDescription.ActionDescriptor;
            ActionMethodInfo = actionDescriptor.MethodInfo;
            ActionName = actionDescriptor.ActionName;
            ControllerName = actionDescriptor.ControllerName;
        }

        public ApiActionMeta(
            ApiDescription description,
            ControllerActionDescriptor actionDescriptor)
        {
            if (description == null) throw new ArgumentNullException(nameof(description));
            if (actionDescriptor == null) throw new ArgumentNullException(nameof(actionDescriptor));
            
            RelativePath = description.RelativePath;
            HttpMethod  = description.HttpMethod;
            Parameters = GetActionParameters(actionDescriptor);
        }
        
        private static ApiParameterMeta[] GetActionParameters(ActionDescriptor actionDescriptor) 
        {
            return actionDescriptor.Parameters.OfType<ControllerParameterDescriptor>()
                .Select(p => new ApiParameterMeta(p))
                .ToArray(); 
        }
    }
}
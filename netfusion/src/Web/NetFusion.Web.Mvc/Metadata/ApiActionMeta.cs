using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace NetFusion.Web.Mvc.Metadata
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
        public ApiParameterMeta[] RouteParameters { get; }
        
        /// <summary>
        /// Metadata about the headers accepted by the action.
        /// </summary>
        public ApiParameterMeta[] HeaderParameters { get; }

        
        public ApiActionMeta(ApiDescription description)
        {
            if (description == null) throw new ArgumentNullException(nameof(description));
            
            RelativePath = description.RelativePath;
            HttpMethod  = description.HttpMethod;
            RouteParameters = GetActionParameters(description.ActionDescriptor, BindingSource.Path);
            HeaderParameters = GetActionParameters(description.ActionDescriptor, BindingSource.Header);

            SetActionDescriptions(description);
        }

        private void SetActionDescriptions(ApiDescription apiDescription)
        {
            var actionDescriptor = (ControllerActionDescriptor)apiDescription.ActionDescriptor;
            ActionMethodInfo = actionDescriptor.MethodInfo;
            ActionName = actionDescriptor.ActionName;
            ControllerName = actionDescriptor.ControllerName;
        }

        private static ApiParameterMeta[] GetActionParameters(ActionDescriptor actionDescriptor, BindingSource source) 
        {
            return actionDescriptor.Parameters.OfType<ControllerParameterDescriptor>()
                .Where(p => p.BindingInfo.BindingSource == source)
                .Select(p => new ApiParameterMeta(p))
                .ToArray(); 
        }
    }
}
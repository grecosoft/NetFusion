using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using NetFusion.Common;
using NetFusion.Common.Extensions.Reflection;
using System;
using System.Linq;

namespace NetFusion.Web.Mvc.Metadata.Core
{
    /// <summary>
    /// Metadata containing information for controller routes
    /// decorated with the ActionMetadataAttribute.
    /// </summary>
    public class ApiActionMeta
    {
        /// <summary>
        /// The name assigned to the to the action.
        /// </summary>
        public string ActionName { get; }

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

        public ApiActionMeta(
            ApiDescription description,
            ControllerActionDescriptor actionDescriptor)
        {
            Check.NotNull(description, nameof(description));
            Check.NotNull(actionDescriptor, nameof(actionDescriptor));

            this.ActionName = GetActionName(actionDescriptor);
            this.RelativePath = description.RelativePath;
            this.HttpMethod  = description.HttpMethod;
            this.Parameters = GetActionParameters(actionDescriptor);
        }

        private string GetActionName(ControllerActionDescriptor actionDescriptor)
        {
            var attrib = actionDescriptor.MethodInfo.GetAttribute<ActionMetaAttribute>();
            if (attrib == null)
            {
                throw new InvalidOperationException(
                    $"Action metadata can only be created for controller routes decorated with " +
                    $"{nameof(ActionMetaAttribute)}.  The route named {actionDescriptor.ActionName} " +
                    $"does not have attribute specified");
            }

            return attrib.ActionName;
        }

        private ApiParameterMeta[] GetActionParameters(ControllerActionDescriptor actionDescriptor) 
        {
            return actionDescriptor.Parameters.OfType<ControllerParameterDescriptor>()
                .Select(p => new ApiParameterMeta(p))
                .ToArray(); 
        }
    }
}
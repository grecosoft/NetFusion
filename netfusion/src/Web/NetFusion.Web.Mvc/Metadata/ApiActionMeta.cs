using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
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
        
        /// <summary>
        /// Metadata about the query parameters accepted by the action.
        /// </summary>
        public ApiParameterMeta[] QueryParameters { get; }
        
        public ApiParameterMeta[] BodyParameters { get; }
        
        public ApiResponseMeta[] ResponseMeta { get; private set; }

        private readonly ApiDescription _apiDescription;

        public ApiActionMeta(ApiDescription apiDescription)
        {
            _apiDescription = apiDescription ?? throw new ArgumentNullException(nameof(apiDescription));
            
            SetActionMeta(apiDescription);
            
            RelativePath = apiDescription.RelativePath;
            HttpMethod  = apiDescription.HttpMethod;
            
            RouteParameters = GetActionParameters(apiDescription, BindingSource.Path);
            HeaderParameters = GetActionParameters(apiDescription, BindingSource.Header);
            QueryParameters = GetActionParameters(apiDescription, BindingSource.Query);
            BodyParameters = GetActionParameters(apiDescription, BindingSource.Body);
            
            SetActionResponseMeta(apiDescription);
        }

        public IEnumerable<T> GetFilterMetadata<T>()
        {
            return _apiDescription.ActionDescriptor.FilterDescriptors.Select(fd => fd.Filter).OfType<T>();
        }

        private void SetActionMeta(ApiDescription apiDescription)
        {
            var actionDescriptor = (ControllerActionDescriptor)apiDescription.ActionDescriptor;
            ActionMethodInfo = actionDescriptor.MethodInfo;
            ActionName = actionDescriptor.ActionName;
            ControllerName = actionDescriptor.ControllerName;
        }

        // Looks for all ProducesResponseType attributes describing the results that can be
        // produced by the action method.
        private void SetActionResponseMeta(ApiDescription apiDescription)
        {
            ResponseMeta = GetFilterMetadata<ProducesResponseTypeAttribute>(apiDescription)
                .Select(p => new ApiResponseMeta(p.StatusCode, p.Type))
                .ToArray();
            
            // Next, determine if the action method as the ProducesDefaultResponseType attribute
            // specified to determine the response type to use if not specified.
            var defaultType = GetFilterMetadata<ProducesDefaultResponseTypeAttribute>(apiDescription).FirstOrDefault();
            if (defaultType == null) return;

            foreach (ApiResponseMeta meta in ResponseMeta.Where(rm => rm.ModelType == null))
            {
                meta.ModelType = defaultType.Type;
            }
        }

        private static ApiParameterMeta[] GetActionParameters(ApiDescription apiDescription, BindingSource source)
        {
            return apiDescription.ParameterDescriptions
                .Where(p => p.Source == source)
                .Select(p => new ApiParameterMeta(p))
                .ToArray(); 
        }

        private static IEnumerable<T> GetFilterMetadata<T>(ApiDescription apiDescription)
            where T: IFilterMetadata
        {
            var actionDescriptor = (ControllerActionDescriptor)apiDescription.ActionDescriptor;
            return actionDescriptor.FilterDescriptors.Select(fd => fd.Filter).OfType<T>();
        }
    }
}
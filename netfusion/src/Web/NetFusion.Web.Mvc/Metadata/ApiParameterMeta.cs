using System;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace NetFusion.Web.Mvc.Metadata
{
    /// <summary>
    /// Parameter metadata for route template parameters that are
    /// specified as part of the route URL.
    /// </summary>
    public class ApiParameterMeta
    {
        /// <summary>
        /// The name of the parameter contained within the route template.
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        /// The type of the parameter expected by the controller's action.
        /// </summary>
        public Type ParameterType { get; }
        
        /// <summary>
        /// The name used by the caller to bind a value to the parameter.
        /// </summary>
        public string BindingName { get; }

        /// <summary>
        /// Determines if the route parameter is optional and a value need not specified.
        /// </summary>
        public bool IsOptional { get; }

        /// <summary>
        /// The specified default value used if one is not specified within the route.
        /// </summary>
        public object DefaultValue { get; }

        public ApiParameterMeta(ControllerParameterDescriptor descriptor)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            
            
            ParameterName = descriptor.Name;
            ParameterType = descriptor.ParameterType;
            BindingName = descriptor.BindingInfo.BinderModelName;
            IsOptional = descriptor.ParameterInfo.IsOptional;
            DefaultValue = descriptor.ParameterInfo.DefaultValue;
        }
    }
}
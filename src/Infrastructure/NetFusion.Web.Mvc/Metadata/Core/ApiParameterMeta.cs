using Microsoft.AspNetCore.Mvc.Controllers;
using NetFusion.Common;
using System;

namespace NetFusion.Web.Mvc.Metadata.Core
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
        /// Determines if the route parameter is optional and a value
        /// not need specified.
        /// </summary>
        public bool IsOptional { get; }

        /// <summary>
        /// The specified default value used if one is not specified within the route.
        /// </summary>
        public object DefaultValue { get; }

        public ApiParameterMeta(ControllerParameterDescriptor descriptor)
        {
            Check.NotNull(descriptor, nameof(descriptor));

            this.ParameterName = descriptor.Name;
            //this.ParameterType = descriptor.ParameterType;
            this.IsOptional = descriptor.ParameterInfo.IsOptional;
            this.DefaultValue = descriptor.ParameterInfo.DefaultValue;
        }
    }
}
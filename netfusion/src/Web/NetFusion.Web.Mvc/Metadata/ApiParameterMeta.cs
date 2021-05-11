using System;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace NetFusion.Web.Mvc.Metadata
{
    /// <summary>
    /// Contains metadata for different parameter sources used to populate
    /// values used by the controller.
    /// </summary>
    public class ApiParameterMeta
    {
        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        /// The type of the parameter expected.
        /// </summary>
        public Type ParameterType { get; }

        /// <summary>
        /// Determines if the route parameter is optional.
        /// </summary>
        public bool IsOptional { get; }

        /// <summary>
        /// The specified default value used if one is not specified within the route.
        /// </summary>
        public object DefaultValue { get; }

        public ApiParameterMeta(ApiParameterDescription apiParameterDescription)
        {
            if (apiParameterDescription == null) throw new ArgumentNullException(nameof(apiParameterDescription));
            
            if (apiParameterDescription.ParameterDescriptor is not ControllerParameterDescriptor paramDescriptor)
            {
                throw new InvalidCastException(
                    $"Expected {nameof(ParameterDescriptor)} derived type of: {typeof(ControllerParameterDescriptor)}");
            }
            
            ParameterName = apiParameterDescription.Name;
            ParameterType = apiParameterDescription.Type;
            IsOptional = paramDescriptor.ParameterInfo.IsOptional;

            object defaultValue = paramDescriptor.ParameterInfo.DefaultValue ?? DBNull.Value;
            if (defaultValue.GetType() != typeof(DBNull))
            {
                DefaultValue = paramDescriptor.ParameterInfo.DefaultValue;
            }
        }
    }
}
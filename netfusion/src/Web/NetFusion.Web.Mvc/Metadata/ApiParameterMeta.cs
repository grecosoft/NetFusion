using System;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
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
        /// Determines if the route parameter is optional and a value need not specified.
        /// </summary>
        public bool IsOptional { get; }

        /// <summary>
        /// The specified default value used if one is not specified within the route.
        /// </summary>
        public object DefaultValue { get; }

        public ApiParameterMeta(ApiParameterDescription apiParameterDescription)
        {
            if (apiParameterDescription == null) throw new ArgumentNullException(nameof(apiParameterDescription));
            
            if (!(apiParameterDescription.ParameterDescriptor is ControllerParameterDescriptor paramDescriptor))
            {
                throw new InvalidCastException(
                    $"Expected ParameterDescriptor derived type of: {typeof(ControllerParameterDescriptor)}");
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
using System;
using System.Reflection;

namespace NetFusion.Rest.Server.Linking
{
    /// <summary>
    /// This class maps the action method parameter name to the corresponding resource's named property.
    /// This mapping is executed at runtime to obtain the resource's property value to be used as the 
    /// corresponding route-value.
    /// </summary>
    public class RouteParameter
    {
        /// <summary>
        /// The name of the route-value parameter corresponding to the controller's action method.
        /// </summary>
        public string ActionParamName { get; }

        /// <summary>
        /// The property on the resource corresponding to the ActionParamName.  When generating links,
        /// the value of this property on the resource is used as the corresponding route-value.
        /// </summary>
        public string ResourcePropName { get; }

        /// <summary>
        /// Information for the property on the resource corresponding to the ResourcePropName.
        /// </summary>
        public PropertyInfo ResourcePropInfo { get; }

        /// <summary>
        /// The method to be called to obtain the resource property value corresponding to the
        /// controller's action method route-value.
        /// </summary>
        public MethodInfo ResourceMethodInfo { get;  }

        public RouteParameter(string actionParamName, PropertyInfo resourcePropInfo)
        {
            if (string.IsNullOrWhiteSpace(actionParamName))
                throw new ArgumentException("Action parameter name not specified.", nameof(actionParamName));

            if (resourcePropInfo == null) throw new ArgumentNullException(nameof(resourcePropInfo),
                "Resource property cannot be null.");

            ActionParamName = actionParamName;
            ResourcePropName = resourcePropInfo.Name;
            ResourcePropInfo = resourcePropInfo;
            ResourceMethodInfo = resourcePropInfo.GetGetMethod();
        }

        // Called at runtime to receive the value of the resource property corresponding
        // to the action method's route parameter.
        public object GetPropValue(object resource)
        {
            if (resource == null)throw new ArgumentNullException(nameof(resource), 
                "Resource not specified.");

            return ResourceMethodInfo.Invoke(resource, null);
        }
    }
}

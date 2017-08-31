using NetFusion.Rest.Resources;
using System;
using System.Reflection;

namespace NetFusion.Rest.Server.Actions
{
    /// <summary>
    /// During the bootstrap process, the plug-in scans for all IResourceMap instances indicating how
    /// a given resource should be augmented with links.  At runtime, this cached information is used
    /// to generate URL's corresponding to controller action methods.
    /// 
    /// This class maps the action method parameter name to the corresponding resource's named property.
    /// This mapping is executed at runtime to obtain the resources's property value to be used as the 
    /// corresponding route-value.
    /// </summary>
    public class ActionParamValue
    {
        /// <summary>
        /// The name of the route-value parameter corresponding to the controller's action method.
        /// </summary>
        public string ActionParamName { get; }

        /// <summary>
        /// The property on the resource corresponding to the ActionParamName.
        /// When generating links, the value of this property on the resource 
        /// is used as the corresponding route-value.
        /// </summary>
        public string ResourcePropName { get; }

        /// <summary>
        /// Information for the property on the resource corresponding to the ActionParamName.
        /// </summary>
        public PropertyInfo ResourcePropInfo { get; }

        /// <summary>
        /// The method to be called to obtain the resource property value
        /// corresponding the that controller's action method route-value.
        /// </summary>
        public MethodInfo ResourceMethodInfo { get;  }

        public ActionParamValue(string actionParamName, PropertyInfo resourcePropInfo)
        {
            if (string.IsNullOrWhiteSpace(actionParamName))
                throw new ArgumentException("Action parameter name not specified.", nameof(actionParamName));

            if (resourcePropInfo == null)
                throw new ArgumentNullException(nameof(resourcePropInfo), "Resource property info not specified.");

            ActionParamName = actionParamName;
            ResourcePropName = resourcePropInfo.Name;
            ResourcePropInfo = resourcePropInfo;
            ResourceMethodInfo = resourcePropInfo.GetGetMethod();
        }

        // Creates new instance based a different resource type.
        public ActionParamValue CreateCopyFor<TNewResourceType>()
            where TNewResourceType : class, IResource
        {
            var newResourcePropInfo = typeof(TNewResourceType).GetProperty(ResourcePropName);
            return new ActionParamValue(ActionParamName, newResourcePropInfo);
        }

        // Called at runtime to receive the value of the resource property corresponding
        // to the action method's route parameter.
        public object GetModelPropValue(object resource)
        {
            if (resource == null)
                throw new ArgumentNullException(nameof(resource), "Resource not specified.");

            return ResourceMethodInfo.Invoke(resource, null);
        }
    }
}

using System;
using System.Reflection;

namespace NetFusion.Rest.Server.Linking
{
    /// <summary>
    /// This class maps the action method parameter name to the corresponding state named property.
    /// This mapping is executed at runtime to obtain the resource's state property value to be used
    /// as the corresponding route-value.
    /// </summary>
    public class RouteParameter
    {
        /// <summary>
        /// The name of the route-value parameter corresponding to the controller's action method.
        /// </summary>
        public string ActionParamName { get; }

        /// <summary>
        /// The property on resource's state corresponding to the ActionParamName.  When generating
        /// links, the value of this property on the sate is used as the corresponding route-value.
        /// </summary>
        public string SourcePropName { get; }

        /// <summary>
        /// Information for the property on the resource's state corresponding to the ModelPropName.
        /// </summary>
        public PropertyInfo SourcePropInfo { get; }

        /// <summary>
        /// The method to be called to obtain the state's property value corresponding to the
        /// controller's action method route-value.
        /// </summary>
        public MethodInfo SourceMethodInfo { get;  }

        public RouteParameter(string actionParamName, PropertyInfo statePropInfo)
        {
            if (string.IsNullOrWhiteSpace(actionParamName))
                throw new ArgumentException("Action parameter name not specified.", nameof(actionParamName));

            if (statePropInfo == null) throw new ArgumentNullException(nameof(statePropInfo),
                "State property cannot be null.");

            ActionParamName = actionParamName;
            SourcePropName = statePropInfo.Name;
            SourcePropInfo = statePropInfo;
            SourceMethodInfo = statePropInfo.GetGetMethod();
        }

        // Called at runtime to receive the value of the state's property corresponding
        // to the action method's route parameter.
        public object GetSourcePropValue(object source)
        {
            if (source == null)throw new ArgumentNullException(nameof(source), 
                "Source not specified.");

            return SourceMethodInfo.Invoke(source, null);
        }
    }
}

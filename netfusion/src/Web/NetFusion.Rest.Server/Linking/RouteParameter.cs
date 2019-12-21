using System;
using System.Reflection;

namespace NetFusion.Rest.Server.Linking
{
    /// <summary>
    /// This class maps the action method parameter name to the corresponding model named property.
    /// This mapping is executed at runtime to obtain the model's property value to be used as the 
    /// corresponding route-value.
    /// </summary>
    public class RouteParameter
    {
        /// <summary>
        /// The name of the route-value parameter corresponding to the controller's action method.
        /// </summary>
        public string ActionParamName { get; }

        /// <summary>
        /// The property on the model corresponding to the ActionParamName.  When generating links,
        /// the value of this property on the model is used as the corresponding route-value.
        /// </summary>
        public string ModelPropName { get; }

        /// <summary>
        /// Information for the property on the model corresponding to the ModelPropName.
        /// </summary>
        public PropertyInfo ModelPropInfo { get; }

        /// <summary>
        /// The method to be called to obtain the model's property value corresponding to the
        /// controller's action method route-value.
        /// </summary>
        public MethodInfo ModelMethodInfo { get;  }

        public RouteParameter(string actionParamName, PropertyInfo modelPropInfo)
        {
            if (string.IsNullOrWhiteSpace(actionParamName))
                throw new ArgumentException("Action parameter name not specified.", nameof(actionParamName));

            if (modelPropInfo == null) throw new ArgumentNullException(nameof(modelPropInfo),
                "Model property cannot be null.");

            ActionParamName = actionParamName;
            ModelPropName = modelPropInfo.Name;
            ModelPropInfo = modelPropInfo;
            ModelMethodInfo = modelPropInfo.GetGetMethod();
        }

        // Called at runtime to receive the value of the model's property corresponding
        // to the action method's route parameter.
        public object GetModelPropValue(object model)
        {
            if (model == null)throw new ArgumentNullException(nameof(model), 
                "Model not specified.");

            return ModelMethodInfo.Invoke(model, null);
        }
    }
}

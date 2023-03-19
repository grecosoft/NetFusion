using System;
using System.Reflection;

namespace NetFusion.Web.Rest.Server.Linking;

/// <summary>
/// This class maps the action method parameter name to the corresponding model named property.
/// This mapping is executed at runtime to obtain the model's property values to be used as the
/// corresponding route-values.
/// </summary>
public class RouteParameter
{
    /// <summary>
    /// The name of the route-value parameter corresponding to the controller's action method.
    /// </summary>
    public string ActionParamName { get; }

    /// <summary>
    /// The property on model corresponding to the ActionParamName.  When generating links,
    /// the value of this property on the model is used as the corresponding route-value.
    /// </summary>
    public string SourcePropName { get; }

    /// <summary>
    /// Information for the property on the model corresponding to the SourcePropName.
    /// </summary>
    public PropertyInfo SourcePropInfo { get; }

    /// <summary>
    /// The method to be called to obtain the model's property value corresponding to the
    /// controller's action method route-value.
    /// </summary>
    public MethodInfo SourceMethodInfo { get;  }

    public RouteParameter(string? actionParamName, PropertyInfo sourcePropInfo)
    {
        if (string.IsNullOrWhiteSpace(actionParamName))
            throw new ArgumentException("Action parameter name not specified.", nameof(actionParamName));

        if (sourcePropInfo == null) throw new ArgumentNullException(nameof(sourcePropInfo),
            "State property cannot be null.");
            
        ActionParamName = actionParamName;
        SourcePropName = sourcePropInfo.Name;
        SourcePropInfo = sourcePropInfo;
        SourceMethodInfo = sourcePropInfo.GetGetMethod() ?? throw new InvalidOperationException(
            $"Get Method not defined for Property: {SourcePropName} on Type: {sourcePropInfo.DeclaringType}" );
    }

    // Called at runtime to receive the value of the state's property corresponding
    // to the action method's route parameter.
    public object? GetSourcePropValue(object source)
    {
        if (source == null)throw new ArgumentNullException(nameof(source), 
            "Source not specified.");

        return SourceMethodInfo.Invoke(source, null);
    }
}
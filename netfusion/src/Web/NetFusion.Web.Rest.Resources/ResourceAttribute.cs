using System;

namespace NetFusion.Web.Rest.Resources;

/// <summary>
/// Used to map a string name to a given resource model.  This attribute
/// specifies a name used to identity the resource to clients consuming
/// the REST Api.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ResourceAttribute : Attribute
{
    public string ResourceName { get; }

    public ResourceAttribute(string resourceName)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
            throw new ArgumentException("Resource name not specified.", nameof(resourceName));

        ResourceName = resourceName;
    }
}
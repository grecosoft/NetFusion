using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NetFusion.Web.Rest.Docs;

/// <summary>
/// Attribute used to specify the possible embedded resources returned from a WebApi method.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class EmbeddedResourceAttribute : Attribute, IFilterMetadata
{
    /// <summary>
    /// The parent resource type containing an embedded resource or resource collection.
    /// </summary>
    public Type ParentResourceType { get; set; }
        
    /// <summary>
    /// The type of the child resource embedded within the parent resource.
    /// </summary>
    public Type ChildResourceType { get; set; }
        
    /// <summary>
    /// The name used to identity the embedded resource or resource collection.
    /// </summary>
    public string EmbeddedName { get; set; }

    /// <summary>
    /// Indicates that the embedded child is a collection of embedded resources.
    /// </summary>
    public bool IsCollection { get; set; }

    public EmbeddedResourceAttribute(Type parentResourceType, Type childResourceType,
        string embeddedName,
        bool isCollection = false)
    {
        if (string.IsNullOrWhiteSpace(embeddedName))
        {
            throw new ArgumentException("Embedded Name must be specified.", nameof(embeddedName));
        }

        ParentResourceType = parentResourceType ?? throw new ArgumentNullException(nameof(parentResourceType));
        ChildResourceType = childResourceType ?? throw new ArgumentNullException(nameof(childResourceType));

        EmbeddedName = embeddedName;
        IsCollection = isCollection;
    }
}
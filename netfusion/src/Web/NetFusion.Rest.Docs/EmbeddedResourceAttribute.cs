using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NetFusion.Rest.Docs
{
    /// <summary>
    /// Attribute used to specify the possible embedded resources returned
    /// from a WebApi method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class EmbeddedResourceAttribute : Attribute, IFilterMetadata
    {
        public Type ParentResourceType { get; set; }
        public string EmbeddedName { get; set; }
        public Type ChildResourceType { get; set; }
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
}
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
        public Type ParentModelType { get; set; }
        public string EmbeddedName { get; set; }
        public Type ChildModelType { get; set; }
        
        public EmbeddedResourceAttribute(Type parentModelType, Type childModelType, string embeddedName)
        {
            if (string.IsNullOrWhiteSpace(embeddedName))
            {
                throw new ArgumentException("Embedded Name must be specified.", nameof(embeddedName));
            }

            ParentModelType = parentModelType ?? throw new ArgumentNullException(nameof(parentModelType));
            ChildModelType = childModelType ?? throw new ArgumentNullException(nameof(childModelType));

            EmbeddedName = embeddedName;
        }
    }
}
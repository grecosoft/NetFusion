using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NetFusion.Rest.Docs.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class EmbeddedResourceAttribute : Attribute, IFilterMetadata
    {
        public Type[] EmbeddedTypes { get; }
        
        public EmbeddedResourceAttribute(params Type[] embeddedTypes)
        {
            EmbeddedTypes = embeddedTypes;
        }
    }
}
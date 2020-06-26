using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NetFusion.Rest.Docs.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class EmbeddedResourceAttribute : Attribute, IFilterMetadata
    {
        public string EmbeddedName { get; set; }
        public Type EmbeddedType { get; set; }
        
        public EmbeddedResourceAttribute(string embeddedName, Type embeddedType)
        {
            EmbeddedName = embeddedName;
            EmbeddedType = embeddedType;
        }
    }
}
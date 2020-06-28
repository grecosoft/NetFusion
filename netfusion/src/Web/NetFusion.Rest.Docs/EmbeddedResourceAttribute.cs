using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NetFusion.Rest.Docs
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class EmbeddedResourceAttribute : Attribute, IFilterMetadata
    {
        public Type ParentModelType { get; set; }
        public string EmbeddedName { get; set; }
        public Type ChildModelType { get; set; }
        
        public EmbeddedResourceAttribute(Type parentModelType, Type childModelType, string embeddedName)
        {
            ParentModelType = parentModelType;
            ChildModelType = childModelType;

            EmbeddedName = embeddedName;
        }
    }
}
using System;

namespace NetFusion.Rest.Server.Resources
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ResourceTypeAttribute : Attribute
    {
        public Type ResourceType { get; }

        public ResourceTypeAttribute(Type resourceType)
        {
            ResourceType = resourceType;
        }
    }
}

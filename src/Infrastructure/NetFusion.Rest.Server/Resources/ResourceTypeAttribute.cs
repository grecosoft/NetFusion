using System;

namespace NetFusion.Rest.Server.Resources
{
    /// <summary>
    /// Used to specify the resource type for a controller's action method that does not
    /// accept or return a resource type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ResourceTypeAttribute : Attribute
    {
        /// <summary>
        /// The action method resource type.
        /// </summary>
        public Type ResourceType { get; }

        /// <summary>
        /// Specifies the resource type associated with the action method.
        /// </summary>
        /// <param name="resourceType">The resource type.</param>
        public ResourceTypeAttribute(Type resourceType)
        {
            ResourceType = resourceType ??
                throw new ArgumentNullException(nameof(resourceType), "resource type not specified.");
        }
    }
}

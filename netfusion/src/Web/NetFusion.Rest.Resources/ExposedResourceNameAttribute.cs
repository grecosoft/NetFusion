using System;

namespace NetFusion.Rest.Resources
{
    /// <summary>
    /// Used to map a string name to a given resource implementing the IResource interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ExposedResourceNameAttribute : Attribute
    {
        public string ResourceName { get; }

        public ExposedResourceNameAttribute(string resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
                throw new ArgumentException("Resource name not specified.", nameof(resourceName));

            ResourceName = resourceName;
        }
    }
}

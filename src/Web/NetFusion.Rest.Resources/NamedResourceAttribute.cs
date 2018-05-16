using System;

namespace NetFusion.Rest.Resources
{
    /// <summary>
    /// Used to map a string name to a given resource implementing the IResource interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class NamedResourceAttribute : Attribute
    {
        public string ResourceName { get; }

        public NamedResourceAttribute(string resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
                throw new ArgumentException("Resource name not specified.", nameof(resourceName));

            ResourceName = resourceName;
        }
    }
}

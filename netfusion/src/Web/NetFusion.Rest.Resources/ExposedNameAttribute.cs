using System;

namespace NetFusion.Rest.Resources
{
    /// <summary>
    /// Used to map a string name to a given resource model.  A API model marked with this attribute,
    /// specifying an embedded name, will be used when a model is embedded into a parent.  This name
    /// is used to communicate to the client the content of an embedded item.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ExposedNameAttribute : Attribute
    {
        public string ResourceName { get; }

        public ExposedNameAttribute(string resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
                throw new ArgumentException("Resource name not specified.", nameof(resourceName));

            ResourceName = resourceName;
        }
    }
}

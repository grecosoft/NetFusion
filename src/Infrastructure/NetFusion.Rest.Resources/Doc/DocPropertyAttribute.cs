using System;

namespace NetFusion.Rest.Resources.Doc
{
    /// <summary>
    /// Attribute used to specify a description for a resource's property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Property, Inherited = true)]
    public class DocPropertyAttribute : Attribute
    {
        /// <summary>
        /// Description of the resource.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Specifies documentation for a resource's property.
        /// </summary>
        /// <param name="description">Resource property description.</param>
        public DocPropertyAttribute(string description)
        {
            Description = description 
                ?? throw new ArgumentNullException(nameof(description), "Resource Property Description not specified.");
        }
    }
}

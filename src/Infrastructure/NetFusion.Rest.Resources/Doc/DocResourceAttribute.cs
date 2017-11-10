using System;

namespace NetFusion.Rest.Resources.Doc
{
    /// <summary>
    /// Used to provide a description of a resource.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class DocResourceAttribute : Attribute
    {
        /// <summary>
        /// Description of a resource.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Specifies information used by the documentation generation process.
        /// </summary>
        /// <param name="description">Resource description.</param>
        public DocResourceAttribute(string description)
        {
            Description = description ?? throw new ArgumentNullException(nameof(description), 
                "Resource Description cannot be null.");
        }
    }
}

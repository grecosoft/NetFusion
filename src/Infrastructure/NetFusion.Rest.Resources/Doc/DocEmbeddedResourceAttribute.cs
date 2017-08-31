using System;

namespace NetFusion.Rest.Resources.Doc
{
    /// <summary>
    /// Used to specify an embedded resource by name or type returned from
    /// a controller's action method.  Multiple attributes can be applied.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class DocEmbeddedResourceAttribute : Attribute
    {
        public string[] EmbeddedResourceNames { get; }
        public Type[] EmbeddedResourceTypes { get; private set; }

        /// <summary>
        /// The names of the resources.
        /// </summary>
        /// <param name="embeddedResourceNames">The names used to identify the returned embedded resources.</param>
        public DocEmbeddedResourceAttribute(params string[] embeddedResourceNames)
        {            
            EmbeddedResourceNames = embeddedResourceNames ??
                throw new ArgumentNullException(nameof(embeddedResourceNames), "Embedded Resource names not specified.");
        }

        /// <summary>
        /// The types of the resources.
        /// </summary>
        /// <param name="embeddedResourceTypes">The types of the embedded resources.</param>
        public DocEmbeddedResourceAttribute(params Type[] embeddedResourceTypes)
        {
            EmbeddedResourceTypes = embeddedResourceTypes ??
                throw new ArgumentNullException(nameof(embeddedResourceTypes), "Embedded Resource types not specified.");
           
        }
    }
}

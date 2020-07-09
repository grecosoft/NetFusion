using System;
using System.Collections.Generic;

namespace NetFusion.Rest.Docs.Models
{
    /// <summary>
    /// Model describing a resource returned from a Web Api method.
    /// </summary>
    public class ApiResourceDoc
    {
        /// <summary>
        /// Description of the resource.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Identifies the resource's type exposed by the API.
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// Documentation for the resources properties.
        /// </summary>
        public ICollection<ApiPropertyDoc> Properties { get; } = new List<ApiPropertyDoc>();

        /// <summary>
        /// Documentation for child embedded named resources.
        /// </summary>
        public ApiEmbeddedDoc[] EmbeddedResourceDocs { get; set; }

        /// <summary>
        /// Documentation for an link relations associated with the resource.
        /// </summary>
        public ApiRelationDoc[] RelationDocs { get; set; }

        /// <summary>
        /// The resource type for which the documentation is associated.
        /// </summary>
        internal Type ResourceType { get; set; }
    }
}
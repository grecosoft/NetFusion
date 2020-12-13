using System;
using System.Collections.Generic;

namespace NetFusion.Rest.Docs.Models
{
    /// <summary>
    /// Model describing a resource returned from or posted to a Web Api method.
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
        public ICollection<ApiPropertyDoc> Properties { get; set; } = new List<ApiPropertyDoc>();

        /// <summary>
        /// Documentation for child embedded named resources.
        /// </summary>
        public ICollection<ApiEmbeddedDoc> EmbeddedResourceDocs { get; set; } = new List<ApiEmbeddedDoc>();

        /// <summary>
        /// Documentation for an link relations associated with the resource.
        /// </summary>
        public ICollection<ApiRelationDoc> RelationDocs { get; set; } = new List<ApiRelationDoc>();

        /// <summary>
        /// The resource type for which the documentation is associated.
        /// </summary>
        internal Type ResourceType { get; set; }
    }
}
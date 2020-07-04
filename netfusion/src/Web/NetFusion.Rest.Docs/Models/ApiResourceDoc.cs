using System.Collections.Generic;

namespace NetFusion.Rest.Docs.Models
{
    public class ApiResourceDoc
    {
        // Add links descriptions? ...

        /// <summary>
        /// Description of the resource.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Identifies the resource's type exposed by the API.
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// Document for the resources properties.
        /// </summary>
        public ICollection<ApiPropertyDoc> Properties { get; } = new List<ApiPropertyDoc>();

        /// <summary>
        /// When returning a resource, the action can embedded additional resources into the
        /// resource being returned or any of the child embedded resources.  This collection
        /// contains documentation for all the possible embedded resource types.
        /// </summary>
        public ApiEmbeddedDoc[] EmbeddedResources { get; set; }
    }
}
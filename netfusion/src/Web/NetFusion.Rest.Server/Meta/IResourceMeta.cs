using NetFusion.Rest.Server.Actions;
using System;
using System.Collections.Generic;

namespace NetFusion.Rest.Server.Meta
{
    /// <summary>
    /// Common resource metadata associated with a resource of a given type.
    /// </summary>
    public interface IResourceMeta
    {
        /// <summary>
        /// The resource type associated with the metadata.
        /// </summary>
        Type ResourceType { get; }

        /// <summary>
        /// The link metadata associated with the resource.
        /// </summary>
        IReadOnlyCollection<ActionLink> Links { get; }
    }
}

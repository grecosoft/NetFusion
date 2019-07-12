using System;
using System.Collections.Generic;
using NetFusion.Rest.Server.Linking;

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
        /// The link metadata associated with the resource.  Based on how the link is selected or specified,
        /// one of the derived ResourceLink classes will be added to this collection.  If the link is just a
        /// hard-coded string, then an instance of the the base ResourceLink class is added.
        /// </summary>
        IReadOnlyCollection<ResourceLink> Links { get; }
    }
}

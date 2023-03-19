using System;
using System.Collections.Generic;
using NetFusion.Web.Rest.Server.Linking;

namespace NetFusion.Web.Rest.Server.Meta;

/// <summary>
/// Common resource metadata associated with a source type.
/// </summary>
public interface IResourceMeta
{
    /// <summary>
    /// The source type associated with the metadata.
    /// </summary>
    Type SourceType { get; }

    /// <summary>
    /// The link metadata associated with the source type.  Based on how the link is selected or specified,
    /// one of the derived ResourceLink classes will be added to this collection.  If the link is just a
    /// hard-coded string, then an instance of the the base ResourceLink class is added.
    /// </summary>
    IReadOnlyCollection<ResourceLink> Links { get; }
}
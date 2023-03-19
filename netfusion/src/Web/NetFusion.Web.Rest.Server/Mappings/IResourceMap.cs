using System;
using System.Collections.Generic;
using NetFusion.Core.Bootstrap.Plugins;
using NetFusion.Web.Rest.Server.Meta;

namespace NetFusion.Web.Rest.Server.Mappings;

/// <summary>
/// Implementation specifying how REST based attributes are mapped to a resource
/// for a given media type (i.e. HAL).
/// </summary>
public interface IResourceMap : IPluginKnownType
{
    /// <summary>
    /// The media type associated with the mapping.
    /// </summary>
    string MediaType { get; }

    /// <summary>
    /// The provider type that applies the resource metadata to a resource
    /// for the given media-type.
    /// </summary>
    Type ProviderType { get; }

    /// <summary>
    /// The resource metadata configured by the mapping for a set of resource types.
    /// </summary>
    IReadOnlyCollection<IResourceMeta> ResourceMeta { get; }

    /// <summary>
    /// Instructs the resource map to add mappings.  Called by plugin module 
    /// during the bootstrap  process to cache all REST metadata.
    /// </summary>
    void BuildMap();
}
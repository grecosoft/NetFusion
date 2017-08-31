using NetFusion.Base.Plugins;
using NetFusion.Rest.Server.Meta;
using System;
using System.Collections.Generic;

namespace NetFusion.Rest.Server.Mappings
{
    /// <summary>
    /// Implementation specifies how REST based attributes are mapped to a resource.
    /// </summary>
    public interface IResourceMap : IKnownPluginType
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
        /// Instructs the resource map to add mappings.  Called by the plug's module 
        /// during the bootstrap  process to cache all REST metadata.
        /// </summary>
        void BuildMap();
    }
}

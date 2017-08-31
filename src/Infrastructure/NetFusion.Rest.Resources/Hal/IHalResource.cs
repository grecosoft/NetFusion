using Newtonsoft.Json;
using System.Collections.Generic;

namespace NetFusion.Rest.Resources.Hal
{
    /// <summary>
    /// Represents a resource model having an associated set of links
    /// used to navigate to related resources and a set of related
    /// embedded resources.
    /// </summary>
    public interface IHalResource : IResource, ILinkedResource
    {
        /// <summary>
        /// Dictionary of named embedded resources.
        /// </summary>
        [JsonProperty(PropertyName = "_embedded", Order = 1)]
        IDictionary<string, IResource> Embedded { get; set; }

        void Embed(IHalResource embeddedResource, string named = null);

        void Embed<T>(IEnumerable<T> embeddedResources, string named = null)
            where T : class, IHalResource;
    }
}

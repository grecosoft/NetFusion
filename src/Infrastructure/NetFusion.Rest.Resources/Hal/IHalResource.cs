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

        /// <summary>
        /// Embeds resource into the parent resource.
        /// </summary>
        /// <param name="embeddedResource">The resource to embed.</param>
        /// <param name="named">The key name returned to the client used to identify 
        /// embedded resource.</param>
        void Embed(IHalResource embeddedResource, string named = null);

        /// <summary>
        /// Embeds a collection of resources into the parent resource
        /// </summary>
        /// <typeparam name="T">The type of the embedded resource.</typeparam>
        /// <param name="embeddedResources">collection of embedded resources.</param>
        /// <param name="named">The key name returned to the client used to identify
        /// embedded resources.</param>
        void Embed<T>(IEnumerable<T> embeddedResources, string named = null)
            where T : class, IHalResource;
    }
}

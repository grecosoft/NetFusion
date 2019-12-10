using System.Collections.Generic;

namespace NetFusion.Rest.Resources.Hal
{
    /// <summary>
    /// Represents a resource model having an associated set of links
    /// used to navigate to related resources and a set of related
    /// embedded resources.
    /// </summary>
    public interface IHalResource : IResource
    {
        /// <summary>
        /// The links associated with the resource.
        /// </summary>
        IDictionary<string, Link> Links { get; set; }
        
        /// <summary>
        /// Dictionary of named embedded resources.
        /// </summary>
        IDictionary<string, IResource> Embedded { get; set; }
    }
}

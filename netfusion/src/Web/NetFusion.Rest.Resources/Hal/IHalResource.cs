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
        /// The model containing the data associated with the resource.
        /// This is separate from the Links and Embedded information so
        /// the client can easily access the model to send back to server.
        /// </summary>
        object Model { get; }
        
        /// <summary>
        /// The links associated with the resource.
        /// </summary>
        IDictionary<string, Link> Links { get; set; }
        
        /// <summary>
        /// Dictionary of named embedded resources.
        /// </summary>
        IDictionary<string, object> Embedded { get; set; }
    }
}

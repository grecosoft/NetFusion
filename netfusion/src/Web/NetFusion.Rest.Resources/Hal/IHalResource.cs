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
        /// Untyped reference to the resource's model.  Only set
        /// when a server-side resource is created.  Will be null
        /// on the client.
        /// </summary>
        object ModelValue { get; }

        /// <summary>
        /// The links associated with the resource.
        /// </summary>
        IDictionary<string, Link> Links { get; set; }
        
        /// <summary>
        /// Dictionary of named embedded resources.
        /// </summary>
        IDictionary<string, object> Embedded { get; set; }
    }

    /// <summary>
    /// Represents a resource model having an associated set of links
    /// used to navigate to related resources and a set of related
    /// embedded resources.
    /// </summary>
    /// <typeparam name="TModel">The model associated with the resource.</typeparam>
    public interface IHalResource<out TModel> : IHalResource
        where TModel: class
    {
        /// <summary>
        /// The model containing the data associated with the resource.
        /// This is separate from the Links and Embedded information so
        /// the client can easily access the model to send back to server.
        /// </summary>
        TModel Model { get; }
    }
}

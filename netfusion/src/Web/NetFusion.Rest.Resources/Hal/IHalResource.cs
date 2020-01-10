using System.Collections.Generic;

namespace NetFusion.Rest.Resources.Hal
{
    /// <summary>
    /// A HAL based resource adds information to normal API models returned to the client.
    /// The resource contains the model state, links for navigating to related resources,
    /// and/or links to take action on the current resource.  Sets of related resources 
    /// can also be returned by embedding related resources and models into parent resources.
    /// </summary>
    public interface IHalResource : IResource
    {
        /// <summary>
        /// Untyped reference to the resource's model.  Only set when a server-side
        /// resource is created.  Will be null on the client.
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
    /// Represents a resource HAL resource with a typed model.
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

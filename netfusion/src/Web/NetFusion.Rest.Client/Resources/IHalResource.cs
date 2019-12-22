using System.Collections.Generic;

namespace NetFusion.Rest.Client.Resources
{
    public interface IHalResource
    {
        /// <summary>
        /// The links associated with the resource.
        /// </summary>
        IDictionary<string, Link> Links { get; set; }
        
        /// <summary>
        /// Dictionary of named embedded resources.
        /// </summary>
        IDictionary<string, object> Embedded { get; set; }
    }
    
    public interface IHalResource<out TModel> : IHalResource
    {
        /// <summary>
        /// The model containing the data associated with the resource.
        /// This is separate from the Links and Embedded information so
        /// the client can easily access the model to send back to server.
        /// </summary>
        TModel Model { get; }
    }
}
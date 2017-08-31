using NetFusion.Rest.Resources;

namespace NetFusion.Rest.Server.Hal
{
    /// <summary>
    /// Can be injected by service components responsible for assembling
    /// HAL embedded resources.
    /// </summary>
    public interface IHalEmbededResourceContext
    {
        /// <summary>
        /// Determines if the client has requested if a given resource type should be embedded.
        /// </summary>
        /// <typeparam name="TResource">The type of the embedded resource.</typeparam>
        /// <returns>True if requested by client.  If the client didn't specify, True is returned.
        /// False if the client did specify the list of embedded resources but the resource type
        /// is not specified within the list.</returns>
        bool IsResourceRequested<TResource>()
            where TResource : IResource;

        string[] RequestedEmbeddedResources { get; }
    }
}

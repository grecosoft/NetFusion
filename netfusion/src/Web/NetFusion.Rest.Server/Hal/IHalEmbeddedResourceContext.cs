namespace NetFusion.Rest.Server.Hal
{
    /// <summary>
    /// Can be injected by service components responsible for assembling
    /// HAL embedded resources.
    /// </summary>
    public interface IHalEmbeddedResourceContext
    {
        /// <summary>
        /// Determines if the client specified the embedded model's name to be returned.
        /// </summary>
        /// <typeparam name="TModel">The type of the embedded resource model.</typeparam>
        /// <returns>True if requested by client.  If the client didn't specify, True is returned.
        /// False if the client did specify the list of embedded model names but the model's associated
        /// name is not specified within the list.</returns>
        bool IsRequested<TModel>();

        /// <summary>
        /// Determines if the client has specified the embedded model's name to be returned.
        /// </summary>
        /// <param name="modelName">The name associated with a model.</param>
        /// <returns>True if requested by client.  If the client didn't specify, True is returned.
        /// False if the client did specify the list of embedded model names but the model's associated
        /// name is not specified within the list.</returns>
        bool IsRequested(string modelName);

        /// <summary>
        /// List of the embedded resource model names specified by the requesting client.
        /// </summary>
        string[] RequestedEmbeddedModels { get; }
    }
}

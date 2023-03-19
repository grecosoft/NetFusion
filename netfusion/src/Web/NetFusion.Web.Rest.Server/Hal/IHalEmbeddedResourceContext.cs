namespace NetFusion.Web.Rest.Server.Hal;

/// <summary>
/// Can be injected by service components responsible for assembling HAL embedded resources.
/// Contains methods used to determine the embedded resources requested from the client.
/// </summary>
public interface IHalEmbeddedResourceContext
{
    /// <summary>
    /// Determines if the client has specified the embedded named resource to be returned.
    /// </summary>
    /// <param name="embeddedName">The name associated with a embedded resource.</param>
    /// <returns>True if requested by client.  If the client didn't specify, True is returned.
    /// False if the client did specify the list of embedded resource names but the provided 
    /// name is not specified within the list.</returns>
    bool IsRequested(string embeddedName);

    /// <summary>
    /// List of the embedded resource model names specified by the requesting client.
    /// </summary>
    string[] RequestedEmbeddedModels { get; }
}
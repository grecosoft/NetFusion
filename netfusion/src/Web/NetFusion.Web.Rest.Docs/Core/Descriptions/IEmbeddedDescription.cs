using NetFusion.Web.Rest.Docs.Models;

namespace NetFusion.Web.Rest.Docs.Core.Descriptions;

/// <summary>
/// Interface implemented by a class responsible for describing
/// an embedded resource returned by a controller's action method.
/// </summary>
public interface IEmbeddedDescription : IDocDescription
{
    /// <summary>
    /// Adds documentation to an embedded document.
    /// </summary>
    /// <param name="embeddedDoc">The embedded documentation model to describe.</param>
    /// <param name="attribute">The attribute from which the embedded document was created.</param>
    void Describe(ApiEmbeddedDoc embeddedDoc, EmbeddedResourceAttribute attribute);
}
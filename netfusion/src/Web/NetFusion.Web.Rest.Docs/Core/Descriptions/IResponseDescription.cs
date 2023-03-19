using NetFusion.Web.Metadata;
using NetFusion.Web.Rest.Docs.Models;

namespace NetFusion.Web.Rest.Docs.Core.Descriptions;

/// <summary>
/// Interface implemented by a class responsible for describing
/// a controller's action responses.
/// </summary>
public interface IResponseDescription : IDocDescription
{
    /// <summary>
    /// Called to add documentation to the response document model.        
    /// </summary>
    /// <param name="responseDoc">The response document to describe.</param>
    /// <param name="responseMeta">The associated document metadata from which
    /// the document model was created.</param>
    void Describe(ApiResponseDoc responseDoc, ApiResponseMeta responseMeta);
}